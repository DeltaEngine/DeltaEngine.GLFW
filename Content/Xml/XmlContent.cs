using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace DeltaEngine.Content.Xml
{
	/// <summary>
	/// Loads Xml data via the Content system
	/// </summary>
	public class XmlContent : ContentData
	{
		public XmlContent(string contentName)
			: base(contentName) {}

		public XmlData Data { get; set; }

		protected override void LoadData(Stream fileData)
		{
			try
			{
				Data = new XmlData(XDocument.Load(fileData).Root);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				if (Debugger.IsAttached)
					throw new XmlContentNotFound(Name, ex);
				Data = new XmlData(Name);
			}
		}

		public class XmlContentNotFound : Exception
		{
			public XmlContentNotFound(string contentName, Exception innerException)
				: base(contentName, innerException) {}
		}

		protected override void DisposeData() {}
	}
}