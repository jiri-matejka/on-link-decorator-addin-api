using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AddinServices.Logic.Test
{
    [TestClass]
    public class FaviconLoaderTests
    {        

        [TestMethod]
        public void TestCorrectIcons()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var loader = new FaviconLoader();            

            TestIcon(loader, "https://www.github.com");
            TestIcon(loader, "http://www.seznam.cz");            
            TestIcon(loader, "https://sede.policia.gob.es/Tasa790_012/ImpresoRellenar"); // no favicon in HTML, no favicon.ico
        }

        private static void TestIcon(FaviconLoader loader, string url)
        {
            Task<string> task = GetHtml(url);
            string html = GetHtml(url).Result;

            Assert.IsNotNull(html);

            FaviconLoader.Result result = loader.Load(new Uri(url), html).Result;

            if (result != null)
            {
                Assert.IsTrue(result.Data != null || result.FaviconUrl == null);

				Size size = new FaviconSizeDetector().DetectSize(result.Data);

                Console.WriteLine(result.FaviconUrl + " " + result.MimeType + " " + size);
            }
            else
                Console.WriteLine(url + ": NO FAVICON");
        }

        private async static Task<string> GetHtml(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string source;
                try
                {
                    source = await client.GetStringAsync(url);
                    return source;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
