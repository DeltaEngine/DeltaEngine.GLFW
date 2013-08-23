﻿using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Shapes;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class TouchDragTriggerTests : TestWithMocksOrVisually
	{
		[Test]
		public void ShowRedCircleOnTouch()
		{
			new FontText(FontXml.Default, "Touch screen to show red circle", Rectangle.One);
			var ellipse = new Ellipse(new Rectangle(0.1f, 0.1f, 0.1f, 0.1f), Color.Red);
			new Command(() => ellipse.Center = Point.Half).Add(new TouchDragTrigger());
			new Command(() => ellipse.Center = Point.Zero).Add(new TouchPressTrigger(State.Released));
		}

		[Test, CloseAfterFirstFrame]
		public void Create()
		{
			Assert.DoesNotThrow(() => new TouchDragTrigger());
		}
	}
}