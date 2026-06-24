using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows;
using TcpClient.Models;
using TcpClient.Views;
using static TcpClient.Models.NetworkBase;

namespace TcpClient.ViewModel
{
    public partial class TCPViewModel : ObservableObject
    {
        bool? isClient = null;
        bool isConfirm;
        Client client;
        Server server;
        [ObservableProperty]
        private string? txtIPAddress;

        [ObservableProperty]
        private string? txtPort;

        [ObservableProperty]
        private string? txtMessage;

        [ObservableProperty]
        private ObservableCollection<GetFrame> lvMessage = new();

        [RelayCommand]
        private async Task Connect()
        {
            bool isConnect;
            if (isClient == null)
            {
                MessageBox.Show("Chọn Server hoặc Client");
                return;
            }
            else if (isClient == true)
            {
                client = new Client
                {
                    IpAddress = TxtIPAddress,
                    Port = TxtPort
                };
                client.FrameReceived += OnFrameReceived;
                isConnect = await client.ConnectToServer();
            }
            else
            {
                server = new Server
                {
                    IpAddress = TxtIPAddress,
                    Port = TxtPort
                };
                server.FrameReceived += OnFrameReceived;
                isConnect = await server.ConnetToIP();
            }

            if (isConnect == true)
            {
                MessageBox.Show("Kết nối thành công");
            }
            else
            {
                MessageBox.Show("Lỗi kết nối");
            }
        }
        [RelayCommand]
        private void Server()
        {
            isClient = false;
        }
        [RelayCommand]
        private void Client()
        {
            isClient = true;
        }
        [RelayCommand]
        private async Task SEND()
        {
            if (isConfirm == false) return;
            bool isSend;
            if (isClient == true)
            {
                client.TxMessage = TxtMessage;
                isSend = await client.Send();
            }
            else
            {
                server.TxMessage = TxtMessage;
                isSend = await server.Send();
            }
            if (isSend == true) MessageBox.Show("Đã gửi");
            else MessageBox.Show("Lỗi");
        }
        [RelayCommand]
        private async Task REQT()
        {
            bool isSend;
            if (isClient == true)
            {
                client.TxMessage = "CALL";
                isSend = await client.Reqt();
            }
            else
            {
                server.TxMessage = "CALL";
                isSend = await server.Reqt();
            }
            if (isSend == true) MessageBox.Show("Đã gửi");
            else MessageBox.Show("Lỗi");
        }
        [RelayCommand]
        private void HEX()
        {

        }
        private async void OnFrameReceived(GetFrame frame)
        {
            if (frame.Command == CommandType.REQT && frame.Message == "CALL")
            {
                isConfirm = MessageBoxYesNo(frame);
                if (isConfirm == true)
                {
                    Application.Current.Dispatcher.Invoke(() => LvMessage.Add(frame));
                    if (isClient == true) await client.Resp("OK");
                    else await server.Resp("OK");
                }
                else
                {
                    if (isClient == true) await client.Resp("NO");
                    else await server.Resp("NO");
                }
            }
            else if(frame.Command == CommandType.RESP)
            {
                if (frame.Message == "OK") isConfirm = true;
                else isConfirm = false;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => LvMessage.Add(frame));
            }
        }
        public string GetLocalIPv4Address()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            IPAddress? ipv4Address = addresses.FirstOrDefault(ip =>
                ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ip)
            );
            return ipv4Address?.ToString() ?? "127.0.0.1";
        }
        public TCPViewModel()
        {
            txtIPAddress = GetLocalIPv4Address();
            txtPort = "8888";
        }
        public bool MessageBoxYesNo(GetFrame getframe)
        {
            MessageBoxResult result = MessageBox.Show(
                $"Command: {getframe.Command}\nMessage: {getframe.Message}",
                "Connection Request",
                MessageBoxButton.YesNo
            );
            if (result == MessageBoxResult.Yes) return true;
            else return false;
        }
    }
}