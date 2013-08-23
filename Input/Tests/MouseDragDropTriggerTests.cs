using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class MouseDragDropTriggerTests : TestWithMocksOrVisually
	{
		[Test, CloseAfterFirstFrame]
		public void Create()
		{
			var trigger = new MouseDragDropTrigger(Rectangle.One, MouseButton.Right);
			Assert.AreEqual(Rectangle.One, trigger.StartArea);
			Assert.AreEqual(MouseButton.Right, trigger.Button);
			Assert.AreEqual(Point.Unused, trigger.StartDragPosition);
		}

		[Test, CloseAfterFirstFrame]
		public void CreateFromString()
		{
			var trigger = new MouseDragDropTrigger("1.1 2.2 3.3 4.4 Right");
			Assert.AreEqual(new Rectangle(1.1f, 2.2f, 3.3f, 4.4f), trigger.StartArea);
			Assert.AreEqual(MouseButton.Right, trigger.Button);
			Assert.AreEqual(Point.Unused, trigger.StartDragPosition);
			Assert.Throws<MouseDragDropTrigger.CannotCreateMouseDragDropTriggerWithoutStartArea>(
				() => new MouseDragDropTrigger("1 2 3"));
		}
	}
}