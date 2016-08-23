using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace WTool
{
    public interface IWClient
    {  
        void Send(byte[] Data);
        void Close();
    }
}
