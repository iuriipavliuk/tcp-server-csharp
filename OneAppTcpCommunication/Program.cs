using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OneAppTcpCommunication
{
  internal static class Program
  {
    private static Dictionary<string, RegisterPodRequest> _registeredPods;

    public static void Main(string[] args)
    {
      OneAppTcpServer server = new OneAppTcpServer(8080);
      
      server.AddRoute(FmsUrl.RegisterPod, RegisterPodRequestHandler);
      server.AddRoute(FmsUrl.RequestPod, RequestPodRequestHandler);
      
      server.Start();
    }

    private static TcpResponse RequestPodRequestHandler(TcpRequest request)
    {
      Console.WriteLine($"Request: {request.url}, {request.payload}");

      if (_registeredPods == null || _registeredPods.Count == 0)
        return new TcpResponse()
        {
          code = 2, 
          payload = "No pod available"
        };

      return new TcpResponse()
      {
        code = 0,
        payload = string.Format("{" + "'p2pUrl':{0}" + "}", _registeredPods.First(item => string.IsNullOrEmpty(item.Key)).Value.p2pUrl)
      };
    }

    private static TcpResponse RegisterPodRequestHandler(TcpRequest request)
    {
      Console.WriteLine($"Request: {request.url}, {request.payload}");

      var pod = JsonConvert.DeserializeObject<RegisterPodRequest>(request.payload);

      if (_registeredPods == null)
        _registeredPods = new Dictionary<string, RegisterPodRequest>();

      if (_registeredPods.ContainsKey(pod.id))
      {
        return new TcpResponse()
        {
          code = 1,
          payload = "Pod is already registered"
        };
      }

      _registeredPods.Add(pod.id, pod);
        
      return new TcpResponse()
      {
        code = 0,
        payload = "Pod is registered"
      };
    }
  }
}