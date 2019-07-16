namespace OneAppTcpCommunication
{
    public class TcpRequest
    {
        public string url;
        public string payload;
    }
    
    public class RegisterPodRequest
    {
        public string id;
        public double[] location;
        public string p2pUrl;
    }
}