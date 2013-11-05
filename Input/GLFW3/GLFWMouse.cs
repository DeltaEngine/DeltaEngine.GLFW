using System.Collections.Generic;
using DeltaEngine.Core;
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
			//Glfw.SetScrollCallback(nativeWindow, UpdateScrollWheelValue); See case 9460 7142
		}

		public override bool IsAvailable { get; protected set; }

		private readonly GlfwWindowPtr nativeWindow;

		public override void Dispose() {}

		public override void SetPosition(Vector2D position)
		{
			position = ScreenSpace.Current.ToPixelSpace(position);
			Glfw.SetCursorPos(nativeWindow, position.X, position.Y);
		}

		public override void Update(IEnumerable<Entity> entities)
		{
			MouseState newState = MouseState.GetMouseState(nativeWindow);
			UpdateValuesFromState(newState);
			base.Update(entities);
		}

		private void UpdateValuesFromState(MouseState newState)
		{
			Position =
				ScreenSpace.Current.FromPixelSpace(new Vector2D((float)newState.X, (float)newState.Y));
			LeftButton = LeftButton.UpdateOnNativePressing(newState.LeftButton);
			MiddleButton = MiddleButton.UpdateOnNativePressing(newState.MiddleButton);
			RightButton = RightButton.UpdateOnNativePressing(newState.RightButton);
		}

		private void UpdateScrollWheelValue(GlfwWindowPtr window, double offsetX, double offsetY)
		{
			ScrollWheelValue += (int)offsetY;
		}
	}
}