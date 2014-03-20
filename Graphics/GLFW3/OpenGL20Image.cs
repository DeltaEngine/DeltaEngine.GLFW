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
	public class OpenGL20Image : Image
	{
		private readonly GLFW3Device device;

		public int Handle { get; private set; }

		protected OpenGL20Image(string contentName, GLFW3Device device)
			: base(contentName)
		{
			this.device = device;
			CreateHandleAndSetDefaultSamplerState();
		}

		private void CreateHandleAndSetDefaultSamplerState()
		{
			Handle = device.GenerateTexture();
			if (Handle == GLFW3Device.InvalidHandle)
				throw new UnableToCreateOpenGLTexture();
			device.BindTexture(Handle);
			SetSamplerState();
		}

		private OpenGL20Image(ImageCreationData data, GLFW3Device device)
			: base(data)
		{
			this.device = device;
			CreateHandleAndSetDefaultSamplerState();
		}

		protected override void SetSamplerStateAndTryToLoadImage(Stream fileData)
		{
			SetSamplerState();
			LoadImage(fileData);
		}

		protected override void TryLoadImage(Stream fileData)
		{
			using (Bitmap bitmap = new Bitmap(fileData))
			{
				Size bitmapSize = new Size(bitmap.Width, bitmap.Height);
				CompareActualSizeMetadataSize(bitmapSize);
				LoadBitmap(bitmap);
			}
		}

		private void LoadBitmap(Bitmap bitmap)
		{
			device.BindTexture(Handle);
			WarnAboutWrongAlphaFormat(bitmap.PixelFormat == PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, HasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
			device.LoadTextureInNativePlatformFormat(bitmap.Width, bitmap.Height, data.Scan0, HasAlpha);
			bitmap.UnlockBits(data);
			device.BindTexture(0);
		}

		public override void FillRgbaData(byte[] rgbaColors)
		{
			if (PixelSize.Width * PixelSize.Height * 4 != rgbaColors.Length)
				throw new InvalidNumberOfBytes(PixelSize);
			device.BindTexture(Handle);
			device.FillTexture(PixelSize, rgbaColors, HasAlpha);
		}

		protected sealed override void SetSamplerState()
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

		private class UnableToCreateOpenGLTexture : Exception {}
	}
}