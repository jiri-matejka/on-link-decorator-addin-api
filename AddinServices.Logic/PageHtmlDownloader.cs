using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AddinServices.Logic
{
	public class PageHtmlDownloader
	{
		public class Result
		{
			public string Source { get; set; }
			public Uri ResultUri { get; set; }
		}

		public async Task<Result> Download(string address)
		{
			if (string.IsNullOrEmpty(address))
				throw new ArgumentNullException(nameof(address));

			if(address.StartsWith("http://") || address.StartsWith("https://"))
			{
				Uri uri = new Uri(address, UriKind.Absolute);

				string source = await DownloadUri(uri);
				return new Result { Source = source, ResultUri = uri };
			}
			else
			{
				string httpsAddress = "https://" + address;

				Uri httpsUri = new Uri(httpsAddress, UriKind.Absolute);

				string httpsResult = await DownloadUri(httpsUri);
				if (httpsResult != null)
				{
					return new Result { Source = httpsResult, ResultUri = httpsUri };
				}
				else
				{
					string httpAddress = "http://" + address;
					Uri httpUri = new Uri(httpAddress, UriKind.Absolute);
					string httpResult = await DownloadUri(httpUri);
					if (httpResult != null)
					{
						return new Result { Source = httpResult, ResultUri = httpUri };
					}
					else
						return null;
				}
			}
		}

		private async Task<string> DownloadUri(Uri address)
		{
			// TODO: construct HttpClient in constructor
			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(4);
				
				try
				{
					return await client.GetStringAsync(address);
				}
				catch(TimeoutException)
				{
					Console.WriteLine("Timeout for address " + address);
					return null;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					return null;
				}
			}
		}
		
	}
}