using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    public class OneAppTcpClient
    {
        private System.Net.Sockets.TcpClient _client;
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        
        public OneAppTcpClient(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }
        
        public void SendRequest(TcpRequest request, Action<TcpResponse> callback)
        {
            var requestJson = JsonConvert.SerializeObject(request);

            void OnResponse(string json)
            {
                var response = JsonConvert.DeserializeObject<TcpResponse>(json);
                callback?.Invoke(response);
            }
            
            var task = SendRequest(requestJson, OnResponse);
        }

        private async Task SendRequest(string request, Action<string> callback)
        {
            _client = new System.Net.Sockets.TcpClient();
            _client.Connect(_ipAddress, _port);

            var startOffset = 2;
            var requestData = Encoding.UTF8.GetBytes(request);
            var size = BitConverter.GetBytes(requestData.Length);
            var header = new byte[startOffset];
            var buffer = new byte[requestData.Length + size.Length + header.Length];
            Array.Copy(header, 0, buffer, 0, header.Length);
            Array.Copy(size, 0, buffer, header.Length, size.Length);
            Array.Copy(requestData, 0, buffer, size.Length + header.Length, requestData.Length);
            
            var stream = _client.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            
            size = new byte[4];
            await stream.ReadAsync(size, 0, size.Length).ConfigureAwait(false);

            var responseData = new byte[BitConverter.ToInt32(size, 0)];
            await stream.ReadAsync(responseData, 0, responseData.Length).ConfigureAwait(false);

            stream.Close();
            
            var responseJson = System.Text.Encoding.UTF8.GetString(responseData, 0, responseData.Length);
            callback?.Invoke(responseJson);
        }

        public void Destroy()
        {
            _client.Close();
        }
    }
}