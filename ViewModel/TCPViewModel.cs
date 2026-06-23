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
        Client client;
        [ObservableProperty]
        private string? txtIPAddress;

        [ObservableProperty]
        private string? txtPort;

        [ObservableProperty]
        private string? txtMessage;

        [ObservableProperty]
        private string? lvMessage;

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
        [RelayCommand] 
        private async Task SEND() 
        {
            client.TxMessage = TxtMessage;
            if (await client.Send() == true) MessageBox.Show("Đã gửi");
            else MessageBox.Show("Lỗi");
        }
        [RelayCommand] 
        private async Task REQT() 
        {
            client.TxMessage = "CALL";
            if (await client.Reqt() == true) MessageBox.Show("Đã gửi");
            else MessageBox.Show("Lỗi");
        }
        [RelayCommand] 
        private void HEX() 
        {

        }
    }
}
