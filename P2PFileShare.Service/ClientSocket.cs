using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PFileShare.Services
{
    public class ClientSocket
    {
        private Socket _socket;

        public Socket Socket
        {
            get
            {
                return _socket;
            }
            set
            {
                if (value != _socket)
                {
                    _socket = value;
                }
            }
        }
        public ClientSocket(IPAddress ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(ip, port);
            } catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void DisconnectSocket()
        {
            if (Socket.Connected)
            {
                Socket.Disconnect(true);
            }
        }

        public void SendFile(string file)
        {
            if (_socket.Connected)
            {
                string filePath = "";
                string fileName = file.Replace("\\", "/");

                while (fileName.IndexOf("/") > -1)
                {
                    filePath += fileName.Substring(0, fileName.IndexOf("/") + 1);
                    fileName = fileName.Substring(fileName.IndexOf("/") + 1);
                }

                byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                byte[] fileData = File.ReadAllBytes(filePath + fileName);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                _socket.SendFile(file); 
            }
        }

        public void EndConnection()
        {
            _socket.Close();
        }

}
}
