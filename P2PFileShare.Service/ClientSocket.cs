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

        /**
         * Creates a new socket and try to connect to it.
         * <param name="ip">The IP address to connect</param>
         * <param name="port">The port to connect</param>
         * <param name="callback">The response returned by the async</param>
         */
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

        /**
         * Sends a file via the connected socket
         * <param name="file">The path to the file to send</param>
         */
        public void SendFile(string file)
        {
            if (_socket.Connected && !String.IsNullOrEmpty(file))
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
                fileNameByte.CopyTo(fileNameData, 0); // Copy file name in the prepared buffer.
                Socket.Send(fileNameData); // Sends file name to connected server.
                Socket.SendFile(file); // Then sends file data.
            }
        }

        /**
         * Close socket connection.
         */
        public void EndConnection()
        {
            _socket.Close();
        }

}
}
