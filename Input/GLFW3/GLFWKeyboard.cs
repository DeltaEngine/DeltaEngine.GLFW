using System;
using DeltaEngine.Core;
using Pencil.Gaming;

namespace DeltaEngine.Input.GLFW3
{
	/// <summary>
	/// Native implementation of the Keyboard interface using GLFW
	/// </summary>
	public sealed class GLFWKeyboard : Keyboard
	{
		public GLFWKeyboard(Window window)
		{
			this.window = window;
			if (window.Handle == null)
				throw new CannotCreateKeyboardWithoutWindow();
			IsAvailable = true;
		}

		private class CannotCreateKeyboardWithoutWindow : Exception {}

		private readonly Window window;

		public override void Dispose()
		{
			IsAvailable = false;
		}

		protected override void UpdateKeyStates()
		{
			for (int i = 0; i < (int)Key.NumberOfKeys; i++)
				UpdateKeyState((Key)i);
		}

		private void UpdateKeyState(Key key)
		{
			bool isKeyPressed = Glfw.GetKey((GlfwWindowPtr)window.Handle, ConvertToPencilKey(key));
			keyboardStates[(int)key] = keyboardStates[(int)key].UpdateOnNativePressing(isKeyPressed);
			if (keyboardStates[(int)key] == State.Pressing)
				newlyPressedKeys.Add(key);
		}

		private static Pencil.Gaming.Key ConvertToPencilKey(Key key)
		{
			Pencil.Gaming.Key pencilKey;
			string keyName = key.ToString().Replace("Cursor", "").Replace("NumPad", "KP");
			if (Enum.TryParse(keyName, out pencilKey))
				return pencilKey;

			if (keyName == "Multiply" || keyName == "Add" || keyName == "Subtract" ||
				keyName == "Divide" || keyName == "Decimal")
				if (Enum.TryParse("KP" + keyName, out pencilKey))
					return pencilKey;

			if (key == Key.Scroll)
				return Pencil.Gaming.Key.ScrollLock;
			if (key == Key.OpenBrackets)
				return Pencil.Gaming.Key.LeftBracket;
			if (key == Key.CloseBrackets)
				return Pencil.Gaming.Key.RightBracket;

			return (Pencil.Gaming.Key)key;
		}
	}
}