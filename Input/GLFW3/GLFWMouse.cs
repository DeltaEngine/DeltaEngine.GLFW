using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.ScreenSpaces;
using Pencil.Gaming;

namespace DeltaEngine.Input.GLFW3
{
	/// <summary>
	/// Native implementation of the Mouse interface using Xna
	/// </summary>
	public sealed class GLFWMouse : Mouse
	{
		public GLFWMouse(Window window)
		{
			IsAvailable = true;
			nativeWindow = (GlfwWindowPtr)window.Handle;
		}

		public override bool IsAvailable { get; protected set; }
		private readonly GlfwWindowPtr nativeWindow;

		public override void Dispose() {}

		public override void SetPosition(Point newPosition)
		{
			newPosition = ScreenSpace.Current.ToPixelSpace(newPosition);
			Glfw.SetCursorPos(nativeWindow, newPosition.X, newPosition.Y);
		}

		public override void Update(IEnumerable<Entity> entities)
		{
			MouseState newState = MouseState.GetMouseState(nativeWindow);
			UpdateValuesFromState(ref newState);
			base.Update(entities);
		}

		private void UpdateValuesFromState(ref MouseState newState)
		{
			Position = ScreenSpace.Current.FromPixelSpace(new Point((float)newState.X, (float)newState.Y));
			ScrollWheelValue = newState.ScrollWheel;
			UpdateButtonStates(ref newState);
		}

		private void UpdateButtonStates(ref MouseState newState)
		{
			LeftButton = LeftButton.UpdateOnNativePressing(newState.LeftButton);
			MiddleButton = MiddleButton.UpdateOnNativePressing(newState.MiddleButton);
			RightButton = RightButton.UpdateOnNativePressing(newState.RightButton);
		}
	}
}