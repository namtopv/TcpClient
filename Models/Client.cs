using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows;

namespace TcpClient.Models
{
    public class Client : ObservableObject
    {
        private string? _ipaddress;
        private string? _port;
        private string? _txmessage;
        private string? _rxmessage;
        private System.Net.Sockets.TcpClient? _tcpClient;
        private System.Net.Sockets.NetworkStream? _stream;
        private NetworkBase? _network;
        private System.Threading.CancellationTokenSource? _cts;

        // Phát mỗi khi nhận được một khung hợp lệ từ server
        public event Action<NetworkBase.GetFrame>? FrameReceived;
        public string? IpAddress
        {
            get => _ipaddress;
            set
            {
                _ipaddress = value;
            }
        }
        public string? Port
        {
            get => _port;
            set
            {
                _port = value;
            }
        }
        public string? TxMessage
        {
            get => _txmessage;
            set
            {
                _txmessage = value;
            }
        }
        public string? RxMessage
        {
            get => _rxmessage;
            set
            {
                _rxmessage = value;
            }
        }
        public async Task<bool> ConnectToServer()
        {
            if (IpAddress == null || int.TryParse(Port, out int portNumber) == false) return false;
            _tcpClient = new System.Net.Sockets.TcpClient();
            try
            {
                await _tcpClient.ConnectAsync(IpAddress, portNumber);
                StartReceiving();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void StartReceiving()
        {
            if (_tcpClient == null) return;
            _stream = _tcpClient.GetStream();
            _network = new NetworkBase();
            _cts = new System.Threading.CancellationTokenSource();
            _ = _network.ReceiveLoopAsync(_stream, frame => FrameReceived?.Invoke(frame), _cts.Token);
        }
        public void Disconnect()
        {
            _cts?.Cancel();
            _tcpClient?.Close();
        }
        public async Task<bool> Send()
        {
            NetworkBase networkbase = new NetworkBase();
            if (await networkbase.Write(TcpClient.Models.NetworkBase.CommandType.SEND, TxMessage, _tcpClient) == true) return true;
            else return false;
        }
        public async Task<bool> Reqt()
        {
            NetworkBase networkbase = new NetworkBase();
            if (await networkbase.Write(TcpClient.Models.NetworkBase.CommandType.REQT, TxMessage, _tcpClient) == true) return true;
            else return false;
        }
    }
}