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

		/// <summary>
		/// The fastest way to convert this enum is also one of the ugliest, Enum.Convert is too slow.
		/// </summary>
		private static Pencil.Gaming.Key ConvertToPencilKey(Key key)
		{
			switch (key)
			{
			case Key.Backspace:
				return Pencil.Gaming.Key.Backspace;
			case Key.Tab:
				return Pencil.Gaming.Key.Tab;
			case Key.Enter:
				return Pencil.Gaming.Key.Enter;
			case Key.Pause:
				return Pencil.Gaming.Key.Pause;
			case Key.CapsLock:
				return Pencil.Gaming.Key.CapsLock;
			case Key.Escape:
				return Pencil.Gaming.Key.Escape;
			case Key.PageUp:
				return Pencil.Gaming.Key.PageUp;
			case Key.PageDown:
				return Pencil.Gaming.Key.PageDown;
			case Key.End:
				return Pencil.Gaming.Key.End;
			case Key.Home:
				return Pencil.Gaming.Key.Home;
			case Key.CursorLeft:
				return Pencil.Gaming.Key.Left;
			case Key.CursorUp:
				return Pencil.Gaming.Key.Up;
			case Key.CursorRight:
				return Pencil.Gaming.Key.Right;
			case Key.CursorDown:
				return Pencil.Gaming.Key.Down;
			case Key.PrintScreen:
				return Pencil.Gaming.Key.PrintScreen;
			case Key.Insert:
				return Pencil.Gaming.Key.Insert;
			case Key.Delete:
				return Pencil.Gaming.Key.Delete;
			case Key.LeftWindows:
				return Pencil.Gaming.Key.LeftSuper;
			case Key.RightWindows:
				return Pencil.Gaming.Key.RightSuper;
			case Key.WindowsKey:
				return Pencil.Gaming.Key.LeftSuper;
			case Key.NumPad0:
				return Pencil.Gaming.Key.KP0;
			case Key.NumPad1:
				return Pencil.Gaming.Key.KP1;
			case Key.NumPad2:
				return Pencil.Gaming.Key.KP2;
			case Key.NumPad3:
				return Pencil.Gaming.Key.KP3;
			case Key.NumPad4:
				return Pencil.Gaming.Key.KP4;
			case Key.NumPad5:
				return Pencil.Gaming.Key.KP5;
			case Key.NumPad6:
				return Pencil.Gaming.Key.KP6;
			case Key.NumPad7:
				return Pencil.Gaming.Key.KP7;
			case Key.NumPad8:
				return Pencil.Gaming.Key.KP8;
			case Key.NumPad9:
				return Pencil.Gaming.Key.KP9;
			case Key.Multiply:
				return Pencil.Gaming.Key.KPMultiply;
			case Key.Add:
				return Pencil.Gaming.Key.KPAdd;
			case Key.Separator:
				return Pencil.Gaming.Key.KPDecimal;
			case Key.Subtract:
				return Pencil.Gaming.Key.KPSubtract;
			case Key.Decimal:
				return Pencil.Gaming.Key.KPDecimal;
			case Key.Divide:
				return Pencil.Gaming.Key.KPDivide;
			case Key.F1:
				return Pencil.Gaming.Key.F1;
			case Key.F2:
				return Pencil.Gaming.Key.F2;
			case Key.F3:
				return Pencil.Gaming.Key.F3;
			case Key.F4:
				return Pencil.Gaming.Key.F4;
			case Key.F5:
				return Pencil.Gaming.Key.F5;
			case Key.F6:
				return Pencil.Gaming.Key.F6;
			case Key.F7:
				return Pencil.Gaming.Key.F7;
			case Key.F8:
				return Pencil.Gaming.Key.F8;
			case Key.F9:
				return Pencil.Gaming.Key.F9;
			case Key.F10:
				return Pencil.Gaming.Key.F10;
			case Key.F11:
				return Pencil.Gaming.Key.F11;
			case Key.F12:
				return Pencil.Gaming.Key.F12;
			case Key.NumLock:
				return Pencil.Gaming.Key.NumLock;
			case Key.Scroll:
				return Pencil.Gaming.Key.ScrollLock;
			case Key.Shift:
				return Pencil.Gaming.Key.LeftShift;
			case Key.Control:
				return Pencil.Gaming.Key.LeftControl;
			case Key.Alt:
				return Pencil.Gaming.Key.LeftAlt;
			case Key.LeftShift:
				return Pencil.Gaming.Key.LeftShift;
			case Key.RightShift:
				return Pencil.Gaming.Key.RightShift;
			case Key.LeftControl:
				return Pencil.Gaming.Key.LeftControl;
			case Key.RightControl:
				return Pencil.Gaming.Key.RightControl;
			case Key.LeftAlt:
				return Pencil.Gaming.Key.LeftAlt;
			case Key.RightAlt:
				return Pencil.Gaming.Key.RightAlt;
			case Key.Semicolon:
				return Pencil.Gaming.Key.Semicolon;
			case Key.Plus:
				return Pencil.Gaming.Key.KPAdd;
			case Key.Comma:
				return Pencil.Gaming.Key.Comma;
			case Key.Minus:
				return Pencil.Gaming.Key.Minus;
			case Key.Period:
				return Pencil.Gaming.Key.Period;
			case Key.Question:
				return Pencil.Gaming.Key.Slash;
			case Key.Tilde:
				return Pencil.Gaming.Key.GraveAccent;
			case Key.OpenBrackets:
				return Pencil.Gaming.Key.LeftBracket;
			case Key.Pipe:
				return Pencil.Gaming.Key.GraveAccent;
			case Key.CloseBrackets:
				return Pencil.Gaming.Key.RightBracket;
			case Key.Quotes:
				return Pencil.Gaming.Key.Apostrophe;
			case Key.Backslash:
				return Pencil.Gaming.Key.Backslash;
			default:
				return (Pencil.Gaming.Key)key;
			}
		}
	}
}