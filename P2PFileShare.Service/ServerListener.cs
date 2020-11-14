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
        TcpClient handlerSocket;
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
            _ = Listen();
        }

        public string getSavePath()
        {
            return Path.Combine(RootPath, Repository);
        }

        /**
         * Listens connections on the given IP and Port.
         */
        public async Task Listen()
        {
            IpAddress= GetIPAddress();
            Port = 5656;
            server = new TcpListener(IpAddress, Port);

            server.Start();

            while (true)
            {
                handlerSocket = await server.AcceptTcpClientAsync(); // Blocks code execution whether client hasn't connected yet.
                _ = Handler();
            }
        }

        /**
         * Handles a client to connection to read the networkstream.
         * Writes the networkstream into a file.
         */
        public async Task Handler()
        {
            while (true) {
                if (!handlerSocket.Connected)
                    break;
                NetworkStream Nw = handlerSocket.GetStream();
                Byte[] dataBytes = new Byte[DATA_BUFFER_SIZE];
                Byte[] fileNameBytes = new Byte[FILE_NAME_BUFFER_SIZE];
                await Nw.ReadAsync(fileNameBytes, 0, FILE_NAME_BUFFER_SIZE); // Reads the file name bytes.
                Stream writingStream = File.OpenWrite(Path.Combine(getSavePath(), getFileName(fileNameBytes))); // Opens stream in save path.
                while (true) {
                    int size = await Nw.ReadAsync(dataBytes, 0, DATA_BUFFER_SIZE); // Reads file until read size reaches 0.
                    if (!(size > 0)) {
                        writingStream.Close();
                        handlerSocket.Close();
                        break;
                    }
                    writingStream.Write(dataBytes, 0, size); // Writes data read into the stream.
                }
            }
        }

        /**
         *  Whether default save directory doesn't exist, creates it.
         */
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
        
        /**
         * Forces a connection to get the local network card IP
         * which is routed to the gateway.
         */
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

        /**
         * Gets the file name in the corresponding buffer.
         * <param name="fileNameBytes">The file name bytes part</param>
         */
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
