using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DeltaEngine.Content;
using Color = DeltaEngine.Datatypes.Color;
using Image = DeltaEngine.Content.Image;
using Size = DeltaEngine.Datatypes.Size;

namespace DeltaEngine.Graphics.GLFW3
{
	/// <summary>
	/// Base class to provide an environment to load texture data in OpenGL.
	/// </summary>
	public class GLFW3Image : Image
	{
		protected GLFW3Image(string contentName, GLFW3Device device)
			: base(contentName)
		{
			this.device = device;
			CreateHandleAndSetDefaultSamplerState();
		}

		private readonly GLFW3Device device;

		private void CreateHandleAndSetDefaultSamplerState()
		{
			Handle = device.GenerateTexture();
			if (Handle == GLFW3Device.InvalidHandle)
				throw new UnableToCreateOpenGLTexture();
			device.BindTexture(Handle);
			SetSamplerState();
		}

		public int Handle { get; private set; }

		private class UnableToCreateOpenGLTexture : Exception {}

		private GLFW3Image(ImageCreationData data, GLFW3Device device)
			: base(data)
		{
			this.device = device;
			CreateHandleAndSetDefaultSamplerState();
		}

		protected override void LoadImage(Stream fileData)
		{
			using (var bitmap = new Bitmap(fileData))
			{
				var bitmapSize = new Size(bitmap.Width, bitmap.Height);
				CompareActualSizeMetadataSize(bitmapSize);
				LoadBitmap(bitmap);
			}
		}

		private void LoadBitmap(Bitmap bitmap)
		{
			device.BindTexture(Handle);
			WarnAboutWrongAlphaFormat(bitmap.PixelFormat == PixelFormat.Format32bppArgb);
			var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly, HasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
			device.LoadTexture(new Size(bitmap.Width, bitmap.Height), data.Scan0, HasAlpha);
			bitmap.UnlockBits(data);
			device.BindTexture(0);
		}

		public override void Fill(Color[] colors)
		{
			if (PixelSize.Width * PixelSize.Height != colors.Length)
				throw new InvalidNumberOfColors(PixelSize);
			device.BindTexture(Handle);
			device.LoadTexture(PixelSize, Color.GetBytesFromArray(colors), true);
		}

		public override void Fill(byte[] colors)
		{
			if (PixelSize.Width * PixelSize.Height * 4 != colors.Length)
				throw new InvalidNumberOfBytes(PixelSize);
			device.BindTexture(Handle);
			device.LoadTexture(PixelSize, colors, true);
		}

		protected override sealed void SetSamplerState()
		{
			device.SetTextureSamplerState(DisableLinearFiltering, AllowTiling);
		}

		protected override void DisposeData()
		{
			if (Handle == GLFW3Device.InvalidHandle)
				return;
			device.DeleteTexture(Handle);
			Handle = GLFW3Device.InvalidHandle;
		}
	}
}