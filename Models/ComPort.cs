using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Windows;

namespace TcpClient.Models
{
    public class ComPort
    {
        private string _portName;
        private int _baudRate;
        private int _size;
        private SerialPort _serialPort;

        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
            }
        }
        public int BaudRate
        {
            get => _baudRate;
            set
            {
                _baudRate = value;
            }
        }
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
            }
        }
        public void ConnectCom()
        {
            _serialPort = new SerialPort(PortName, BaudRate);

            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = Size;
            _serialPort.Handshake = Handshake.None;

            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            MessageBox.Show("--> Ket noi cong COM thanh cong!");
        }
    }
}
