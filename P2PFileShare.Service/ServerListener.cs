using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PFileShare.Services
{
    public class ServerListener
    {
        #region Variables
        TcpListener server = null;
        private IPAddress _ipAddress;
        private int _port;
        private const int DATA_BUFFER_SIZE = 2048;
        private const int FILE_NAME_BUFFER_SIZE = 64;
        private string _rootPath;
        private string _repository;
        #endregion

        #region Propriétés
        public string RootPath
        {
            get { return _rootPath; }
            set
            {
                if (value != _rootPath)
                {
                    _rootPath = value;
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
                }
            }
        }

        public IPAddress IpAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                }
            }
        }
        public int Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                }
            }
        }
        #endregion

        public ServerListener()
        {
            RootPath = @"C:\Temp\";
            Repository = @"Efforceurs";
            CheckDefaultSaveDir();
            Listen();
        }

        public string getSavePath()
        {
            return Path.Combine(RootPath, Repository);
        }

        public async Task Listen()
        {
            IpAddress= GetIPAddress();
            Port = 5656;
            server = new TcpListener(IpAddress, Port);

            server.Start();

            while (true)
            {
                TcpClient handlerSocket = await server.AcceptTcpClientAsync();
                if (handlerSocket.Connected)
                {
                    Handler(handlerSocket);
                }
            }
        }

        public async Task Handler(TcpClient handlerSocket)
        {
            NetworkStream Nw = new NetworkStream(handlerSocket.Client);
            if (Nw.DataAvailable)
            {
                int thisRead = 0;
                Byte[] dataBytes = new Byte[DATA_BUFFER_SIZE];
                Byte[] fileNameBytes = new Byte[FILE_NAME_BUFFER_SIZE];

                await Nw.ReadAsync(fileNameBytes, 0, FILE_NAME_BUFFER_SIZE);
                
                Stream writingStream = File.OpenWrite(Path.Combine(getSavePath(), getFileName(fileNameBytes)));
                while (true)
                {
                    thisRead = await Nw.ReadAsync(dataBytes, 0, DATA_BUFFER_SIZE);
                    writingStream.Write(dataBytes, 0, thisRead);
                    if (thisRead == 0)
                        break;
                }
                writingStream.Close();
                handlerSocket = null;
            }
        }

        private void CheckDefaultSaveDir()
        {
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
            if (!Directory.Exists(getSavePath()))
            {
                Directory.CreateDirectory(getSavePath());
            }
        }

        private IPAddress GetIPAddress()
        {
            IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
                return IPAddress.Parse(localIP);
            }
        }

        private string getFileName(byte[] fileNameBytes)
        {
            int i = fileNameBytes.Length - 1;
            while (fileNameBytes[i] == 0)
                --i;

            byte[] filteredFileNameBytes = new byte[i + 1];
            Array.Copy(fileNameBytes, filteredFileNameBytes, i + 1);

            return Encoding.UTF8.GetString(filteredFileNameBytes);
        }
    }
}
