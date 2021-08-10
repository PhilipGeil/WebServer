using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebServer
{
    class Server
    {
        int timeout = 8;
        Encoding charEncoder = Encoding.UTF8;
        Socket serverSocket;
        private static Dictionary<string, string> extensions = new Dictionary<string, string>()
        { 
            //{ "extension", "content type" }
            { "htm", "text/html" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };
        /// <summary>
        /// Start listenening for requests
        /// </summary>
        /// <param name="iPAddress"></param>
        /// <param name="port"></param>
        /// <param name="maxNofCon"></param>
        /// <param name="contentPath"></param>
        public void Start(IPAddress iPAddress, int port, int maxNofCon, string contentPath)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(iPAddress, port));
                serverSocket.Listen(maxNofCon);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
            }
            catch (Exception)
            {
                Debug.WriteLine("Socket creation has failed");
                throw new SocketException();
            }

            Thread requestListener = new Thread(() =>
            {
                while (true)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();

                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try
                            {
                                // Handle the request.
                                Handler.HandleRequest(clientSocket, contentPath);
                            }
                            catch (Exception)
                            {

                                try
                                {
                                    clientSocket.Close();
                                }
                                catch (Exception)
                                {
                                }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Accepting connection failed");
                        throw;
                    }
                }
            });
            requestListener.Start();
        }

    }
}
