﻿using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;

namespace DeltaEngine.Input
{
	/// <summary>
	/// Provides a way to fetch the current game pad input values.
	/// </summary>
	public abstract class GamePad : InputDevice
	{
		protected GamePad(GamePadNumber number)
		{
			Number = number;
		}

		protected GamePadNumber Number { get; private set; }

		public abstract Point GetLeftThumbStick();
		public abstract Point GetRightThumbStick();
		public abstract float GetLeftTrigger();
		public abstract float GetRightTrigger();
		public abstract State GetButtonState(GamePadButton button);
		public abstract void Vibrate(float strength);

		public override void Update(IEnumerable<Entity> entities)
		{
			if (!IsAvailable)
				return;
			foreach (Entity entity in entities)
			{
				var button = entity as GamePadButtonTrigger;
				if (button != null)
					button.WasInvoked = GetButtonState(button.Button) == button.State;
				var stick = entity as GamePadAnalogTrigger;
				if (stick != null)
					stick.WasInvoked = IsGamePadStickTriggered(stick);
			}
		}

		private bool IsGamePadStickTriggered(GamePadAnalogTrigger trigger)
		{
			switch (trigger.Analog)
			{
			case GamePadAnalog.LeftThumbStick:
				trigger.Position = GetLeftThumbStick();
				break;
			case GamePadAnalog.RightThumbStick:
				trigger.Position = GetRightThumbStick();
				break;
			case GamePadAnalog.LeftTrigger:
				trigger.Position = new Point(GetLeftTrigger(), GetLeftTrigger());
				break;
			case GamePadAnalog.RightTrigger:
				trigger.Position = new Point(GetRightTrigger(), GetRightTrigger());
				break;
			}
			return trigger.Position != Point.Unused;
		}
	}
}