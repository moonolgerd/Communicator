using System;
using System.Windows;

namespace Communicator.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
