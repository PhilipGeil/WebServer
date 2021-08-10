using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    public static class Handler
    {
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
        /// Method for handling the client - Serve the correct response etc..
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="contentPath"></param>
        public static void HandleRequest(Socket clientSocket, string contentPath)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Receive the request
            string strReceived = Encoding.UTF8.GetString(buffer, 0, receivedBCount);

            // Parse method of the request
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);

            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                requestedFile = requestedUrl.Split('?')[0];
            else // You can implement other methods...
            {
                SendNotImplemented(clientSocket,
                      File.ReadAllBytes(contentPath + "\\501.html"), "text/html");
                return;
            }

            requestedFile = requestedFile.Replace("/", @"\").Replace("\\..", "");
            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                // Check if this extension is supported
                if (extensions.ContainsKey(extension))
                    if (File.Exists(contentPath + requestedFile))
                        // Send what has been requested because everything is ok
                        SendOkResponse(clientSocket,
                          File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    else
                    {
                        // Don't know what you were looking for, but it's not here - so send a not found
                        SendNotFound(clientSocket,
                          File.ReadAllBytes(contentPath + "\\404.html"), extensions[extension]);
                    }
            }
            else
            {
                if (requestedFile.Substring(length - 1, 1) != @"\")
                    requestedFile += @"\";
                if (File.Exists(contentPath + requestedFile + "index.html"))
                    SendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile + "\\index.html"), "text/html");
                else if (File.Exists(contentPath + requestedFile))
                    SendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile), "text/html");
                else
                {
                    SendNotFound(clientSocket,
                      File.ReadAllBytes(contentPath + "\\404.html"), "text/html");
                }
            }
        }

        /// <summary>
        /// Method for sending a Not Found page
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="contentType"></param>
        private static void SendNotFound(Socket clientSocket, byte[] bContent, string contentType)
        {
            SendResponse(clientSocket, bContent, "404 Not Found", contentType);
        }
        /// <summary>
        /// Send a OK response, with the requested page
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="contentType"></param>
        private static void SendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            SendResponse(clientSocket, bContent, "200 OK", contentType);
        }
        /// <summary>
        /// Method for handling a Not Implemented
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="contentType"></param>
        private static void SendNotImplemented(Socket clientSocket, byte[] bContent, string contentType)
        {
            SendResponse(clientSocket, bContent, "501 Not Implemented", contentType);
        }
        // For strings
        private static void SendResponse(Socket clientSocket, string strContent, string responseCode,
                                  string contentType)
        {
            byte[] bContent = Encoding.UTF8.GetBytes(strContent);
            SendResponse(clientSocket, bContent, responseCode, contentType);
        }

        // For byte arrays
        private static void SendResponse(Socket clientSocket, byte[] bContent, string responseCode,
                                  string contentType)
        {
            try
            {
                byte[] bHeader = Encoding.UTF8.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Atasoy Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch(Exception) {
                Debug.WriteLine("There was an error sending the response to the client");
                throw;
            }
        }
    }
}
