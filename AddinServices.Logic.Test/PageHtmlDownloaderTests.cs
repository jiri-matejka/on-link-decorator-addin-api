using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AddinServices.Logic.Test
{
	[TestClass]
	public class PageHtmlDownloaderTests
	{
		[ClassInitialize]
		public static void Initialize(TestContext context)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

		[TestMethod]
		public void HtmlIsDownloaded_WhenSpecifiedCompleteUri()
		{
			PageHtmlDownloader d = new PageHtmlDownloader();

			TestSuccessfulCase(d, "https://www.github.com");
			TestSuccessfulCase(d, "https://www.bing.com");
		}

		[TestMethod]
		public void HtmlIsDownloaded_WhenSpecifiedIncompleteUri()
		{
			PageHtmlDownloader d = new PageHtmlDownloader();

			TestSuccessfulCase(d, "github.com", "https://github.com");
			TestSuccessfulCase(d, "bing.com", "https://bing.com");

			// HTTPS can incorrect certificate, so it fails for https://
			TestSuccessfulCase(d, "clasevirtual.ru/index/peliculas/0-4", "http://clasevirtual.ru/index/peliculas/0-4");
		}

		[TestMethod]
		public void HtmlIsNotDownloaded_WhenSpecifiedInvalidUri()
		{
			PageHtmlDownloader d = new PageHtmlDownloader();

			TestUnsuccessfulCase(d, "github_not_exists.com");
			TestUnsuccessfulCase(d, "bing_not_exists.com");
		}

		private void TestUnsuccessfulCase(PageHtmlDownloader downloader, string address)
		{
			var result = downloader.Download(address).Result;

			Assert.IsNull(result);
		}

			private void TestSuccessfulCase(PageHtmlDownloader downloader, string address, string resultUriExpected = null)
		{
			if (resultUriExpected == null)
				resultUriExpected = address;
			if (resultUriExpected.EndsWith("/"))
				resultUriExpected = resultUriExpected.Substring(0, resultUriExpected.Length - 1);

			var result = downloader.Download(address).Result;

			Assert.IsNotNull(result);
			Assert.IsTrue(!string.IsNullOrEmpty(result.Source));

			string resultUri = result.ResultUri.ToString();
			if (resultUri.EndsWith("/"))
				resultUri = resultUri.Substring(0, resultUri.Length - 1);

			Assert.AreEqual(resultUriExpected, resultUri);			
		}

	}
}
