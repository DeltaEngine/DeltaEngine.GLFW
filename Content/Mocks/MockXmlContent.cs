using System.IO;
using DeltaEngine.Content.Xml;

namespace DeltaEngine.Content.Mocks
{
	/// <summary>
	/// Mocks xml content in unit tests.
	/// </summary>
	public class MockXmlContent : XmlContent
	{
		public MockXmlContent(string contentName)
			: base(contentName)
		{
			ContentChanged += () => changeCounter++;
		}

		public int changeCounter;

		protected override void LoadData(Stream fileData)
		{
			LoadCounter++;
			if (Name == "VectorText")
			{
				if (vectorTextData == null)
					SetupVectorText();
				Data = vectorTextData;
				return;
			}
			Data = new XmlData("Root");
			Data.AddChild(new XmlData("Hi"));
		}

		public int LoadCounter { get; private set; }

		private static void SetupVectorText()
		{
			vectorTextData = new XmlData("VectorText");
			for (int i = 'A'; i <= 'Z'; i++)
				AddCharacter(i);
			for (int i = '0'; i <= '9'; i++)
				AddCharacter(i);
			AddCharacter('.');
		}

		private static XmlData vectorTextData;

		private static void AddCharacter(int i)
		{
			var character = new XmlData("Char" + i);
			vectorTextData.AddChild(character);
			character.AddAttribute("Character", (char)i);
			character.AddAttribute("Lines", "(0,0)-(1,1)");
		}

		protected override void CreateDefault()
		{
			base.CreateDefault();
			Data = new XmlData("Default");
		}
	}
}