using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        IPAddress IP;
        int port = 1337;

        TcpClient client = null;
        TcpListener listener = null;

        Thread thClient = null;
        Thread thListener = null;

        NetworkStream dataStream = null;

        string messageToSend;

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateString()
        {
            client = new TcpClient();
            thClient = new Thread(new ParameterizedThreadStart(SendPacket));
            thClient.IsBackground = true;
            thClient.Start(client);
        }

        private void WriteToChatBox(string message)
        {
            if (textBox4.InvokeRequired)
            {
                textBox4.Invoke((Action)(() => WriteToChatBox(message)));
            }
            else
            {
                textBox4.Text += message;
                textBox4.Text += "\r\n";
            }
        }

        // Nit za client
        private void SendPacket(object pClient)
        {
            try
            {
                client = (TcpClient)pClient;
                client.Connect(IP, port);

                dataStream = client.GetStream();
                byte[] strMessage = Encoding.UTF8.GetBytes(messageToSend);
                dataStream.Write(strMessage, 0, strMessage.Length);
                dataStream.Close();
                client.Close();
            }
            catch (Exception izjema)
            {
                Console.WriteLine("Napaka: pošiljanje neuspešno!");
            }
        }

        // Nit za listener
        private void ListenForBroadcasts()
        {
            listener = new TcpListener(IP, 1234);
            listener.Start();

            while (true)
            {
                try
                {
                    client = listener.AcceptTcpClient();
                    dataStream = client.GetStream();
                    byte[] message = new byte[1024];
                    dataStream.Read(message, 0, message.Length);
                    dataStream.Close();

                    string strMessage = Encoding.UTF8.GetString(message);
                    strMessage = strMessage.Replace("\0", string.Empty);
                    strMessage.Trim();
                    WriteToChatBox(strMessage);
                }
                catch (Exception izjema)
                {
                    Console.WriteLine("Napaka pri prejemanju broadcasta!");
                    thListener.Join();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Connect button
        private void button1_Click(object sender, EventArgs e)
        {
            // Branje iz textboxov
            string IPString = textBox1.Text;
            string portString = textBox2.Text;
            string username = textBox3.Text;

            // Kodiranje v simaProtocol
            messageToSend = "!#C|" + username;

            // Pretvorba stringov v IP in port
            if (IPString.Equals("default"))
            {
                IP = IPAddress.Parse("192.168.1.100");
                port = 1337;
            }

            else
            {
                try
                {
                    IP = IPAddress.Parse(IPString);
                }
                catch (Exception izjema)
                {
                    Console.WriteLine("Napačen IP!");
                    Console.WriteLine("Pritisni tipko za izhod...");
                    Console.ReadKey();
                    return;
                }

                // Branje porta
                try
                {
                    port = Convert.ToInt32(portString);
                }
                catch (Exception izjema)
                {
                    Console.WriteLine("Napačen port!");
                    Console.WriteLine("Pritisni tipko za izhod...");
                    Console.ReadKey();
                    return;
                }
            }

            // Ustvarjanje niti za client
            CreateString();

            // Ustvarjanje listenerja za sporočila od strežnika
            thListener = new Thread(new ThreadStart(ListenForBroadcasts));
            thListener.IsBackground = true;
            thListener.Start();
        }
        
        // Disconnect button
        private void button2_Click(object sender, EventArgs e)
        {
            messageToSend = "!#D|";
            CreateString();
        }

        // Send button
        private void button3_Click(object sender, EventArgs e)
        {
            messageToSend = "!#M|" + textBox5.Text;
            CreateString();
        }
    }
}
