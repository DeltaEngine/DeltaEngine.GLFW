using System;
using System.Collections.Generic;
using System.IO;
using DeltaEngine.Commands;
using DeltaEngine.Content.Xml;
using DeltaEngine.Extensions;

namespace DeltaEngine.Content.Mocks
{
	/// <summary>
	/// Loads mock content used in unit tests.
	/// </summary>
	public class MockContentLoader : ContentLoader
	{
		public MockContentLoader(ContentDataResolver resolver)
			: base("Content")
		{
			base.resolver = resolver;
		}

		protected override Stream GetContentDataStream(ContentData content)
		{
			var stream = Stream.Null;
			if (content.Name.Equals("Test"))
				stream = new XmlFile(new XmlData("Root").AddChild(new XmlData("Hi"))).ToMemoryStream();
			else if (content.Name.Equals("Texts"))
				stream = new XmlFile(new XmlData("Texts").AddChild(GoLocalizationNode)).ToMemoryStream();
			else if (content.Name.Equals("DefaultCommands"))
				stream = CreateInputCommandXml().ToMemoryStream();
			else if (content.Name.Equals("Level"))
				stream = new MemoryStream(LoadLevelJsonAsBytes());
			else if (content.Name.Equals("Verdana12") || content.Name.Equals("Tahoma30"))
				stream = CreateFontXml().ToMemoryStream();
			return stream;
		}

		protected override bool HasValidContentAndMakeSureItIsLoaded()
		{
			return true;
		}

		private static byte[] LoadLevelJsonAsBytes()
		{
			return StringExtensions.ToByteArray(@"""objects"":[
	{
		""width"":32,
		""height"":32,
		""name"":""player""
	}, 
	{
		""width"":32,
		""height"":32,
		""name"":""monster""
	},");
		}

		private static XmlFile CreateFontXml()
		{
			var glyph = new XmlData("Glyph");
			glyph.AddAttribute("Character", ' ');
			glyph.AddAttribute("UV", "0 0 1 16");
			glyph.AddAttribute("AdvanceWidth", "7.34875");
			glyph.AddAttribute("LeftBearing", "0");
			glyph.AddAttribute("RightBearing", "4.21875");
			var glyphs = new XmlData("Glyphs").AddChild(glyph);
			var bitmap = new XmlData("Bitmap");
			bitmap.AddAttribute("Name", "Verdana12Font");
			bitmap.AddAttribute("Width", "128");
			bitmap.AddAttribute("Height", "128");
			var font = new XmlData("Font");
			font.AddAttribute("Family", "Verdana");
			font.AddAttribute("Size", "12");
			font.AddAttribute("Style", "AddOutline");
			font.AddAttribute("LineHeight", "16");
			font.AddChild(bitmap).AddChild(glyphs);
			return new XmlFile(font);
		}

		private static XmlData GoLocalizationNode
		{
			get
			{
				var keyNode = new XmlData("Go");
				keyNode.AddAttribute("en", "Go");
				keyNode.AddAttribute("de", "Los");
				keyNode.AddAttribute("es", "¡vamos!");
				return keyNode;
			}
		}

		private static XmlFile CreateInputCommandXml()
		{
			var inputSettings = new XmlData("InputSettings");
			AddToInputCommandXml(inputSettings, Command.Exit, new MockCommand("KeyTrigger", "Escape"));
			AddToInputCommandsXml(inputSettings, Command.Click,
				new List<MockCommand>
				{
					new MockCommand("KeyTrigger", "Space"),
					new MockCommand("MouseButtonTrigger", "Left"),
					new MockCommand("TouchPressTrigger", ""),
					new MockCommand("GamePadButtonTrigger", "A")
				});
			AddToInputCommandXml(inputSettings, Command.MiddleClick,
				new MockCommand("MouseButtonTrigger", "Middle"));
			AddToInputCommandXml(inputSettings, Command.RightClick,
				new MockCommand("MouseButtonTrigger", "Right"));
			AddToInputCommandsXml(inputSettings, Command.MoveLeft,
				new List<MockCommand>
				{
					new MockCommand("KeyTrigger", "CursorLeft Pressed"),
					new MockCommand("KeyTrigger", "A"),
					new MockCommand("GamePadAnalogTrigger", "LeftThumbStick")
				});
			AddToInputCommandsXml(inputSettings, Command.MoveRight,
				new List<MockCommand>
				{
					new MockCommand("KeyTrigger", "CursorRight Pressed"),
					new MockCommand("KeyTrigger", "D"),
					new MockCommand("GamePadAnalogTrigger", "RightThumbStick")
				});
			AddToInputCommandXml(inputSettings, Command.MoveUp,
				new MockCommand("KeyTrigger", "CursorUp Pressed"));
			AddToInputCommandXml(inputSettings, Command.MoveDown,
				new MockCommand("KeyTrigger", "CursorDown Pressed"));
			AddToInputCommandsXml(inputSettings, Command.MoveDirectly,
				new List<MockCommand>
				{
					new MockCommand("MousePositionTrigger", "Left"),
					new MockCommand("TouchPressTrigger", "")
				});
			AddToInputCommandXml(inputSettings, Command.RotateDirectly,
				new MockCommand("GamePadAnalogTrigger", "RightThumbStick"));
			AddToInputCommandXml(inputSettings, Command.Back,
				new MockCommand("KeyTrigger", "Backspace Pressed"));
			AddToInputCommandXml(inputSettings, Command.Drag,
				new MockCommand("MouseDragTrigger", "Left Pressed"));
			AddToInputCommandXml(inputSettings, Command.Flick, new MockCommand("TouchFlickTrigger", ""));
			AddToInputCommandXml(inputSettings, Command.Pinch, new MockCommand("TouchPinchTrigger", ""));
			AddToInputCommandXml(inputSettings, Command.Hold, new MockCommand("TouchHoldTrigger", ""));
			AddToInputCommandXml(inputSettings, Command.DoubleClick,
				new MockCommand("MouseDoubleClickTrigger", "Left"));
			AddToInputCommandXml(inputSettings, Command.Rotate, new MockCommand("TouchRotateTrigger", ""));
			return new XmlFile(inputSettings);
		}

		private static void AddToInputCommandXml(XmlData inputSettings, string commandName,
			MockCommand command)
		{
			var entry = new XmlData("Command").AddAttribute("Name", commandName);
			entry.AddChild(command.Trigger, command.Command);
			inputSettings.AddChild(entry);
		}

		private struct MockCommand
		{
			public MockCommand(string trigger, string command)
			{
				Trigger = trigger;
				Command = command;
			}

			public readonly string Trigger;
			public readonly string Command;
		}

		private static void AddToInputCommandsXml(XmlData inputSettings, string commandName,
			IEnumerable<MockCommand> commands)
		{
			var entry = new XmlData("Command").AddAttribute("Name", commandName);
			foreach (var command in commands)
				entry.AddChild(command.Trigger, command.Command);
			inputSettings.AddChild(entry);
		}

		protected override ContentMetaData GetMetaData(string contentName,
			Type contentClassType = null)
		{
			if (contentName.StartsWith("Unavailable"))
				return null;
			ContentType contentType = ConvertClassTypeToContentType(contentClassType);
			if (contentType == ContentType.Material)
				return CreateMaterialMetaData(contentName);
			if (contentName.Contains("SpriteSheet") || contentType == ContentType.SpriteSheetAnimation)
				return CreateSpriteSheetAnimationMetaData(contentName);
			if (contentName == "ImageAnimationNoImages")
				return CreateImageAnimationNoImagesMetaData(contentName);
			if (contentName == "ImageAnimation" || contentType == ContentType.ImageAnimation)
				return CreateImageAnimationMetaData(contentName);
			if (contentType == ContentType.Image)
				return CreateImageMetaData(contentName);
			if (contentType == ContentType.Shader)
				return null;
			return new ContentMetaData { Name = contentName, Type = contentType };
		}

		private static ContentType ConvertClassTypeToContentType(Type contentClassType)
		{
			if (contentClassType == null)
				return ContentType.Xml;
			var typeName = contentClassType.Name;
			foreach (var contentType in EnumExtensions.GetEnumValues<ContentType>())
				if (contentType != ContentType.Image && contentType != ContentType.Mesh &&
					typeName.Contains(contentType.ToString()))
					return contentType;
			if (typeName.Contains("Image") || typeName.Contains("Texture"))
				return ContentType.Image;
			if (typeName.Contains("Mesh") || typeName.Contains("Geometry"))
				return ContentType.Mesh;
			return ContentType.Xml;
		}

		private static ContentMetaData CreateMaterialMetaData(string name)
		{
			var metaData = new ContentMetaData { Name = name, Type = ContentType.Material };
			if (!name.Contains("NoShader"))
				metaData.Values.Add("ShaderName", "Position2DUv");
			if (!name.Contains("NoImage"))
				if (name.Contains("ImageAnimation"))
					metaData.Values.Add("ImageOrAnimationName", "ImageAnimation");
				else if (name.Contains("SpriteSheet"))
					metaData.Values.Add("ImageOrAnimationName", "SpriteSheet");
				else
					metaData.Values.Add("ImageOrAnimationName", "DeltaEngineLogo");
			return metaData;
		}

		private static ContentMetaData CreateSpriteSheetAnimationMetaData(string name)
		{
			var metaData = new ContentMetaData { Name = name, Type = ContentType.SpriteSheetAnimation };
			metaData.Values.Add("ImageName", "EarthImages");
			metaData.Values.Add("DefaultDuration", "5.0");
			metaData.Values.Add("SubImageSize", "32,32");
			return metaData;
		}

		private static ContentMetaData CreateImageAnimationMetaData(string name)
		{
			var metaData = new ContentMetaData { Name = name, Type = ContentType.ImageAnimation };
			metaData.Values.Add("ImageNames", "ImageAnimation01,ImageAnimation02,ImageAnimation03");
			metaData.Values.Add("DefaultDuration", "3");
			return metaData;
		}

		private static ContentMetaData CreateImageAnimationNoImagesMetaData(string name)
		{
			var metaData = new ContentMetaData { Name = name, Type = ContentType.ImageAnimation };
			metaData.Values.Add("ImageNames", "");
			metaData.Values.Add("DefaultDuration", "3");
			return metaData;
		}

		private static ContentMetaData CreateImageMetaData(string contentName)
		{
			var metaData = new ContentMetaData { Name = contentName, Type = ContentType.Image };
			metaData.Values.Add("PixelSize", "128,128");
			return metaData;
		}
	}
}