using System;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.IO;

namespace AddinServices.Logic //Elmah.Io.FaviconLoader
{
    public class FaviconLoader
    {
        public class Result
        {
            public string FaviconUrl { get; internal set; }
            public byte[] Data { get; internal set; }
            public string MimeType { get; internal set; }
        }


        public async Task<Result> Load(Uri url, string html)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (html == null)
                throw new ArgumentNullException(nameof(html));

            Result extractedIcon = await ExamineHtml(url, html);
            if (extractedIcon != null)
                return extractedIcon;

            Result guessedIcon = await TryFaviconIco(url);
            return guessedIcon;
        }

        private async Task<Result> ExamineHtml(Uri url, string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // If no <link rel="icon" ...> found just return null;
            var elements = htmlDocument.DocumentNode.SelectNodes("//link[contains(@rel, 'icon')]");
            if (elements == null || !elements.Any()) return null;

            // If favicon link was found, but no href specified just return null;
            var favicon = elements.First();
            var href = favicon.GetAttributeValue("href", null);
            if (string.IsNullOrWhiteSpace(href)) return null;

            // If not a valid url just return null
            Uri faviconUrl;
            if (!Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out faviconUrl)) return null;

            // If relative, force absolute url
            if (!faviconUrl.IsAbsoluteUri)
            {
                faviconUrl = new Uri(url, faviconUrl);
            }

            // Return favicon url if it can be requested.
            Tuple<byte[], string> data = await LoadIcon(faviconUrl);
            if (data != null)
                return new Result { FaviconUrl = faviconUrl.ToString(), Data = data.Item1, MimeType = data.Item2 };
            else
                return null;
        }

        private async Task<Result> TryFaviconIco(Uri url)
        {
            Uri baseUrl = new Uri($"{url.Scheme}://{url.Authority}");
            Uri faviconIcoUrl = new Uri(baseUrl, "favicon.ico");

            Tuple<byte[], string> data = await LoadIcon(faviconIcoUrl);
            if (data != null)
                return new Result { FaviconUrl = faviconIcoUrl.ToString(), Data = data.Item1, MimeType = data.Item2 };
            else
                return null;
        }

        private async Task<Tuple<byte[], string>> LoadIcon(Uri url)
        {
            WebRequest webRequest = HttpWebRequest.Create(url);
            webRequest.Method = "GET";
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)(await webRequest.GetResponseAsync());
            }
            catch(WebException /*e*/)
            {
                //Console.WriteLine(e);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        byte[] data = ReadFully(stream, (int)response.ContentLength);
                        return new Tuple<byte[], string>(data, response.GetResponseHeader("Content-Type"));
                    }
                }
                catch(Exception /*e*/)
                {
                    //Console.WriteLine(e);
                    return null;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// Credit: Jon Skeet
        /// http://jonskeet.uk/csharp/readbinary.html
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }

    public interface IFavicon
    {
        Uri Load(Uri url);
    }
}
