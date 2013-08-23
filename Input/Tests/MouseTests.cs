using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Input.Mocks;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class MouseTests : TestWithMocksOrVisually
	{
		[SetUp, CloseAfterFirstFrame]
		public void SetUp()
		{
			isClicked = false;
		}

		private bool isClicked;

		[Test, CloseAfterFirstFrame]
		public void TestLeftMouseButtonClickPassingAction()
		{
			new Command(() => isClicked = true).Add(new MouseButtonTrigger(MouseButton.Left,
				State.Pressed));
			TestCommand();
		}

		private void TestCommand()
		{
			Assert.IsFalse(isClicked);
			var mockMouse = Resolve<Mouse>() as MockMouse;
			if (mockMouse == null)
				return; //ncrunch: no coverage
			mockMouse.SetButtonState(MouseButton.Left, State.Pressed);
			AdvanceTimeAndUpdateEntities();
			Assert.IsTrue(isClicked);
			Assert.IsTrue(mockMouse.IsAvailable);
		}

		[Test, CloseAfterFirstFrame]
		public void TestLeftMouseButtonClickPassingPositionAction()
		{
			new Command((Point point) => isClicked = true).Add(new MouseButtonTrigger(MouseButton.Left,
				State.Pressed));
			TestCommand();
		}

		[Test, CloseAfterFirstFrame]
		public void GetButtonState()
		{
			var mouse = Resolve<Mouse>();
			Assert.AreEqual(State.Released, mouse.GetButtonState(MouseButton.Left));
			Assert.AreEqual(State.Released, mouse.GetButtonState(MouseButton.Middle));
			Assert.AreEqual(State.Released, mouse.GetButtonState(MouseButton.Right));
			Assert.AreEqual(State.Released, mouse.GetButtonState(MouseButton.X1));
			Assert.AreEqual(State.Released, mouse.GetButtonState(MouseButton.X2));
		}
	}
}