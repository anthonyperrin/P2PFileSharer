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
        TcpListener server = null;
        private const int DATA_BUFFER_SIZE = 2048;
        private const int FILE_NAME_BUFFER_SIZE = 64;
        private const string ROOT_PATH = @"C:\Temp\";
        private const string EFFORCEUR_DIR = @"Efforceurs";

        public ServerListener()
        {
            CheckDefaultSaveDir();
            Listen();
        }

        public string getSavePath()
        {
            return Path.Combine(ROOT_PATH, EFFORCEUR_DIR);
        }

        public async Task Listen()
        {
            int port = 5656;
            IPAddress localAddr = GetIPAddress();
            Console.WriteLine(localAddr);
            server = new TcpListener(localAddr, port);

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
            if (!Directory.Exists(ROOT_PATH))
            {
                Directory.CreateDirectory(ROOT_PATH);
            }
            if (!Directory.Exists(getSavePath()))
            {
                Directory.CreateDirectory(Path.Combine(ROOT_PATH, EFFORCEUR_DIR));
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
