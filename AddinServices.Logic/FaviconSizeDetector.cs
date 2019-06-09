using System.Drawing;
using System.IO;


namespace AddinServices.Logic
{
	public class Size {
		public int Width { get; set; }
		public int Height { get; set; }

		public override string ToString()
		{
			return $"{Width} x {Height}";
		}
	}

	public class FaviconSizeDetector 
	{
		public Size DetectSize(byte[] imageData)
		{
			try {
				using (MemoryStream stream = new MemoryStream(imageData)) {
					using (Image img = Image.FromStream(stream)) {
						return new Size { Width = img.Width, Height = img.Height };
					}
				}
			}
			catch {
				return null;
			}
		}


	}
}
