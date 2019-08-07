using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    public class OneAppTcpServer
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private bool _isRunning;

        private readonly TcpListener _listener;

        public OneAppTcpServer(int port)
        {
            _port = port;
            _ipAddress = CommunicationUtils.GetMyIpAddress();

            _requestHandlers = new Dictionary<string, TcpRequestHandlerDelegate>();

            _listener = new TcpListener(_ipAddress, _port);
        }

        public void Start()
        {
            var serverThread = new Thread(Run);
            serverThread.Start();

            Console.WriteLine($"Start server on {_ipAddress}:{_port}");
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
                
                var task = HandleClient(client);
                task.Wait();

                client.Close();
            }

            _isRunning = false;
            _listener.Stop();
        }
        
        private async Task HandleClient(TcpClient client)
        {
            TcpResponse response;
            byte[] buffer;
            string msg;

            var stream = client.GetStream();

            var header = new byte[2];
            await stream.ReadAsync(header, 0, header.Length);

            if(BitConverter.ToInt16(header, 0) == 0)
            {
                var sizeBuffer = new byte[4];
                await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);

                var size = BitConverter.ToInt32(sizeBuffer, 0);

                buffer = new byte[size];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                msg = Encoding.UTF8.GetString(buffer);

                Console.WriteLine($"Request: {msg}");

                var request = JsonConvert.DeserializeObject<TcpRequest>(msg);

                response = _requestHandlers.ContainsKey(request.url)
                    ? _requestHandlers[request.url].Invoke(request)
                    : new TcpResponse() { code = 404, payload = $"Unknown request url: {request.url}" };
            }
            else
            { 
                buffer = new byte[1024];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                
                msg = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"Request: {msg}");
                
                response = new TcpResponse() { code = 404, payload = $"Unknown request: {msg}" };
            }
            
            var responseData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            await stream.WriteAsync(responseData, 0, responseData.Length);

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