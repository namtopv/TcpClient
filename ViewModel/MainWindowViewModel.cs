using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TcpClient.Views;

namespace TcpClient.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [RelayCommand]
        public void TCP()
        {
            TCPView tcpview = new TCPView();
            tcpview.Show();
        }
        [RelayCommand]
        public void UART()
        {

        }
    }
}
