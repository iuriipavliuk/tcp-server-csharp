using System;
using System.Diagnostics;
using System.Net;
using System.Text;
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
            
        public void RegisterPod(Action<TcpResponse> callback)
        {
            var data = new RegisterPodRequest()
            {
                id = "AX101",
                p2pUrl = $"{CommunicationUtils.GetMyIpAddress()}:{4001}",
                location = new double[] {1, 1}// UserAppState.Instance.CurrentLocation.Coordinates.ToArray()
            };

            var request = new TcpRequest()
            {
                url = "register-pod",
                payload = Newtonsoft.Json.JsonConvert.SerializeObject(data)
            };
            
            var requestJson = JsonConvert.SerializeObject(request);

            void OnResponse(string json)
            {
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<TcpResponse>(json);
                callback?.Invoke(response);
            }

            SendRequest(requestJson, OnResponse);
        }

        public void SendRequest(string request, Action<string> callback)
        {
            _client = new System.Net.Sockets.TcpClient();
            _client.Connect(_ipAddress, _port);
                    
            var requestData = Encoding.UTF8.GetBytes(request);
            var size = BitConverter.GetBytes(requestData.Length);
            var buffer = new byte[requestData.Length + size.Length];
            Array.Copy(size, 0, buffer, 0, size.Length);
            Array.Copy(requestData, 0, buffer, size.Length, requestData.Length);
            
            var stream = _client.GetStream();
            stream.Write(buffer, 0, buffer.Length );
            
            size = new byte[4];
            stream.Read(size, 0, size.Length);
            
            var responseData = new byte[BitConverter.ToInt32(size, 0)];
            stream.Read(responseData, 0, responseData.Length);
            
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