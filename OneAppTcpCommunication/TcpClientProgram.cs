using System;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    public class TcpClientProgram
    {
        public static void Start()
        {
            OneAppTcpClient client = new OneAppTcpClient(CommunicationUtils.GetMyIpAddress(), 8080);

            var data = new
            {
                id = "test",
                message = "hello from client"
            };

            var request = new TcpRequest()
            {
                url = FmsUrl.RegisterPod,
                payload = JsonConvert.SerializeObject(data)
            };

            void OnResponse(TcpResponse response)
            {
                Console.WriteLine($"Response: {response.payload}");
            }

            client.SendRequest(request, OnResponse);
        }
    }
}
