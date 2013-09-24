using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;

namespace DeltaEngine.Input.GLFW3
{
	/// <summary>
	/// GLFW does currently not support touch, use another input framework or wait for GLFW3 support.
	/// </summary>
	public sealed class GLFWTouch : Touch
	{
		public GLFWTouch()
		{
			IsAvailable = false;
		}

		public override bool IsAvailable { get; protected set; }

		public override void Dispose() {}

		public override Vector2D GetPosition(int touchIndex)
		{
			return new Vector2D();
		}

		public override State GetState(int touchIndex)
		{
			return State.Released;
		}

		public override void Update(IEnumerable<Entity> entities) {}
	}
}