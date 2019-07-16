using System;

namespace OneAppTcpCommunication
{
  internal static class Program
  {
    public static void Main(string[] args)
    {
      OneAppTcpServer server = new OneAppTcpServer(8080);
      
      server.AddRoute("register-pod", TcpRequestHandler);
      
      server.Start();
    }

    private static TcpResponse TcpRequestHandler(TcpRequest request)
    {
      Console.WriteLine($"Request: {request.url}, {request.payload}");
      return new TcpResponse()
      {
        code = 0,
        payload = "Pod is registered"
      };
    }
  }
}