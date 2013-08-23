using System;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Input.Windows.Tests
{
	public class WindowsKeyboardTests : TestWithMocksOrVisually
	{
		/*[Test]
		public void HandleProcMessageKeyDown()
		{
			var keyboard = new WindowsKeyboard();
			keyboard.HandleProcMessage((IntPtr)Key.A, (IntPtr)0, 0);
			keyboard.Dispose();
			keyboard.Run();
			Assert.AreEqual(State.Pressing, keyboard.GetKeyState(Key.A));
			CloseAfterFirstFrame();
		}

		[Test]
		public void HandleProcMessageKeyUp()
		{
			var keyboard = new WindowsKeyboard();
			keyboard.HandleProcMessage((IntPtr)Key.A, (IntPtr)0, 0);
			keyboard.Run();
			keyboard.HandleProcMessage((IntPtr)Key.A, (IntPtr)(1 << 0xFF), 0);
			keyboard.Dispose();
			keyboard.Run();
			Assert.AreEqual(State.Releasing, keyboard.GetKeyState(Key.A));
			CloseAfterFirstFrame();
		}

		[Test]
		public void HandleProcMessageFullLifecycle()
		{
			var keyboard = new WindowsKeyboard();
			keyboard.HandleProcMessage((IntPtr)Key.A, (IntPtr)0, 0);
			keyboard.Run();
			Assert.AreEqual(State.Pressing, keyboard.GetKeyState(Key.A));
			keyboard.Run();
			Assert.AreEqual(State.Pressed, keyboard.GetKeyState(Key.A));
			keyboard.HandleProcMessage((IntPtr)Key.A, (IntPtr)(1 << 0xFF), 0);
			keyboard.Dispose();
			keyboard.Run();
			Assert.AreEqual(State.Releasing, keyboard.GetKeyState(Key.A));
			keyboard.Run();
			Assert.AreEqual(State.Released, keyboard.GetKeyState(Key.A));
			CloseAfterFirstFrame();
		}*/
	}
}
