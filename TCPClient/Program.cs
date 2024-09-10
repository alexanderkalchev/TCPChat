using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TCPClient
{

    public class TCPClient
    {
        private string name;
        private Socket clientSocket;
        private Thread RecvMsgThread;

        public TCPClient(string name, string address)
        {
            this.name = name;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ConnectSocket(address);
            RecvMsgThread = new Thread(this.RecvMessages);
            RecvMsgThread.Start();
        }

        private void ConnectSocket(string address)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(address.Split(':')[0]), int.Parse(address.Split(':')[1]));
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("The client has connected to the server.");
        }

        public void DisconnectSocket() 
        {
            //STOP RECEIVEING AND SENDING AFTER ALL PENDING DATA IS RCVD/SENT
            clientSocket.Shutdown(SocketShutdown.Both);
            //DESTROY THE SOCKET
            clientSocket.Close();
            RecvMsgThread.Abort();
        }

        public void SendMessage(string msg) 
        {
            clientSocket.Send(Encoding.ASCII.GetBytes($"[{this.name}] {msg}"), SocketFlags.None);
        }

        private void RecvMessages()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int recievedSize = clientSocket.Receive(buffer, SocketFlags.None);
                Array.Resize(ref buffer, recievedSize);
                string res = Encoding.ASCII.GetString(buffer);
                Console.WriteLine(res);
                Array.Resize(ref buffer, 1024);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter name: ");
                string name = Console.ReadLine();
                TCPClient client = new TCPClient(name, "18.158.249.75:11405");
                string input;
                while (true)
                {
                    input = Console.ReadLine();
                    client.SendMessage(input);
                    if (input == "exit") break;
                }
                client.DisconnectSocket();
            }
            catch (SocketException e) 
            {
                Console.WriteLine("Client has disconnected from the server.");
            }
        }
    }
}
