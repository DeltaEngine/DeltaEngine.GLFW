﻿using System.Collections.Generic;

namespace DeltaEngine.Content.Xml
{
	/// <summary>
	/// For localising an application to a region.
	/// </summary>
	public class Localization : XmlContent
	{
		public Localization(string contentName)
			: base(contentName) {}

		public string GetText(string languageKey)
		{
			var keyNode = Data.GetChild(languageKey);
			if (keyNode == null)
				throw new KeyNotFoundException(languageKey);
			return keyNode.GetAttributeValue(TwoLetterLanguageName);
		}

		public string TwoLetterLanguageName { get; set; }
	}
}