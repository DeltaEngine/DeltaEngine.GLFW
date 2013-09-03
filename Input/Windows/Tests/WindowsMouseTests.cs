using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Input.Windows.Tests
{
	internal class WindowsMouseTests : TestWithMocksOrVisually
	{
		[Test]
		public void DisposingMouse()
		{
			Resolve<Mouse>().Dispose();
		}

		[Test]
		public void SetAndGetPosition()
		{
			var mouse = Resolve<Mouse>();
			var setPoint = new Point(0.8f, 0.4f);
			var moveTrigger = new MouseHoverTrigger();
			mouse.SetPosition(setPoint);
			mouse.Update(new List<Entity>(new[] { moveTrigger }));
		}
	}
}