using IniParser;
using IniParser.Model;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CredProvider.NET.Socket
{
    class SocketConnector
    {
        private static SocketConnector instance = new SocketConnector();

        public static SocketConnector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SocketConnector();
                }
                return instance;
            }
        }

        public delegate void CodeCommandHandler(Bitmap imageMessage);

        public delegate void AuthCommandHandler(string accountName, string pwd);

        public event CodeCommandHandler OnReceiveCodeCommand;

        public event AuthCommandHandler OnReceiveAuthCommand;


        private EasyClient client;

        private bool shouldConnected;

        public Guid id;

        private string remoteAddr;

        private int remotePort;

        private SocketConnector()
        {
            id = Guid.NewGuid();
            string endPoint = "10.1.100.100:2021";
            try
            {
                FileIniDataParser parser = new FileIniDataParser();
                IniData data = parser.ReadFile(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CredentialProvider)).Location) + "\\config.ini");
                endPoint = data["common"]["connect.endpoint"];
                Logger.Write("read endpoint from ini:" + endPoint);
                remoteAddr = endPoint;
                remotePort = 2021;

                Logger.Write("endPoint=" + endPoint);
                Logger.Write("port=" + endPoint.Substring(endPoint.IndexOf(":") + 1));

                if (endPoint.IndexOf(":") > 0)
                {
                    remoteAddr = endPoint.Substring(0, endPoint.IndexOf(":"));
                    remotePort = Convert.ToInt32(endPoint.Substring(endPoint.IndexOf(":") + 1));
                }
                else
                {
                    remoteAddr = endPoint;
                }

                client = new EasyClient();
                client.Initialize(new DefaultReceiveFilter(), DispatchMessage);
                client.Connected += Client_Connected;
                client.Error += Client_Error;
                client.Closed += Client_Closed;
            }
            catch (Exception e)
            {
                Logger.Write("parser.ReadFile err:" + e.ToString());
            }
            
        }
        public async Task<bool> ConnectAsync()
        {
            shouldConnected = true;

            bool connected = false;
            while (!connected)
            {
                connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort));
            }

            return true;
        }
        public async Task LoginToSocketAsync()
        {
            if (!client.IsConnected)
            {
                await ConnectAsync();
            }
            string msg = "LOGIN " + id.ToString() + "\r\n";
            Logger.Write("Send message " + msg);
            client.Send(Encoding.ASCII.GetBytes(msg));
        }

        public async Task SendMessagetAsync(string command, string message)
        {
            if (!client.IsConnected)
            {
                await ConnectAsync();
                ;
            }
            string msg = command + " " + message + "\r\n";
            Logger.Write("Send message " + msg);
            client.Send(Encoding.ASCII.GetBytes(msg));
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            Logger.Write("connection closed");
            if (shouldConnected)
            {
                _ = ConnectAsync();
            }
        }

        private void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (e.Exception.ToString().IndexOf("connection error System.Net.Sockets.SocketException (0x80004005)") < 0)
            {
                Logger.Write("connection error " + e.Exception.ToString());
            }
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            Logger.Write("connected");
        }

        private void DispatchMessage(DefaultPackageInfo packageInfo)
        {
            switch (packageInfo.Key)
            {
                case CommandName.CODE:
                    HandlePicMessage(packageInfo);
                    break;
                case CommandName.AUTH:
                    HandleTextMessage(packageInfo);
                    break;
                case CommandName.UNKNOW:
                    HandleServerMessage("unknow command");
                    break;
            }
        }

        private void HandlePicMessage(DefaultPackageInfo packageInfo)
        {
            Logger.Write("Socket get server image");
            using (MemoryStream stream = new MemoryStream(packageInfo.Body))
            {
                Bitmap img = new Bitmap(stream);
                OnReceiveCodeCommand?.Invoke(img);
            }
        }
        private void HandleTextMessage(DefaultPackageInfo packageInfo)
        {
            string msg = Encoding.UTF8.GetString(packageInfo.Body);
            Logger.Write("Socket get server response:" + msg);

            string accountName = "";
            string pwd = "";

            string[] pair = msg.Split(':');
            if (pair.Length > 0)
            {
                accountName = pair[0];
            }
            if (pair.Length > 1)
            {
                pwd = pair[1];
            }

            OnReceiveAuthCommand?.Invoke(accountName, pwd);
        }

        private void HandleServerMessage(string status)
        {
            Logger.Write("Socket info message:" + status);
        }


    }
}
