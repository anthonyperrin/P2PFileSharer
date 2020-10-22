using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PFileShare.Services
{
    public class ClientSocket
    {
        #region Variables
        private Socket _socket;
        private const int FILE_NAME_BUFFER_SIZE = 64;
        #endregion

        #region Propriétés
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
        #endregion

        public async Task CreateAndConnectAsync(IPAddress ip, int port, Action<bool> callback)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await _socket.ConnectAsync(ip, port);
                callback(true);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                callback(false);
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
