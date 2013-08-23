using System;
using DeltaEngine.Platforms.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Input.Windows.Tests
{
	public class TouchHookTests
	{
		public TouchHookTests()
		{
			resolver = new MockResolver();
		}

		private readonly MockResolver resolver;

		[Test]
		public void GetTouchDataFromHandleWithInvalidHandle()
		{
			var nativeTouches = TouchHook.GetTouchDataFromHandle(1, IntPtr.Zero);
			Assert.Null(nativeTouches);
		}

		/*[Test]
		public void HandleProcMessage()
		{
			var window = resolver.Window;
			var hook = new TouchHook(window);
			hook.HandleProcMessage((IntPtr)4, IntPtr.Zero, 0);
			Assert.IsEmpty(hook.nativeTouches);
		}*/

		[Test]
		public void DisposeHook()
		{
			var window = resolver.Window;
			var hook = new TouchHook(window);
			hook.Dispose();
		}
	}
}