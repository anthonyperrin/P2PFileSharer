using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using P2PFileShare.Services;
using System.Net;
using System;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Linq;

namespace P2PFileShare.Application
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Variables

        private string _ipAddress;
        private string _port;
        private string _file;
        private string _repository;
        private ServerListener _serverListener;
        private ClientSocket _clientSocket;

        #endregion

        #region Propriétés

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                    OnPropertyChanged("IpAddress");
                }
            }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        public string File
        {
            get { return _file; }
            set
            {
                if (value != _file)
                {
                    _file = value;
                    OnPropertyChanged("File");
                }
            }
        }

        public string Repository
        {
            get { return _repository; }
            set
            {
                if (value != _repository)
                {
                    _repository = value;
                    OnPropertyChanged("Repository");
                }
            }
        }

        public ServerListener ServerListener
        {
            get { return _serverListener; }
            set
            {
                if (value != _serverListener)
                {
                    _serverListener = value;
                }
            }
        }

        public ClientSocket ClientSocket
        {
            get { return _clientSocket; }
            set
            {
                if (value != _clientSocket)
                {
                    _clientSocket = value;
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            ServerListener = new ServerListener();
            ServerInfos.Text = $"En écoute sur {ServerListener.IpAddress}:{ServerListener.Port}";
            IpAddress = "192.168.1.136";
            Port = "5656";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                File = openFileDialog.FileName;
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            ClientSocket = new ClientSocket(IPAddress.Parse(IpAddress), Int32.Parse(Port));
            if (ClientSocket.Socket.Connected)
            {
                ClientSocket.SendFile(File);
                ClientSocket.EndConnection();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (IpAddress != null && Port != null)
            {
                ClientSocket = new ClientSocket(IPAddress.Parse(IpAddress), Int32.Parse(Port));
                if (ClientSocket.Socket.Connected)
                {
                    ConnectionInfo.Text = string.Format("Connecté à : {0}:{1}", IpAddress, Port);
                    ConnectionInfo.Visibility = Visibility.Visible;
                    FileForm.Visibility = Visibility.Visible;
                    LogoutButton.Visibility = Visibility.Visible;
                    LoginButton.Visibility = Visibility.Hidden;
                    ClientSocket.EndConnection();
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ClientSocket.EndConnection();
            if (!ClientSocket.Socket.Connected)
            {
                FileForm.Visibility = Visibility.Hidden;
                ConnectionInfo.Visibility = Visibility.Hidden;
                LogoutButton.Visibility = Visibility.Hidden;
                LoginButton.Visibility = Visibility.Visible;
            }
        }

        private void btnChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                Repository = folderBrowserDialog.SelectedPath;

            if (!string.IsNullOrEmpty(Repository))
            {
                ServerListener.Repository = string.Empty;
                ServerListener.RootPath = Repository;
            }
        }
    }
}
