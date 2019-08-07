using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneAppTcpCommunication
{
	public class JsonToCSharpProgram
	{
		public static void Start()
		{
			var obj = new
			{
				id = "int",
				name = "string",
				location = "double[2]",
				age = "float"
			};

			var json = LoadSchema("UserData");
			Console.WriteLine(json);

			GeneratedClass(json, "UserData");
		}

		private static string LoadSchema(string name)
		{
			var url = $"{name}Schema.json";
			var stream = new FileStream(url, FileMode.Open);
			var bytes = new byte[1024];
			stream.Read(bytes, 0, bytes.Length);
			stream.Close();

			return Encoding.UTF8.GetString(bytes);
		}

		private static void GeneratedClass(string json, string className)
		{
			var text = $"public class {className}" + '\n' + "{" + '\n';

			var obj = JsonConvert.DeserializeObject<JObject>(json);
			var properties = obj.Properties();
			text = properties.Aggregate(text,
				(current, property) => current + ('\t' + $"public {property.Value} {property.Name};" + '\n'));
			text += "}";

			var bytes = Encoding.UTF8.GetBytes(text);
			
			var file = new FileStream("UserData.cs", FileMode.OpenOrCreate);
			file.Write(bytes, 0, bytes.Length);
			file.Flush();
			file.Close();
		}
	}
}