namespace OneAppTcpCommunication
{
	public static class FmsUrl
	{
		public const string RegisterPod = "register-pod";
		public const string RequestPod = "request-pod";
	}

	public enum FmsStatusCode
	{
		Ok = 0,
		PodIsAlreadyRegistered = 1,
		NoPodAvailable = 2
	}
}