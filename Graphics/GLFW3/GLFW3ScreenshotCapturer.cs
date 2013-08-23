using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DeltaEngine.Graphics.GLFW3
{
	/// <summary>
	/// Takes a screenshot in OpenGL.
	/// </summary>
	public class GLFW3ScreenshotCapturer : ScreenshotCapturer
	{
		public GLFW3ScreenshotCapturer(GLFW3Device device, Window window)
		{
			this.device = device;
			this.window = window;
		}

		private readonly GLFW3Device device;
		private readonly Window window;

		public void MakeScreenshot(string fileName)
		{
			if (device == null)
				return;
			width = (int)window.ViewportPixelSize.Width;
			height = (int)window.ViewportPixelSize.Height;
			var rgbData = new byte[width * height * 3];
			device.ReadPixels(new Datatypes.Rectangle(0.0f, 0.0f, width, height), rgbData);
			using (var bitmap = CopyRgbIntoBitmap(rgbData))
			using (Stream stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
				bitmap.Save(stream, ImageFormat.Png);
		}

		private int width;
		private int height;

		private unsafe Bitmap CopyRgbIntoBitmap(byte[] rgbData)
		{
			var bitmap = new Bitmap(width, height);
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
				System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			var bitmapPointer = (byte*)bitmapData.Scan0.ToPointer();
			SwitchTopToBottomAndRgbToBgr(bitmapPointer, rgbData);
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		private unsafe void SwitchTopToBottomAndRgbToBgr(byte* bitmapPointer, byte[] rgbData)
		{
			for (int y = 0; y < height; ++y)
				for (int x = 0; x < width; ++x)
				{
					int targetIndex = (y * width + x) * 3;
					int sourceIndex = (((height - 1) - y) * width + x) * 3;
					bitmapPointer[targetIndex] = rgbData[sourceIndex + 2];
					bitmapPointer[targetIndex + 1] = rgbData[sourceIndex + 1];
					bitmapPointer[targetIndex + 2] = rgbData[sourceIndex];
				}
		}
	}
}