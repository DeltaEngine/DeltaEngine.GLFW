﻿using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;

namespace DeltaEngine.Input
{
	/// <summary>
	/// Allows a touch hold to be detected.
	/// </summary>
	public class TouchHoldTrigger : Trigger
	{
		public TouchHoldTrigger(Rectangle holdArea, float holdTime = DefaultHoldTime)
		{
			HoldArea = holdArea;
			HoldTime = holdTime;
			Start<Touch>();
		}

		public Rectangle HoldArea { get; private set; }
		public float HoldTime { get; private set; }
		private const float DefaultHoldTime = 1.0f;

		public bool IsHovering()
		{
			if (Elapsed >= HoldTime)
				return false;
			Elapsed += Time.Delta;
			return Elapsed >= HoldTime;
		}

		public float Elapsed { get; set; }
		public Point StartPosition { get; set; }
		public Point LastPosition { get; set; }

		public TouchHoldTrigger(string parameter = "")
		{
			Start<Touch>();
		}
	}
}