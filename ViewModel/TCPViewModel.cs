using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TcpClient.Models;
using TcpClient.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Security.Cryptography.Pkcs;

namespace TcpClient.ViewModel
{
    public partial class TCPViewModel : ObservableObject
    {
        bool? isClient = null;
        [ObservableProperty]
        private string? txtIPAddress;

        [ObservableProperty]
        private string? txtPort;

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
                isConnect = await client.ConnectToServer();
            }
            else
            {
                var server = new Server
                {
                    IpAddress = TxtIPAddress,
                    Port = TxtPort
                };
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
    }
}
