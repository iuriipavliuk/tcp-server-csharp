using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    public class TcpServerProgram
	{
		private static Dictionary<string, RegisterPodRequest> _registeredPods;
		
		static public void Start()
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
					code = (int) FmsStatusCode.NoPodAvailable, 
					payload = "No pod available"
				};

			return new TcpResponse()
			{
				code = (int) FmsStatusCode.Ok,
				payload = "{" + $"'p2pUrl':{_registeredPods.First().Value.p2pUrl}" + "}"
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
					code = (int) FmsStatusCode.PodIsAlreadyRegistered,
					payload = "Pod is already registered"
				};
			}

			_registeredPods.Add(pod.id, pod);
        
			return new TcpResponse()
			{
				code = (int) FmsStatusCode.Ok,
				payload = "Pod is registered"
			};
		}
	}
}