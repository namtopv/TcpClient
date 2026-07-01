using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using TcpClient.Models;

namespace TcpClient.ViewModel
{
    public partial class UARTViewModel : ObservableObject
    {
        ComPort comport;
        public ObservableCollection<string> Comports { get; }
        public ObservableCollection<int> Size { get; }
        public ObservableCollection<int> Baud { get; }
        string[] ports = SerialPort.GetPortNames();

        [ObservableProperty]
        public string selectedComPort;

        [ObservableProperty]
        public int selectedBaud;

        [ObservableProperty]
        public int selectedSize;

        [RelayCommand]
        private void Connect()
        {
            comport = new ComPort
            {
                PortName = SelectedComPort,
                BaudRate = SelectedBaud,
                Size = SelectedSize
            };
            comport.ConnectCom();
        }

        public UARTViewModel()
        {
            Comports = new ObservableCollection<string>();
            foreach (string port in ports) Comports.Add(port);
            Baud = new ObservableCollection<int> { 4800, 9600, 14400, 19200, 38400, 56000, 57600, 115200 };
            Size = new ObservableCollection<int> { 7, 8 };
        }

    }
}
