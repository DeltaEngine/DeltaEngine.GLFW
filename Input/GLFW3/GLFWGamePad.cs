using System;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Extensions;
using Pencil.Gaming;

namespace DeltaEngine.Input.GLFW3
{
	/// <summary>
	/// GLFW implementation of a GamePad.
	/// Not all features of the Xbox GamePad are available.
	/// </summary>
	public class GLFWGamePad : GamePad
	{
		public GLFWGamePad(Window window, GamePadNumber number = GamePadNumber.Any)
			: base(number)
		{
			if (window.Handle == null)
				throw new CannotCreateGamePadWithoutWindow();
			states = new State[GamePadButton.A.GetCount()];
			for (int i = 0; i < states.Length; i++)
				states[i] = State.Released;
		}

		private class CannotCreateGamePadWithoutWindow : Exception {}

		private readonly State[] states;

		public override void Vibrate(float strength) {}

		public override bool IsAvailable
		{
			get { return GetPresence(GetJoystickByNumber()); }
			protected set { }
		}

		private static bool GetPresence(Joystick joystick)
		{
			return Glfw.JoystickPresent(joystick);
		}

		public override void Dispose() {}

		private Joystick GetJoystickByNumber()
		{
			if (Number == GamePadNumber.Any)
				return GetAnyJoystick();
			if (Number == GamePadNumber.Two)
				return Joystick.Joystick2;
			if (Number == GamePadNumber.Three)
				return Joystick.Joystick3;
			return Number == GamePadNumber.Four ? Joystick.Joystick4 : Joystick.Joystick1;
		}

		private static Joystick GetAnyJoystick()
		{
			if (GetPresence(Joystick.Joystick2))
				return Joystick.Joystick2;
			if (GetPresence(Joystick.Joystick3))
				return Joystick.Joystick3;
			return GetPresence(Joystick.Joystick4) ? Joystick.Joystick4 : Joystick.Joystick1;
		}

		public override Vector2D GetLeftThumbStick()
		{
			return new Vector2D(axisValues[0], axisValues[1]);
		}

		private float[] axisValues;

		public override Vector2D GetRightThumbStick()
		{
			return new Vector2D(axisValues[4], -axisValues[3]);
		}

		public override float GetLeftTrigger()
		{
			return axisValues[2];
		}

		public override float GetRightTrigger()
		{
			return axisValues[2];
		}

		public override State GetButtonState(GamePadButton button)
		{
			return states[(int)button];
		}

		protected override void UpdateGamePadStates()
		{
			axisValues = Glfw.GetJoystickAxes(GetJoystickByNumber());
			var buttons = Glfw.GetJoystickButtons(GetJoystickByNumber());
			if (buttons != null && buttons.Length >= 10)
				UpdateAllButtons(buttons);
		}

		private void UpdateAllButtons(byte[] buttons)
		{
			UpdateNormalButtons(buttons);
			UpdateStickAndShoulderButtons(buttons);
		}

		private void UpdateNormalButtons(byte[] buttons)
		{
			UpdateButton(buttons[0], GamePadButton.A);
			UpdateButton(buttons[1], GamePadButton.B);
			UpdateButton(buttons[2], GamePadButton.X);
			UpdateButton(buttons[3], GamePadButton.Y);
			UpdateButton(buttons[6], GamePadButton.Back);
			UpdateButton(buttons[7], GamePadButton.Start);
		}

		private void UpdateStickAndShoulderButtons(byte[] buttons)
		{
			UpdateButton(buttons[4], GamePadButton.LeftShoulder);
			UpdateButton(buttons[8], GamePadButton.LeftStick);
			UpdateButton(buttons[5], GamePadButton.RightShoulder);
			UpdateButton(buttons[9], GamePadButton.RightStick);
		}

		private void UpdateButton(byte newState, GamePadButton button)
		{
			var buttonIndex = (int)button;
			states[buttonIndex] = states[buttonIndex].UpdateOnNativePressing(newState == 1);
		}
	}
}