using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using WebSockets.Server;
using System.Diagnostics;
using WebSocketsCmd.Client;
using WebSocketsCmd.Properties;
using WebSockets.Client;
using System.Threading.Tasks;
using WebSockets.Common;
using System.Threading;
using WebSockets;
using WebSockets.Events;
using WebSocketsCmd.Server;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace WebSocketsCmd
{
    public class Program
    {
        private static IWebSocketLogger _logger;

        private static void TestClient(object state)
        {
            try
            {
                string url = (string) state;
                using (var client = new ChatWebSocketClient(true, _logger))
                {
                    Uri uri = new Uri(url);
                    client.TextFrame += Client_TextFrame;
                    client.ConnectionOpened += Client_ConnectionOpened;

                    // test the open handshake
                    client.OpenBlocking(uri);
                }

                _logger.Information(typeof (Program), "Client finished, press any key");
            }
            catch (Exception ex)
            {
                _logger.Error(typeof (Program), ex.ToString());
                _logger.Information(typeof(Program), "Client terminated: Press any key");
            }

            Console.ReadKey();
        }

        private static void Client_ConnectionOpened(object sender, EventArgs e)
        {
            _logger.Information(typeof(Program), "Client: Connection Opened");
            var client = (ChatWebSocketClient)sender;

            // test sending a message to the server
            client.Send("Hi");
        }

        private static void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
            _logger.Information(typeof(Program), "Client: {0}", e.Text);
            var client = (ChatWebSocketClient)sender;

            // lets test the close handshake
            client.Dispose();
        }

        private static X509Certificate2 GetCertificate()
        {
            // it is clearly WRONG to store the certificate and password insecurely on disk like this but this is a demo
            // you would normally use the built in windows certificate store
            string certFile = Settings.Default.CertificateFile;
            if (!File.Exists(certFile))
            {
                throw new FileNotFoundException("Certificate file not found: " + certFile);
            }

            string certPassword = Settings.Default.CertificatePassword;
            var cert = new X509Certificate2(certFile, certPassword);
            _logger.Information(typeof (Program), "Successfully loaded certificate");
            return cert;
        }

        private static void Main(string[] args)
        {
            _logger = new WebSocketLogger();

            try
            {
                int port = 2120;
                string webRoot = Settings.Default.WebRoot;
                if (!Directory.Exists(webRoot))
                {
                    string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                    _logger.Warning(typeof(Program), "Webroot folder {0} not found. Using application base directory: {1}", webRoot, baseFolder);
                    webRoot = baseFolder;
                }

                // used to decide what to do with incoming connections
                ServiceFactory serviceFactory = new ServiceFactory(webRoot, _logger);

                using (WebServer server = new WebServer(serviceFactory, _logger))
                {
                    server.Listen(port);  

                    //Thread clientThread = new Thread(new ParameterizedThreadStart(TestClient));
                    //clientThread.IsBackground = true;
                    
                    //clientThread.Start("ws://localhost:2120/chat");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(Program), ex);
                Console.ReadKey();
            }
        }
    }
}
