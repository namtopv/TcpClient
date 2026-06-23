using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace TcpClient.Models
{
    internal class Server : ObservableObject
    {
        private string? _ipaddress;
        private string? _port;
        private string? _txmessage;
        private string? _rxmessage;
        private System.Net.Sockets.TcpListener? _tcpserver;
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
        public async Task<bool> ConnetToIP()
        {
            if (IpAddress == null || int.TryParse(Port, out int portNumber) == false) return false;
            _tcpserver = new System.Net.Sockets.TcpListener(IPAddress.Parse(IpAddress), portNumber);
            _tcpserver.Start();
            try
            {
                _tcpClient = await _tcpserver.AcceptTcpClientAsync();
                return true;
            }
            catch
            {
                return false;
            }
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
