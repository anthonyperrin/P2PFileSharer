using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using P2PFileShare.Services;
using System.Net;
using System;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Linq;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.IO;

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
        private bool _isReceiving;
        private string _receive;
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

        public bool IsReceiving
        {
            get { return ServerListener.isReceiving; }
            set
            {
                if (value != _isReceiving)
                {
                    _isReceiving = ServerListener.isReceiving;
                    OnPropertyChanged("IsReceiving");
                }
            }
        }
        public string Receive
        {
            get { return _receive; }
            set
            {
                if (value != _receive)
                {
                    _receive = value;
                    OnPropertyChanged("Receive");
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
            ServerListener = new ServerListener();
            InitializeComponent();
            ServerInfos.Text = $"En écoute sur {ServerListener.IpAddress}:{ServerListener.Port}";
            IpAddress = "192.168.1.136";
            Port = "5656";
            Receive = ServerListener.isReceiving.ToString();
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

        private async void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            ClientSocket = new ClientSocket();
            await ClientSocket.CreateAndConnectAsync(IPAddress.Parse(IpAddress), Int32.Parse(Port), VerifySocketConnectionAvailability);
            ClientSocket.SendFile(File);
            ClientSocket.EndConnection();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (IpAddress != null && Port != null)
            {
                ClientSocket = new ClientSocket();
                await ClientSocket.CreateAndConnectAsync(IPAddress.Parse(IpAddress), Int32.Parse(Port), VerifySocketConnectionAvailability);
            }
        }

        private void VerifySocketConnectionAvailability(bool connected)
        {
            if (connected)
            {
                ConnectionInfo.Text = string.Format("Connecté à : {0}:{1}", IpAddress, Port);
                ErrorMessage.Text = "";
                ConnectionInfo.Visibility = Visibility.Visible;
                FileForm.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Visible;
                LoginButton.Visibility = Visibility.Hidden;
            }
            else
            {
                ErrorMessage.Text = "Le serveur Peer-2-Peer renseigné n'est pas accessible";
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

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Repository))
                {
                    Process.Start(Repository);
                } else
                {
                    Process.Start(ServerListener.getSavePath());
                }
            }
            catch (Win32Exception win32Exception)
            {
                Console.WriteLine(win32Exception.Message);
            }
        }
    }
}
