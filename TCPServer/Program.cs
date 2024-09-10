using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TCPServer
{
    [Serializable]
    public class Message
    {
        public string Value { get; private set; }
        public string Sender { get; private set; }
        public string Type { get; private set; }
        public Message(string value, string sender, string type)
        {
            Value = value;
            Sender = sender;
            Type = type;
        }

        public static byte[] SerializeMessage(Message msg)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, msg);
            return stream.ToArray();
        }

        public static Message DeserializeMessage(byte[] serializedMessage) 
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(serializedMessage);
            return (Message)formatter.Deserialize(stream);
        }
    }

    public class TCPServer 
    { 
        private List<Socket> connectedClients;
        private Socket serverSocket;

        public TCPServer() 
        {
            this.connectedClients = new List<Socket>();
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
            this.serverSocket.Listen(100);
            Console.WriteLine($"Server is listening on {this.serverSocket.LocalEndPoint}");
            this.AcceptClients();
        }

        public TCPServer(string address)
        {
            this.connectedClients = new List<Socket>();
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.serverSocket.Bind(new IPEndPoint(IPAddress.Parse(address.Split(':')[0]), int.Parse(address.Split(':')[1])));
            this.serverSocket.Listen(100);
            this.AcceptClients();
        }

        private void AcceptClients() 
        {
            while (true)
            {
                Socket client = this.serverSocket.Accept();
                this.connectedClients.Add(client);
                new Thread(() => this.HandleClient(client)).Start();
            }
        }

        private void HandleClient(Socket client)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int recievedSize = client.Receive(buffer, SocketFlags.None);
                    Array.Resize(ref buffer, recievedSize);
                    Message msg = Message.DeserializeMessage(buffer);
                    Console.WriteLine($"{msg.Sender}: {msg.Value}");
                    if (msg.Value == "exit" && msg.Type == "SERVER_CMD")break;
                    DistributeMessage(msg);
                    Array.Resize(ref buffer, 1024);
                }
                connectedClients.Remove(client);
                throw new SocketException();
            }
            catch (SocketException e)
            {
                Console.WriteLine("The client has disconnected from the server.");
            }

        }

        private void DistributeMessage(Message msg)
        {
            foreach (Socket client in this.connectedClients)
            {
                client.Send(Message.SerializeMessage(msg), SocketFlags.None);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            TCPServer server = new TCPServer();
        }
    }
}
