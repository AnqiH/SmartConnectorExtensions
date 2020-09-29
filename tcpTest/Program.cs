using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace tcpTest
{
    class Program
    {
        private static string ipaddr = "100.96.137.152";
        private static int port = 8083;
        private static TcpClient client;
        private byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(ipaddr), port);
            listener.Start();
            Console.WriteLine("Server started...");
            EndPoint remoteEndPoint;

            while (true)
            {
                try
                {
                    client = listener.AcceptTcpClient();
                    Console.WriteLine("Connection accepted...");
                    remoteEndPoint = client.Client.RemoteEndPoint;
                    NetworkStream ns = client.GetStream();
                    byte[] bytes = new byte[1024];
               
                    try
                    {
                        int bytesRead = ns.Read(bytes, 0, bytes.Length);
                        Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytesRead));
                    }
                    catch(EndOfStreamException ex)
                    {
                        Console.WriteLine("Client at IP address {0} closed the connection.", remoteEndPoint);
                    }
                    catch(Exception exc)
                    {
                        Console.WriteLine(exc.ToString());
                    }
     
                }
                catch(Exception exc)
                {
                    Console.WriteLine(exc.ToString());
                    break;
                }
            }
        }
    }
}
