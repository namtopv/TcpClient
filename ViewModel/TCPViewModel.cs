using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        [ObservableProperty]
        private string? txtIPAddress;

        [ObservableProperty]
        private string? txtPort;
        public ObservableCollection<GetFrame> RxMessage { get; set; } = new();

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
                var client = new Client
                {
                    IpAddress = TxtIPAddress,
                    Port = TxtPort
                };
                client.FrameReceived += OnFrameReceived;
                client.ConnectionLost += () => MessageBox.Show("Client: Mất kết nối tới Server!");
                isConnect = await client.ConnectToServer();
            }
            else
            {
                var server = new Server
                {
                    IpAddress = TxtIPAddress,
                    Port = TxtPort
                };
                server.FrameReceived += OnFrameReceived;
                server.OnDisconnect += () => MessageBox.Show("Server: Client đã ngắt kết nối!");
                isConnect = await server.ConnetToIP();
            }

            if(isConnect == true)
            {
                MessageBox.Show("Kết nối thành công");
            }
            else
            {
                MessageBox.Show("Lỗi kết nối");
            }
        }
        private void OnFrameReceived(GetFrame frame)
        {
            // Vì luồng đọc mạng chạy ngầm (Background Thread), ta cần dùng Dispatcher 
            // để đẩy việc chèn dữ liệu giao diện về luồng chính (UI Thread) an toàn.
            Application.Current.Dispatcher.Invoke(() =>
            {
                RxMessage.Insert(0, frame); // Thêm gói tin mới nhất lên đầu danh sách hiển thị
            });
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
        [RelayCommand] private void SEND() { }
        [RelayCommand] private void REQT() { }
        [RelayCommand] private void HEX() { }
    }
}
