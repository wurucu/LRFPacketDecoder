using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading; 

namespace WTool
{
    public class WServer
    {
        int mPort;
        TcpListener tcpServer;
        bool mListen = false; 

        public delegate void DELSocketData(WClient Client, byte[] Data);
        public event DELSocketData ReceiveData;

        public WServer(int Port)
        { 
            this.mPort = Port;
            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"),this.mPort);
        }

        public void Start()
        {
            tcpServer.Start();
            mListen = true;
            Thread thConnectWait = new Thread(new ThreadStart(THSocketConnectWait));
            thConnectWait.Start();
        }

        public void Stop()
        {
            tcpServer.Stop();
            mListen = false;
        }

        private void THSocketConnectWait()
        {
            while (mListen)
            {
                WClient sclient = new WClient(tcpServer.AcceptSocket());
                Thread thWaitData = new Thread(new ParameterizedThreadStart(THSocketDataWait));
                thWaitData.Start(sclient);
            }
        }

        private void THSocketDataWait(object osclient)
        {
            WClient sclient = (WClient)osclient;
            while (sclient.Soket.Connected)
            { 
                byte[] dataArray = new byte[8192];
                int dataCount = sclient.Soket.Receive(dataArray);

                string dataStr = Encoding.Unicode.GetString(dataArray);
                if (dataCount > 0)
                    if (ReceiveData != null)
                        ReceiveData(sclient, dataArray); 

                if (dataCount == 0)
                    break;
            }
        }

         

    }
}
