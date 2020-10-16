using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PFileShare.Services
{
    public class ClientSocket
    {
        private Socket _socket;
        private const int FILE_NAME_BUFFER_SIZE = 64;

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
            CreateAndConnectAsync(ip, port);
        }

        public async void CreateAndConnectAsync(IPAddress ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await _socket.ConnectAsync(ip, port);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
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

                Byte[] fileNameData = new Byte[FILE_NAME_BUFFER_SIZE];
                byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                fileNameByte.CopyTo(fileNameData, 0);
                Socket.Send(fileNameData);
                Socket.SendFile(file);
            }
        }

        public void EndConnection()
        {
            _socket.Close();
        }

}
}
