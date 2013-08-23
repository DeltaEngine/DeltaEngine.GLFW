using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Sprites;
using DeltaEngine.Scenes.UserInterfaces.Controls;
using NUnit.Framework;

namespace DeltaEngine.Scenes.Tests
{
	public class MenuTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void SetUp()
		{
			menu = new Menu(ButtonSize);
			menu.SetBackground(new Material(Shader.Position2DColorUv, "SimpleSubMenuBackground"));
			text = new FontText(FontXml.Default, "", new Rectangle(0.4f, 0.7f, 0.2f, 0.1f));
		}

		private Menu menu;
		private static readonly Size ButtonSize = new Size(0.3f, 0.1f);
		private FontText text;

		[Test, ApproveFirstFrameScreenshot]
		public void ShowMenuWithTwoButtons()
		{
			menu.AddMenuOption(() => { text.Text = "Clicked Top Button"; });
			menu.AddMenuOption(() => { text.Text = "Clicked Bottom Button"; });
			menu.Show();
		}

		[Test]
		public void ShowMenuWithThreeButtons()
		{
			menu.AddMenuOption(() => { text.Text = "Clicked Top Button"; });
			menu.AddMenuOption(() => { text.Text = "Clicked Middle Button"; });
			menu.AddMenuOption(() => { text.Text = "Clicked Bottom Button"; });
			menu.Show();
		}

		[Test]
		public void ShowMenuWithThreeTextButtons()
		{
			var theme = CreateTextTheme();
			menu.AddMenuOption(theme, () => { text.Text = "Clicked Top Button"; },
				"Top Button");
			menu.AddMenuOption(theme, () => { text.Text = "Clicked Middle Button"; },
				"Middle Button");
			menu.AddMenuOption(theme, () => { text.Text = "Clicked Bottom Button"; },
				"Bottom Button");
			menu.Show();
		}

		private static Theme CreateTextTheme()
		{
			return new Theme
			{
				Button = new Theme.Appearance("DefaultSliderBackground", Color.LightGray),
				ButtonMouseover = new Theme.Appearance("DefaultSliderBackground"),
				ButtonPressed = new Theme.Appearance("DefaultSliderBackground", Color.Red)
			};
		}

		[Test, CloseAfterFirstFrame]
		public void CreatingSetsButtonSizeAndMenuCenter()
		{
			Assert.AreEqual(ButtonSize, menu.ButtonSize);
			Assert.AreEqual(Point.Half, menu.Center);
		}

		[Test, CloseAfterFirstFrame]
		public void ChangingButtonSize()
		{
			menu.ButtonSize = Size.Half;
			Assert.AreEqual(Size.Half, menu.ButtonSize);
		}

		[Test, CloseAfterFirstFrame]
		public void ChangingCenterForSetOfButtons()
		{
			menu.Center = Point.One;
			Assert.AreEqual(Point.One, menu.Center);
		}

		[Test, CloseAfterFirstFrame]
		public void AddingMenuOptionAddsButton()
		{
			menu.Center = new Point(0.6f, 0.6f);
			menu.AddMenuOption(() => { });
			Assert.AreEqual(2, menu.Controls.Count);
			var button = (Button)menu.Controls[1];
			Assert.AreEqual(Theme.Default.Button.Material, button.Material);
			Assert.AreEqual(new Rectangle(0.45f, 0.55f, 0.3f, 0.1f), button.DrawArea);
		}

		[Test, CloseAfterFirstFrame]
		public void ClearClearsButtons()
		{
			menu.AddMenuOption(() => { });
			Assert.AreEqual(1, menu.Buttons.Count);
			menu.Clear();
			Assert.AreEqual(0, menu.Buttons.Count);
		}

		[Test, CloseAfterFirstFrame]
		public void ClearMenuOptionsLeavesOtherControlsAlone()
		{
			var logo = new Material(Shader.Position2DUv, "DeltaEngineLogo");
			menu.Add(new Sprite(logo, Rectangle.One));
			menu.AddMenuOption(() => { });
			Assert.AreEqual(1, menu.Buttons.Count);
			Assert.AreEqual(3, menu.Controls.Count);
			menu.ClearMenuOptions();
			Assert.AreEqual(0, menu.Buttons.Count);
			Assert.AreEqual(2, menu.Controls.Count);
		}
	}
}