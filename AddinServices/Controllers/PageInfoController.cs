using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using AddinServices.Logic;

namespace AddinServices.Controllers
{
    public class PageInfoController : ApiController
    {
        public class PageInfoRequest
        {
            public string Address { get; set; }
        }

        [HttpPost]
        public async Task<object> Post(PageInfoRequest request)
        {
            string address = request.Address;

			PageHtmlDownloader.Result result;
			string source;

			PageHtmlDownloader downloader = new PageHtmlDownloader();
			try
			{
				result = await downloader.Download(address);

				if (result == null)
				{
					return new { Error = "PageNotAvailable" };
				}
				else
					source = result.Source;
			}
			catch(UriFormatException)
			{
				return new { Error = "InvalidAddress" };
			}

            string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;

            FaviconLoader loader = new FaviconLoader();
            FaviconLoader.Result favicon = await loader.Load(result.ResultUri, source);
           
            return new { Error = "", Title = title, FaviconUrl = favicon?.FaviconUrl, FaviconData = favicon?.Data, FaviconMime = favicon?.MimeType, ResultUrl = result.ResultUri.ToString().TrimEnd('/')  };
        }

        public string Get()
        {
            return "Running";
        }
    }
}
