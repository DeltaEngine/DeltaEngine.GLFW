using System;
using DeltaEngine.Scenes.UserInterfaces.Terminal;

namespace DeltaEngine.Scenes.Tests.UserInterfaces.Terminal
{
	public class TestCommands
	{
		[ConsoleCommand("AddFloats")]
		public float AddFloats(float a, float b)
		{
			return a + b;
		}

		[ConsoleCommand("AddInts")]
		public float AddInts(int a, int b)
		{
			return a + b;
		}

		[ConsoleCommand("ThrowsException")]
		public float ThrowsException()
		{
			throw new TestException();
		}

		public class TestException : Exception {}

		public float NoAttribute()
		{
			return 0;
		}
	}
}