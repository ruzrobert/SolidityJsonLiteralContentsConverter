using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace SolidityStandardJsonLiteralContents
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			while (true)
			{
				JsonConverterApp();
				
				Console.WriteLine("== DONE ==");
				Console.WriteLine();
			}
		}
		
		private static void JsonConverterApp()
		{
			Console.WriteLine("1) Hey! This console app will help to generate a proper Standard-Json-Input json compatible with a blockchain scanner.");
			Console.WriteLine("2) First, open your Remix IDE, compile your contract and locate an 'artifacts' folder.");
			Console.WriteLine("3) Locate the 'YourContract_metadata.json' file and paste its contents after pressing ENTER key here.");

			Console.WriteLine();
			Console.WriteLine("Press ENTER to open the pasting form.");
			while (Console.ReadKey().Key != ConsoleKey.Enter) { }

			PasteForm pasteForm = new PasteForm("Please paste the contents of 'YourContract_metadata.json':");
			pasteForm.ShowDialog();
			JObject json = TryLoadJsonFromText(pasteForm.PastedText);

			if (json != null)
			{
				RemoveExcessStuff(json);
				ReplaceUrlsWithContent(json);
				SaveModifiedJson(json);
			}
		}

		private static void RemoveExcessStuff(JObject json)
		{
			json.Property("compiler")?.Remove();
			json.Property("output")?.Remove();
			json.Property("version")?.Remove();

			json["settings"]?["compilationTarget"]?.Parent.Remove();
			json["settings"]?["evmVersion"]?.Parent.Remove();
			json["settings"]?["libraries"]?.Parent.Remove();
			json["settings"]?["metadata"]?.Parent.Remove();
		}

		private static void ReplaceUrlsWithContent(JObject json)
		{
			try
			{
				int count = json["sources"].Count();
				int index = 0;
				foreach (JProperty property in json["sources"])
				{
					ReplaceUrlsWithContent(property, index++, count);
				}

				JObject sourcesObject = json["sources"] as JObject;
				JToken last = sourcesObject.Last;
				last.Remove();
				sourcesObject.AddFirst(last);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void ReplaceUrlsWithContent(JProperty source, int index, int count)
		{
			JToken urls = source.Value["urls"];
			if (urls != null)
			{
				string ipfsLink = urls.Values().Where(x => x.Value<string>() != null)
										.FirstOrDefault(x => x.Value<string>().Contains("ipfs/")).Value<string>();
				
				if (ipfsLink != null)
				{
					int lastIndexOfSlash = ipfsLink.LastIndexOf('/');
					string ipfsHash = ipfsLink.Substring(lastIndexOfSlash + 1);

					Console.WriteLine($"[{index + 1} / {count}] Loading '{ipfsHash}' ({source.Name})");

					string ipfsData = DownloadIpfsData(ipfsHash, source.Name);
					if (string.IsNullOrWhiteSpace(ipfsData)) throw new InvalidOperationException();

					Console.WriteLine($"Loaded {ipfsHash}");

					JProperty contentProperty = new JProperty("content", ipfsData);
					source.Value.Children().ToList().ForEach(x => x.Remove());
					source.Value = new JObject(contentProperty);
				}
			}
		}

		private static void SaveModifiedJson(JObject json)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			saveFileDialog.FileName = "YourContract_StandardInputJson.json";

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				string savePath = saveFileDialog.FileName;
				string jsonStr = json.ToString(Formatting.Indented);

				File.WriteAllText(savePath, jsonStr);
				Console.WriteLine($"Saved to {savePath}");
			}
			else
			{
				Console.WriteLine("Cancelled!");
			}
		}

		private static string DownloadIpfsData(string hash, string importName)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			try
			{
				using (TimeoutWebClient webClient = new TimeoutWebClient(10))
				{
					return webClient.DownloadString("https://ipfs.io/ipfs/" + hash);
				}
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.Timeout)
				{
					Console.WriteLine();
					Console.WriteLine($"Unable to load ipfs for hash '{hash}' ({importName}). Please copy the contract contents in the paste window.");

					PasteForm pasteForm = new PasteForm($"Please paste Solidity code for '{importName}' here:");
					if (pasteForm.ShowDialog() != DialogResult.OK)
					{
						throw new OperationCanceledException();
					}

					Console.WriteLine("Pasted script:");
					Console.WriteLine(pasteForm.PastedText);
					Console.WriteLine();

					return pasteForm.PastedText;
				}

				throw ex;
			}
		}

		private static JObject TryLoadJsonFromPath(string path)
		{
			try
			{
				string text = File.ReadAllText(path);
				return TryLoadJsonFromText(text);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}

		private static JObject TryLoadJsonFromText(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<JObject>(json);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
	}

	public class TimeoutWebClient : WebClient
	{
		public int TimeoutSeconds { get; private set; }

		public TimeoutWebClient(int timeoutSeconds) : base()
		{
			TimeoutSeconds = timeoutSeconds;
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest(uri);
			w.Timeout = TimeoutSeconds * 1000;
			return w;
		}
	}
}