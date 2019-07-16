using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    public class OneAppTcpServer
    {
        private int _port;
        private bool _isRunning;

        private readonly TcpListener _listener;

        public OneAppTcpServer(int port)
        {
            _port = port;
            _requestHandlers = new Dictionary<string, TcpRequestHandlerDelegate>();
            _listener = new TcpListener(CommunicationUtils.GetMyIpAddress(), port);
        }

        public void Start()
        {
            var serverThread = new Thread(Run);
            serverThread.Start();
        }

        private void Run()
        {
            _isRunning = true;
            _listener.Start();
            while (_isRunning)
            {
                Console.WriteLine("Waiting...");
                
                TcpClient client = _listener.AcceptTcpClient();
                
                Console.WriteLine("Client connected");
                
                HandleClient(client);
                client.Close();
            }

            _isRunning = false;
            _listener.Stop();
        }
        
        private void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();

            var size = new byte[4];
            stream.Read(size, 0, size.Length);
            var buffer = new byte[BitConverter.ToInt32(size, 0)];
            stream.Read(buffer, 0, buffer.Length);            
            var msg = Encoding.UTF8.GetString(buffer);
            
            Console.WriteLine($"Request: {msg}");

            var request = JsonConvert.DeserializeObject<TcpRequest>(msg);
            
            var response = _requestHandlers.ContainsKey(request.url)
                ? _requestHandlers[request.url].Invoke(request)
                : new TcpResponse() {code = 404, payload = $"Unknown request url: {request.url}"};
            
            var responseData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            size = BitConverter.GetBytes(responseData.Length);
            var result = new byte[responseData.Length + 4];
            Array.Copy(size, 0, result, 0, size.Length);
            Array.Copy(responseData, 0, result, size.Length, responseData.Length);
            
            stream.Write(result, 0, result.Length);
            stream.Close();
        }

        private readonly Dictionary<string, TcpRequestHandlerDelegate> _requestHandlers;
        public delegate TcpResponse TcpRequestHandlerDelegate(TcpRequest request);
        public void AddRoute(string route, TcpRequestHandlerDelegate tcpRequestHandler)
        {
            if(_requestHandlers.ContainsKey(route))
                throw new ArgumentException($"Route handler already exists: {route}");
            
            _requestHandlers.Add(route, tcpRequestHandler);
        }
    }
}