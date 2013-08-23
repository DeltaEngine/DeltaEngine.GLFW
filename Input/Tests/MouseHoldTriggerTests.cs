using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Shapes;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class MouseHoldTriggerTests : TestWithMocksOrVisually
	{
		[Test]
		public void HoldLeftClickOnRectangle()
		{
			var drawArea = new Rectangle(0.25f, 0.25f, 0.5f, 0.25f);
			new FilledRect(drawArea, Color.Blue);
			var trigger = new MouseHoldTrigger(drawArea);
			var counter = 0;
			var text = new FontText(FontXml.Default, "", drawArea.Move(new Point(0.0f, 0.25f)));
			new Command(() => text.Text = "MouseHold Triggered " + ++counter + " times.").Add(trigger);
		}

		[Test, CloseAfterFirstFrame]
		public void Create()
		{
			var trigger = new MouseHoldTrigger(Rectangle.One, 0.5f, MouseButton.Right);
			Assert.AreEqual(Rectangle.One, trigger.HoldArea);
			Assert.AreEqual(0.5f, trigger.HoldTime);
			Assert.AreEqual(MouseButton.Right, trigger.Button);
		}

		[Test, CloseAfterFirstFrame]
		public void CreateFromString()
		{
			var trigger = new MouseHoldTrigger("1 2 3 4 5.5 Right");
			Assert.AreEqual(new Rectangle(1, 2, 3, 4), trigger.HoldArea);
			Assert.AreEqual(5.5f, trigger.HoldTime);
			Assert.AreEqual(MouseButton.Right, trigger.Button);
			Assert.Throws<MouseHoldTrigger.CannotCreateMouseHoldTriggerWithoutHoldArea>(
				() => new MouseHoldTrigger("1 2 3"));
		}
	}
}