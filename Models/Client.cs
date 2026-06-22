using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows;

namespace TcpClient.Models
{
    public class Client : NetworkBase
    {
        private string? _ipaddress;
        private string? _port;
        private System.Net.Sockets.TcpClient? _tcpClient;
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
        public async Task<bool> ConnectToServer()
        {
            if (IpAddress == null || int.TryParse(Port, out int portNumber) == false) return false;
            _tcpClient = new System.Net.Sockets.TcpClient();
            try
            {
                await _tcpClient.ConnectAsync(IpAddress, portNumber);
                return true;
            }
            catch
            {
                return false;
            }
        }
        protected override void OnDisconnect()
        {
            _isListening = false;
            _tcpClient?.Close();
            RaiseConnectionLost();
        }
    }
}