using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPServer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



namespace TCPGuiClient
{
    public partial class Form1 : Form
    {
        private TCPClient client;
        public Form1()
        {
            InitializeComponent();
        }

        private void SendMsg_Click(object sender, EventArgs e)
        {
            client.SendMessage(InputBox.Text, "CLIENT_MSG");
            InputBox.Text = "";
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            client.SendMessage("exit", "SERVER_CMD");
            client.DisconnectSocket();
            DisconnectButton.Enabled = false;
            ConnectBtn.Enabled = true;
            UsernameTxtBox.Enabled = true;
            ServerTxtBox.Enabled = true;
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            string username = UsernameTxtBox.Text.Trim();
            string serverAddress = ServerTxtBox.Text.Trim();
            if (username != "" && serverAddress != "")
            {
                client = new TCPClient(username, serverAddress, MsgBox);
                DisconnectButton.Enabled = true;
                ConnectBtn.Enabled = false;
                UsernameTxtBox.Enabled = false;
                ServerTxtBox.Enabled = false;
            }
            else
            {
                MessageBox.Show("Username or server blank!");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show(client.clientSocket.Connected.ToString());
            e.Cancel = true;
        }
    }

    public class TCPClient
    {
        private string name;
        public Socket clientSocket { get; private set; }
        private Thread RecvMsgThread;

        public TCPClient(string name, string address, RichTextBox MsgBox)
        {
            this.name = name;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ConnectSocket(address);
            RecvMsgThread = new Thread(() => this.RecvMessages(MsgBox));
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

        public void SendMessage(string text, string type)
        {
            TCPServer.Message msg = new TCPServer.Message(text, this.name, type);
            clientSocket.Send(TCPServer.Message.SerializeMessage(msg), SocketFlags.None);
        }

        private void RecvMessages(RichTextBox MsgBox)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int recievedSize = clientSocket.Receive(buffer, SocketFlags.None);
                Array.Resize(ref buffer, recievedSize);
                TCPServer.Message msg = TCPServer.Message.DeserializeMessage(buffer);
                MsgBox.Text += $"[{msg.Sender}]: {msg.Value}\n";
                Array.Resize(ref buffer, 1024);
            }
        }
    }
}
