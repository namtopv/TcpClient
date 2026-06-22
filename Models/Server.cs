using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace TcpClient.Models
{
    internal class Server : NetworkBase
    {
        private string? _ipaddress;
        private string? _port;
        private System.Net.Sockets.TcpListener? _tcpserver;
        private System.Net.Sockets.TcpClient? client;

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
        public async Task<bool> ConnetToIP()
        {
            if (IpAddress == null || int.TryParse(Port, out int portNumber) == false) return false;
            _tcpserver = new System.Net.Sockets.TcpListener(IPAddress.Parse(IpAddress), portNumber);
            _tcpserver.Start();
            try
            {
                client = await _tcpserver.AcceptTcpClientAsync();
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
            client?.Close();
            RaiseConnectionLost();
        }
    }
}
