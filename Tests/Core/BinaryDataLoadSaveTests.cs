using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DeltaEngine.Content;
using DeltaEngine.Content.Mocks;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Extensions;
using DeltaEngine.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Tests.Core
{
	public class BinaryDataLoadSaveTests
	{
		[Test]
		public void SaveAndLoadPrimitiveDataTypes()
		{
			SaveDataTypeAndLoadAgain((sbyte)-8);
			SaveDataTypeAndLoadAgain(-8);
			SaveDataTypeAndLoadAgain((Int16)8);
			SaveDataTypeAndLoadAgain((UInt16)8);
			SaveDataTypeAndLoadAgain((long)-8);
			SaveDataTypeAndLoadAgain((uint)8);
			SaveDataTypeAndLoadAgain((ulong)8);
			SaveDataTypeAndLoadAgain(3.4f);
			SaveDataTypeAndLoadAgain(8.4);
			SaveDataTypeAndLoadAgain(false);
		}

		private static void SaveDataTypeAndLoadAgain<Primitive>(Primitive input)
		{
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(input);
			var output = BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<Primitive>(data);
			Assert.AreEqual(input, output);
		}

		[Test]
		public void SaveAndLoadOtherDatatypes()
		{
			SaveDataTypeAndLoadAgain("Hi");
			SaveDataTypeAndLoadAgain('x');
			SaveDataTypeAndLoadAgain((decimal)8.4);
			SaveDataTypeAndLoadAgain("asdf".ToCharArray());
			SaveDataTypeAndLoadAgain(StringExtensions.ToByteArray("asdf"));
			SaveDataTypeAndLoadAgain(TestEnum.SomeFlag);
		}

		private enum TestEnum
		{
			SomeFlag,
		}

		[Test]
		public void SaveAndLoadLists()
		{
			SaveAndLoadList(new List<int> { 2, 4, 7, 15 });
			SaveAndLoadList(new List<Object> { 2, 0.5f, "Hello" });
		}

		private static void SaveAndLoadList<Primitive>(List<Primitive> listData)
		{
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(listData);
			var retrievedList =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<List<Primitive>>(data);
			Assert.AreEqual(listData.Count, retrievedList.Count);
			if (typeof(Primitive).IsValueType)
				Assert.IsTrue(listData.Compare(retrievedList));

			for (int index = 0; index < listData.Count; index ++)
				Assert.AreEqual(listData[index].GetType(), retrievedList[index].GetType());
		}

		[Test]
		public void SaveAndLoadDictionaries()
		{
			SaveAndLoadDictionary(new Dictionary<string, string>());
			SaveAndLoadDictionary(new Dictionary<string, string> { { "Key", "Value" } });
			SaveAndLoadDictionary(new Dictionary<string, int> { { "One", 1 }, { "Two", 2 } });
			SaveAndLoadDictionary(new Dictionary<int, float> { { 1, 1.1f }, { 2, 2.2f }, { 3, 3.3f } });
			SaveAndLoadDictionary(new Dictionary<int, object> { { 1, Point.One }, { 2, Color.Red } });
		}

		private static void SaveAndLoadDictionary<Key, Value>(Dictionary<Key, Value> dictionaryData)
		{
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(dictionaryData);
			var retrievedDictionary =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<Dictionary<Key, Value>>(data);
			Assert.AreEqual(dictionaryData.Count, retrievedDictionary.Count);
			if (typeof(Key).IsValueType && typeof(Value).IsValueType)
				Assert.IsTrue(dictionaryData.Compare(retrievedDictionary));
			Assert.IsTrue(!dictionaryData.Except(retrievedDictionary).Any());
		}

		[Test]
		public void SaveAndLoadArrays()
		{
			SaveAndLoadArray(new[] { 2, 4, 7, 15 });
			SaveAndLoadArray(new object[] { 2, 0.5f, "Hello" });
			SaveAndLoadArray(new byte[] { 5, 6, 7 });
			SaveAndLoadArray(new byte[0]);
		}

		private static void SaveAndLoadArray<T>(T[] array)
		{
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(array);
			var retrievedArray = BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<T[]>(data);
			Assert.AreEqual(array.Length, retrievedArray.Length);
			Assert.IsTrue(array.Compare(retrievedArray));
		}

		[Test]
		public void SaveAndLoadArraysContainingNullValues()
		{
			BinaryDataExtensions.SaveDataIntoMemoryStream(new object[] { null });
			BinaryDataExtensions.SaveDataIntoMemoryStream(new object[] { 0, 'a', "hallo", null });
		}

		[Test]
		public void SaveAndLoadClassWithArrays()
		{
			var instance = new ClassWithArrays();
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			var retrieved =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<ClassWithArrays>(data);
			Assert.IsTrue(retrieved.byteData.Compare(new byte[] { 1, 2, 3, 4, 5 }),
				retrieved.byteData.ToText());
			Assert.IsTrue(retrieved.charData.Compare(new[] { 'a', 'b', 'c' }),
				retrieved.charData.ToText());
			Assert.IsTrue(retrieved.intData.Compare(new[] { 10, 20, 30 }), retrieved.intData.ToText());
			Assert.IsTrue(retrieved.stringData.Compare(new[] { "Hi", "there" }),
				retrieved.stringData.ToText());
			Assert.IsTrue(retrieved.enumData.Compare(new[] { DayOfWeek.Monday, DayOfWeek.Sunday }),
				retrieved.enumData.ToText());
			Assert.IsTrue(retrieved.byteEnumData.Compare(new[] { ByteEnum.Normal, ByteEnum.High }),
				retrieved.byteEnumData.ToText());
		}

		private class ClassWithArrays
		{
			public readonly byte[] byteData = { 1, 2, 3, 4, 5 };
			public readonly char[] charData = { 'a', 'b', 'c' };
			public readonly int[] intData = { 10, 20, 30 };
			public readonly string[] stringData = { "Hi", "there" };
			public readonly DayOfWeek[] enumData = { DayOfWeek.Monday, DayOfWeek.Sunday };
			public readonly ByteEnum[] byteEnumData = { ByteEnum.Normal, ByteEnum.High };
		}

		private enum ByteEnum : byte
		{
			Normal,
			High,
		}

		[Test]
		public void SaveAndLoadClassWithEmptyByteArray()
		{
			var instance = new ClassWithByteArray { data = new byte[] { 1, 2, 3 } };
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			var retrieved =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<ClassWithByteArray>(data);
			Assert.IsTrue(instance.data.Compare(retrieved.data));
		}

		private class ClassWithByteArray
		{
			public byte[] data;
		}

		[Test]
		public void SaveAndLoadArrayWithOnlyNullElements()
		{
			var instance = new object[] { null, null };
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			var retrieved = BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<object[]>(data);
			Assert.IsTrue(instance.Compare(retrieved));
		}

		[Test]
		public void SaveAndLoadArrayWithMixedNumbersAndNullElements()
		{
			var instance = new object[] { 1, null };
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			var retrieved = BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<object[]>(data);
			Assert.IsTrue(instance.Compare(retrieved));
		}

		[Test]
		public void SaveAndLoadExplicitLayoutStruct()
		{
			var explicitLayoutTest = new ExplicitLayoutTestClass
			{
				someValue = 8,
				anotherValue = 5,
				unionValue = 7
			};
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(explicitLayoutTest);
			var retrieved =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<ExplicitLayoutTestClass>(data);
			Assert.AreEqual(8, retrieved.someValue);
			Assert.AreEqual(7, retrieved.anotherValue);
			Assert.AreEqual(7, retrieved.unionValue);
		}

		[StructLayout(LayoutKind.Explicit)]
		private class ExplicitLayoutTestClass
		{
			[FieldOffset(0)]
			public int someValue;
			[FieldOffset(4)]
			public int anotherValue;
			[FieldOffset(4)]
			public int unionValue;
		}

		[Test]
		public void SaveAndLoadClassWithAnotherClassInside()
		{
			var instance = new ClassWithAnotherClassInside
			{
				Number = 17,
				Data =
					new ClassWithAnotherClassInside.InnerDerivedClass { Value = 1.5, additionalFlag = true },
				SecondInstanceNotSet = null
			};
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			var retrieved =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<ClassWithAnotherClassInside>(
					data);
			Assert.AreEqual(instance.Number, retrieved.Number);
			Assert.AreEqual(instance.Data.Value, retrieved.Data.Value);
			Assert.AreEqual(instance.Data.additionalFlag, retrieved.Data.additionalFlag);
			Assert.AreEqual(instance.SecondInstanceNotSet, retrieved.SecondInstanceNotSet);
		}

		private class ClassWithAnotherClassInside
		{
			internal class InnerClass
			{
				public double Value { get; set; }
			}

			internal class InnerDerivedClass : InnerClass
			{
				internal bool additionalFlag;
			}

			public int Number { get; set; }

			public InnerDerivedClass Data;
			public InnerDerivedClass SecondInstanceNotSet;
		}

		[Test]
		public void ThrowExceptionTypeNameStartsWithXml()
		{
			Assert.Throws<BinaryDataSaver.UnableToSave>(
				() => BinaryDataExtensions.SaveToMemoryStream(new XmlBinaryData("Xml")));
			Assert.AreEqual("Xml", new XmlBinaryData("Xml").Text);
		}

		private class XmlBinaryData
		{
			public XmlBinaryData(string text)
				: this()
			{
				Text = text;
			}

			public string Text { get; private set; }

			private XmlBinaryData() {}
		}

		[Test]
		public void LoadAndSaveClassWithMemoryStream()
		{
			var instance = new ClassWithMemoryStream(new byte[] { 1, 2, 3, 4 });
			instance.Writer.Write(true);
			instance.Version = 3;
			var data = BinaryDataExtensions.SaveDataIntoMemoryStream(instance);
			// Only the internal data should be saved, 1 byte memory stream not null, 1 byte data length,
			// memory stream data: 4 bytes+1 bool byte, 4 byte for the int Version
			Assert.AreEqual(11, data.Length);
			var retrieved =
				BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<ClassWithMemoryStream>(data);
			Assert.IsNotNull(retrieved.reader);
			Assert.AreEqual(instance.Version, retrieved.Version);
			Assert.AreEqual(instance.Length, retrieved.Length);
			Assert.IsTrue(instance.Bytes.Compare(retrieved.Bytes), retrieved.Bytes.ToText());
		}

		private class ClassWithMemoryStream
		{
			private ClassWithMemoryStream()
			{
				data = new MemoryStream();
				reader = new BinaryReader(data);
				Writer = new BinaryWriter(data);
			}

			public ClassWithMemoryStream(byte[] bytes)
				: this()
			{
				Writer.Write(bytes);
			}

			public int Length
			{
				get { return (int)data.Length; }
			}
			public int Version { get; set; }
			private readonly MemoryStream data;
			internal readonly BinaryReader reader;
			public BinaryWriter Writer { get; private set; }

			public IEnumerable<byte> Bytes
			{
				get { return data.ToArray(); }
			}
		}

		[Test]
		public void LoadingAndSavingKnownTypeShouldNotCauseLoggerMessage()
		{
			using (var logger = new MockLogger())
			{
				var data = BinaryDataExtensions.SaveDataIntoMemoryStream(Point.One);
				var loaded = BinaryDataExtensions.LoadDataWithKnownTypeFromMemoryStream<Point>(data);
				Assert.AreEqual(Point.One, loaded);
				Assert.AreEqual(0, logger.NumberOfMessages);
			}
		}

		[Test]
		public void LoadUnknowTypeShouldThrowException()
		{
			Assert.Throws<Exception>(() => BinaryDataLoader.TryCreateAndLoad(typeof(Point),
				new BinaryReader(new MemoryStream()), new Version(0, 0)));
		}

		[Test]
		public void CreateInstanceOfTypeWithCtorParamsShouldThrowException()
		{
			Assert.Throws<MissingMethodException>(() =>
				BinaryDataLoader.TryCreateAndLoad(typeof(ClassThatRequiresConstructorParameter),
				new BinaryReader(new MemoryStream()), new Version(0, 0)));
		}

		private class ClassThatRequiresConstructorParameter
		{
			//ncrunch: no coverage start 
			public ClassThatRequiresConstructorParameter(string parameter)
			{
				Assert.NotNull(parameter);
			}
			//ncrunch: no coverage end
		}

		[Test]
		public void TestLoadContentType()
		{
			var stream = new MemoryStream();
			var writer = new BinaryWriter(stream);
			const string ContentName = "SomeXml";
			writer.Write(ContentName);
			ContentLoader.current = new MockContentLoader(new ContentDataResolver());
			stream.Position = 0;
			var reader = new BinaryReader(stream);
			object returnedContentType = BinaryDataLoader.TryCreateAndLoad(typeof(MockXmlContentType),
				reader, Assembly.GetExecutingAssembly().GetName().Version);
			var content = returnedContentType as MockXmlContentType;
			Assert.IsNotNull(content);
			Assert.AreEqual(ContentName, content.Name);
		}

		private class MockXmlContentType : ContentData
		{
			//ncrunch: no coverage start
			public MockXmlContentType(string contentName)
				: base(contentName) {}

			protected override void DisposeData() {}
			protected override void LoadData(Stream fileData) { }
			//ncrunch: no coverage end
		}

		[Test]
		public void WriteAndReadNumberMostlyBelow255ThatIsReallyBelow255()
		{
			var data = new MemoryStream();
			var writer = new BinaryWriter(data);
			const int NumberBelow255 = 123456;
			writer.WriteNumberMostlyBelow255(NumberBelow255);
			data.Position = 0;
			var reader = new BinaryReader(data);
			Assert.AreEqual(NumberBelow255, reader.ReadNumberMostlyBelow255());
		}

		[Test]
		public void WriteAndReadNumberMostlyBelow255WithANumberOver255()
		{
			var data = new MemoryStream();
			var writer = new BinaryWriter(data);
			const int NumberOver255 = 123456;
			writer.WriteNumberMostlyBelow255(NumberOver255);
			data.Position = 0;
			var reader = new BinaryReader(data);
			Assert.AreEqual(NumberOver255, reader.ReadNumberMostlyBelow255());
		}

		[Test]
		public void ThrowExceptionOnSavingAnInvalidObject()
		{
			Assert.Throws<NullReferenceException>(
				() =>BinaryDataSaver.TrySaveData(null, typeof(object), null));
		}

		[Test]
		public void SaveContentData()
		{
			new MockContentLoader(new ContentDataResolver());
			var xmlContent = ContentLoader.Load<MockXmlContent>("XmlData");
			using (var dataWriter = new BinaryWriter(new MemoryStream()))
				BinaryDataSaver.TrySaveData(xmlContent, typeof(MockXmlContent), dataWriter);
		}

		[Test]
		public void ThrowExceptionOnSavingAnUnsupportedStream()
		{
			using (var otherStreamThanMemory = new BufferedStream(new MemoryStream()))
			using (var dataWriter = new BinaryWriter(otherStreamThanMemory))
				Assert.Throws<BinaryDataSaver.UnableToSave>(
					() => BinaryDataSaver.TrySaveData(otherStreamThanMemory, typeof(object), dataWriter));
		}
	}
}