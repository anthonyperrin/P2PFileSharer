using System;
using System.Collections.Generic;
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
        private const int BUFFER_SIZE = 2048;
        private const string ROOT_PATH = @"C:\Temp\";
        private const string EFFORCEUR_PATH = @"C:\Temp\Efforceurs";
        TcpClient handlerSocket;

        public ServerListener()
        {
            CheckDefaultSaveDir();
            Listen();
        }

        public void Listen()
        {
            int port = 5656;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);
            server.Start();
            TcpClient handlerSocket = server.AcceptTcpClient();
            if (handlerSocket.Connected)
            {
                Thread thdhandler = new Thread(new ThreadStart(HandlerThread));
                thdhandler.Start();
            }
        }

        private void HandlerThread()
        {
            NetworkStream Nw = new NetworkStream(handlerSocket.Client);
            int thisRead = 0;
            Byte[] dataByte = new Byte[BUFFER_SIZE];
            
            Stream strm = File.OpenWrite(EFFORCEUR_PATH);
            while (true)
            {
                thisRead = Nw.Read(dataByte, 0, BUFFER_SIZE);
                strm.Write(dataByte, 0, thisRead);
                if (thisRead == 0)
                    break;
            }
            strm.Close();
            handlerSocket = null;
            
        }

        private void CheckDefaultSaveDir()
        {
            if (!Directory.Exists(ROOT_PATH))
            {
                Directory.CreateDirectory(ROOT_PATH);
            }
            if (!Directory.Exists(EFFORCEUR_PATH))
            {
                Directory.CreateDirectory(EFFORCEUR_PATH);
            }
         }
    }
}
