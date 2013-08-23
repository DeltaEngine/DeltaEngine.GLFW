﻿using DeltaEngine.Commands;
using DeltaEngine.Content;
using DeltaEngine.Content.Mocks;
using DeltaEngine.Content.Xml;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Fonts;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class InputCommandsTests : TestWithMocksOrVisually
	{
		[Test]
		public void CountPressingAndReleasing()
		{
			int pressed = 0;
			int released = 0;
			var fontText = new FontText(FontXml.Default,
				"MouseLeft pressed: " + pressed + " released: " + released, Rectangle.One);
			new Command(
				() => fontText.Text = "MouseLeft pressed: " + ++pressed + " released: " + released).Add(
					new MouseButtonTrigger());
			new Command(
				() => fontText.Text = "MouseLeft pressed: " + pressed + " released: " + ++released).Add(
					new MouseButtonTrigger(MouseButton.Left, State.Releasing));
		}

		[Test, CloseAfterFirstFrame]
		public void GetInputCommands()
		{
			using (new MockContentLoader(new ContentDataResolver()))
				ContentLoader.Load<InputCommands>("DefaultCommands");
		}
	}
}