using System;
using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Scenes.UserInterfaces.Controls;

namespace DeltaEngine.Scenes
{
	/// <summary>
	/// A simple menu system where all buttons are the same size and auto-arrange on screen.
	/// </summary>
	public class Menu : Scene
	{
		public Menu(Size buttonSize)
		{
			this.buttonSize = buttonSize;
			center = Point.Half;
		}

		private Size buttonSize;
		private Point center;

		public Size ButtonSize
		{
			get { return buttonSize; }
			set
			{
				buttonSize = value;
				ArrangeButtons();
			}
		}

		private void ArrangeButtons()
		{
			var left = Center.X - ButtonSize.Width / 2;
			for (int i = 0; i < buttons.Count; i++)
				buttons[i].DrawArea = new Rectangle(left, GetButtonTop(i), ButtonSize.Width,
					ButtonSize.Height);
		}

		private readonly List<InteractiveButton> buttons = new List<InteractiveButton>();

		internal List<InteractiveButton> Buttons
		{
			get { return buttons; }
		}

		private float GetButtonTop(int button)
		{
			float gapHeight = ButtonSize.Height / 2;
			float totalHeight = buttons.Count * ButtonSize.Height + (buttons.Count - 1) * gapHeight;
			float top = Center.Y - totalHeight / 2;
			return top + button * (ButtonSize.Height + gapHeight);
		}

		public Point Center
		{
			get { return center; }
			set
			{
				center = value;
				ArrangeButtons();
			}
		}

		public override void Clear()
		{
			base.Clear();
			buttons.Clear();
		}

		public void ClearMenuOptions()
		{
			foreach (InteractiveButton button in buttons)
				Remove(button);
			buttons.Clear();
		}
		
		public void AddMenuOption(Action clicked, string text = "")
		{
			AddMenuOption(Theme.Default, clicked,text);
		}

		public void AddMenuOption(Theme theme, Action clicked, string text = "")
		{
			AddButton(theme, clicked, text);
			ArrangeButtons();
		}

		private void AddButton(Theme theme, Action clicked, string text)
		{
			var button = new InteractiveButton(theme, new Rectangle(Point.Zero, ButtonSize), text);
			button.Clicked += clicked;
			button.RenderLayer = 10;
			buttons.Add(button);
			Add(button);
		}
	}
}