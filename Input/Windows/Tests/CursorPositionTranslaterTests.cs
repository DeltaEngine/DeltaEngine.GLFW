using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.ScreenSpaces;
using NUnit.Framework;

namespace DeltaEngine.Input.Windows.Tests
{
	public class CursorPositionTranslaterTests : TestWithMocksOrVisually
	{
		[Test]
		public void GetClientPositionOnScreen()
		{
			var window = Resolve<Window>();
			var translator = new CursorPositionTranslater(window);
			var outsidePosition = Resolve<ScreenSpace>().FromPixelSpace(new Point(-10, -10));
			var screenPos = translator.ToScreenPositionFromScreenSpace(outsidePosition);
			Assert.IsTrue(screenPos.X < window.PixelPosition.X || screenPos.Y < window.PixelPosition.Y);
			Assert.AreEqual(outsidePosition, translator.FromScreenPositionToScreenSpace(screenPos));
		}

		[Test]
		public void ConvertPixelFromScreenPositionAndBack()
		{
			var positionTranslator = new CursorPositionTranslater(Resolve<Window>());
			var topLeftPixel = Point.Zero;
			var outside = positionTranslator.FromScreenPositionToScreenSpace(topLeftPixel);
			Assert.AreEqual(topLeftPixel, positionTranslator.ToScreenPositionFromScreenSpace(outside));
		}
	}
}