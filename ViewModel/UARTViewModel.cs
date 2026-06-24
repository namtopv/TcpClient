using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace TcpClient.ViewModel
{
    public partial class UARTViewModel : Window
    {
        public UARTViewModel()
        {
            InitializeComponent();
            DataContext = new UARTViewModel();
        }
    }
}
