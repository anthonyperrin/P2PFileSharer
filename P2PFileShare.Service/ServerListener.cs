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
    public class ServerListener
    {
        private Socket clientSocket;
        private const int BUFFER_SIZE = 2048;
        private const string ROOT_PATH = @"C:\Temp\";
        private const string EFFORCEUR_PATH = @"C:\Temp\Efforceurs";
        private IPEndPoint EndPoint;

        public ServerListener()
        {
            CheckDefaultSaveDir();
            StartListening();
        }

        public void StartListening()
        {
            EndPoint = new IPEndPoint(IPAddress.Any, 5656);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(EndPoint);
            clientSocket.Listen(1);
        }

        public void ReceiveFile()
        {
            try
            {
                byte[] clientData = new byte[1024 * 5000];

                clientSocket.BeginReceive(clientData, 0, clientData.Length, 0, new AsyncCallback(ReceiveCallback), clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {

                byte[] clientData = new byte[1024 * 5000];
 
                Socket client = (Socket)ar.AsyncState;
                int bytesRead = client.EndReceive(ar);

                int fileNameLen = BitConverter.ToInt32(clientData, 0);

                string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);
                BinaryWriter bWrite = new BinaryWriter(File.Open(EFFORCEUR_PATH + "/" + fileName, FileMode.Append)); ;
                bWrite.Write(clientData, 4 + fileNameLen, bytesRead - 4 - fileNameLen);

                bWrite.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
