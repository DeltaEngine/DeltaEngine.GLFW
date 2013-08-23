﻿using System;
using System.Collections.Generic;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;

namespace DeltaEngine.Commands
{
	/// <summary>
	/// Input Commands are loaded via the InputCommands.xml file, see InputCommands.cs for details.
	/// You can also create your own commands, which will be executed whenever any trigger is invoked.
	/// </summary>
	public class Command : Entity, Updateable
	{
		static Command()
		{
			RegisteredCommands.Use(new Dictionary<string, Trigger[]>());
		}

		private static readonly ThreadStatic<Dictionary<string, Trigger[]>> RegisteredCommands =
			new ThreadStatic<Dictionary<string, Trigger[]>>();

		public static void Register(string commandName, params Trigger[] commandTriggers)
		{
			if (string.IsNullOrEmpty(commandName))
				throw new ArgumentNullException("commandName");
			if (commandTriggers == null || commandTriggers.Length == 0)
				throw new UnableToRegisterCommandWithoutTriggers(commandName);
			if (RegisteredCommands.Current.ContainsKey(commandName))
				RegisteredCommands.Current[commandName] = commandTriggers;
			else
				RegisteredCommands.Current.Add(commandName, commandTriggers);
		}

		public class UnableToRegisterCommandWithoutTriggers : Exception
		{
			public UnableToRegisterCommandWithoutTriggers(string commandName)
				: base(commandName) {}
		}

		public Command(string commandName, Action action)
		{
			triggers.AddRange(LoadTriggersForCommand(commandName));
			this.action = action;
			UpdatePriority = Priority.First;
		}

		private readonly Action action;

		private static IEnumerable<Trigger> LoadTriggersForCommand(string commandName)
		{
			Trigger[] loadedTriggers;
			ContentLoader.Exists("DefaultCommands");
			if (RegisteredCommands.Current.TryGetValue(commandName, out loadedTriggers))
				return loadedTriggers;
			throw new CommandNameWasNotRegistered();
		}

		public class CommandNameWasNotRegistered : Exception {}

		public Command(string commandName, Action<Point> positionAction)
		{
			triggers.AddRange(LoadTriggersForCommand(commandName));
			this.positionAction = positionAction;
			UpdatePriority = Priority.First;
		}

		public Command(string commandName, Action<Point, Point, bool> dragAction)
		{
			triggers.AddRange(LoadTriggersForCommand(commandName));
			this.dragAction = dragAction;
			UpdatePriority = Priority.First;
		}

		public Command(Action action)
		{
			this.action = action;
			UpdatePriority = Priority.First;
		}

		public Command(Action<Point> positionAction)
		{
			this.positionAction = positionAction;
			UpdatePriority = Priority.First;
		}

		private readonly Action<Point> positionAction;

		public Command(Action<Point, Point, bool> dragAction)
		{
			this.dragAction = dragAction;
			UpdatePriority = Priority.First;
		}

		private readonly Action<Point, Point, bool> dragAction;

		public Command Add(Trigger trigger)
		{
			triggers.Add(trigger);
			return this;
		}

		private readonly List<Trigger> triggers = new List<Trigger>();

		public const string Click = "Click";
		public const string DoubleClick = "DoubleClick";
		public const string Hold = "Hold";
		public const string Drag = "Drag";
		public const string Flick = "Flick";
		public const string Pinch = "Pinch";
		public const string Rotate = "Rotate";
		public const string DualDrag = "DualDrag";
		public const string MiddleClick = "MiddleClick";
		public const string RightClick = "RightClick";
		public const string MoveLeft = "MoveLeft";
		public const string MoveRight = "MoveRight";
		public const string MoveUp = "MoveUp";
		public const string MoveDown = "MoveDown";
		/// <summary>
		/// Allows to move left, right, up or down using ASDW or the cursor keys or an onscreen stick.
		/// </summary>
		public const string MoveDirectly = "MoveDirectly";
		/// <summary>
		/// Rotate in 3D space, normally bound to the mouse, game pad thumb stick or onscreen stick.
		/// </summary>
		public const string RotateDirectly = "RotateDirectly";
		/// <summary>
		/// Go back one screen, normally unbound except if scene allows it. Right click or back button.
		/// </summary>
		public const string Back = "Back";
		/// <summary>
		/// Exits whole application (or scene) if supported. Mostly handled by the platform (Alt+F4).
		/// </summary>
		public const string Exit = "Exit";

		public void Update()
		{
			Trigger invokedTrigger = triggers.Find(t => t.WasInvoked);
			if (invokedTrigger == null)
				return;
			var positionTrigger = invokedTrigger as PositionTrigger;
			var dragTrigger = invokedTrigger as DragTrigger;
			if (positionAction != null && positionTrigger != null)
				positionAction(positionTrigger.Position);
			else if (dragAction != null && dragTrigger != null)
				dragAction(dragTrigger.StartPosition, dragTrigger.Position, dragTrigger.DoneDragging);
			else if (action != null)
				action();
			else if (positionAction != null)
				positionAction(Point.Half);
			else if (dragAction != null)
				dragAction(Point.Half, Point.Half, false);
		}

		public List<Trigger> GetTriggers()
		{
			return triggers;
		}
	}
}