using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace TcpClient.Models
{
    public class NetworkBase : ObservableObject
    {
        public const int stxByteCount = 1;
        public const int lengthByteCount = 4;
        public const int commandByteCount = 4;
        public const int checksumByteCount = 2;
        public const int etxByteCount = 1;
        public byte[] length = new byte[4];
        public byte[] command = new byte[4];
        public byte[] checksum = new byte[2];
        public byte[]? messageBytes;
        public const byte stx = 0x02;
        public const byte etx = 0x03;
        public NetworkStream? _stream;
        public bool _isListening;
        public enum CommandType
        {
            REQT = 0x52455154,
            RESP = 0x52455350,
            SEND = 0x53454E44
        }
        public class GetFrame
        {
            public DateTime Date { get; set; }
            public int Length { get; set; }
            public string? Message { get; set; }
            public CommandType Command { get; set; }
        }
        public GetFrame? ParseFrame(byte[] packet)
        {
            GetFrame frame = new GetFrame();
            ushort lowmessage;

            if (packet.Length < 12 || packet == null) return null;
            if (packet[0] != stx || packet[packet.Length - 1] != etx) return null;

            byte[] length = new byte[4];
            Buffer.BlockCopy(packet, 1, length, 0, 4);
            Array.Reverse(length);
            int uLength = BitConverter.ToInt32(length);
            frame.Length = uLength;

            ushort lowlength = (ushort)((packet[3] << 8) | (packet[4]));
            if (uLength == 1) lowmessage = (ushort)(packet[packet.Length - 4]);
            else lowmessage = (ushort)((packet[packet.Length - 5] << 8) | packet[packet.Length - 4]);
            if (lowlength + lowmessage != (ushort)((packet[packet.Length - 3] << 8) | packet[packet.Length - 2])) return null;

            byte[] lengBytes = new byte[4];
            Buffer.BlockCopy(packet, 1, lengBytes, 0, 4);
            Array.Reverse(lengBytes);
            int uNumber = BitConverter.ToInt32(lengBytes);
            string data = Encoding.UTF8.GetString(packet, 9, uNumber);
            frame.Message = data;

            byte[] commannd = new byte[4];
            Buffer.BlockCopy(packet, 5, commannd, 0, 4);
            Array.Reverse(commannd);
            int numbercmd = BitConverter.ToInt32(commannd);
            if (Enum.IsDefined(typeof(CommandType), numbercmd) == false)
            {
                return null;
            }
            CommandType commandtype = (CommandType)numbercmd;
            frame.Command = commandtype;

            frame.Date = DateTime.Now;
            return frame;
        }
        public byte[] BuildFrame(CommandType cmd, string? message)
        {
            int length_message = 0;
            length_message = message.Length;
            messageBytes = Encoding.ASCII.GetBytes(message);
            BinaryPrimitives.WriteInt32BigEndian(length, length_message);
            BinaryPrimitives.WriteInt32BigEndian(command, (int)cmd);
            uint lowLength = (uint)((length[2] << 8) | (length[3]));
            uint lowMessage = 0;
            if (messageBytes.Length >= 2)
            {
                lowMessage = (uint)((messageBytes[messageBytes.Length - 2] << 8) | messageBytes[messageBytes.Length - 1]);
            }
            else if (messageBytes.Length == 1)
            {
                lowMessage = messageBytes[0];
            }

            ushort checksumValue = (ushort)(lowLength + lowMessage);
            BinaryPrimitives.WriteUInt16BigEndian(checksum, checksumValue);
            byte[] finalPacket = new byte[stxByteCount + lengthByteCount + commandByteCount + checksumByteCount + etxByteCount + messageBytes.Length];
            int index = 0;
            finalPacket[index++] = stx;
            Buffer.BlockCopy(length, 0, finalPacket, index, 4);
            index += 4;
            Buffer.BlockCopy(command, 0, finalPacket, index, 4);
            index += 4;
            Buffer.BlockCopy(messageBytes, 0, finalPacket, index, messageBytes.Length);
            index += messageBytes.Length;
            Buffer.BlockCopy(checksum, 0, finalPacket, index, 2);
            index += 2;
            finalPacket[index] = etx;
            return finalPacket;
        }
        public async Task<bool> Write(CommandType commandtype, string? message, System.Net.Sockets.TcpClient client)
        {
            _stream = client.GetStream();
            byte[] packet = BuildFrame(commandtype, message);
            if (packet != null && _stream != null)
            {
                await _stream.WriteAsync(packet, 0, packet.Length);
                return true;
            }
            return false;
        }
        // Vòng lặp đọc liên tục từ stream, tự tách từng khung hoàn chỉnh rồi parse.
        // Khung: STX(1) | length(4 BE) | command(4 BE) | message(N) | checksum(2) | ETX(1) = 12 + N byte.
        public async Task ReceiveLoopAsync(NetworkStream stream, Action<GetFrame> onFrame, CancellationToken token)
        {
            var buffer = new List<byte>();
            var readBuffer = new byte[4096];
            const int maxFrameSize = 1_000_000; // chặn cấp phát quá lớn khi stream mất đồng bộ

            try
            {
                while (token.IsCancellationRequested == false)
                {
                    int bytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, token);
                    if (bytesRead == 0) break; // đầu kia đóng kết nối

                    for (int i = 0; i < bytesRead; i++) buffer.Add(readBuffer[i]);

                    // Tách tất cả khung hoàn chỉnh hiện có trong buffer
                    while (true)
                    {
                        int stxIndex = buffer.IndexOf(stx);
                        if (stxIndex < 0) { buffer.Clear(); break; }        // không có STX -> bỏ rác
                        if (stxIndex > 0) buffer.RemoveRange(0, stxIndex);  // bỏ rác trước STX

                        if (buffer.Count < 9) break; // chưa đủ STX + length(4) + command(4)

                        int messageLength = (buffer[1] << 24) | (buffer[2] << 16)
                                          | (buffer[3] << 8) | buffer[4];
                        if (messageLength < 0 || messageLength > maxFrameSize)
                        {
                            buffer.RemoveAt(0); // length lỗi -> bỏ 1 byte, dò STX kế tiếp
                            continue;
                        }

                        int totalLength = stxByteCount + lengthByteCount + commandByteCount
                                        + messageLength + checksumByteCount + etxByteCount; // 12 + N
                        if (buffer.Count < totalLength) break; // chờ nhận thêm

                        byte[] packet = buffer.GetRange(0, totalLength).ToArray();
                        buffer.RemoveRange(0, totalLength);

                        GetFrame? frame = ParseFrame(packet);
                        if (frame != null) onFrame(frame);
                    }
                }
            }
            catch (OperationCanceledException) { }   // bị hủy chủ động
            catch (IOException) { }                   // stream đóng đột ngột
            catch (ObjectDisposedException) { }       // stream đã dispose
        }
    }
}