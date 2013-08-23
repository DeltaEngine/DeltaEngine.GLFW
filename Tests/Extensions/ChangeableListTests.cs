using System.Collections.Generic;
using DeltaEngine.Extensions;
using NUnit.Framework;

namespace DeltaEngine.Tests.Extensions
{
	public class ChangeableListTests
	{
		[Test]
		public void AddAndRemoveWhileEnumerating()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			foreach (int num in list)
				if (num == 5)
					list.Add(7);
				else if (num > 1)
					list.Remove(num);
			Assert.AreEqual("1, 5, 7", list.ToText());
		}

		[Test]
		public void AddRangeOfElementsWhileEnumerating()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			foreach (int num in list)
				if (num == 5)
					list.AddRange(new[] { 2, 2, 2 });
			Assert.IsTrue(list.Contains(2));
			Assert.IsFalse(list.Contains(4));
			Assert.AreEqual(6, list.Count);
		}

		[Test]
		public void AddElementWhileEnumeratingInInnerLoop()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			foreach (int num in list)
			{
				if (num == 5)
					foreach (int num2 in list)
						if (num2 == 1)
							list.Add(10);
				Assert.AreEqual(3, list.Count);
			}
			Assert.AreEqual(4, list.Count);
			list.Remove(10);
			Assert.AreEqual(3, list.Count);
		}

		[Test]
		public void TestCloningChangeableList()
		{
			var testList = new ChangeableList<int> { 1, 2, 3 };
			foreach (int num1 in testList)
			{
				Assert.AreEqual(1, num1);
				testList.Add(1);
				var testList2 = new ChangeableList<int>(testList);
				foreach (int num2 in testList2)
				{
					Assert.AreEqual(1, num2);
					testList2.Add(2);
					// The lists should be different here (testList2 is cloned)
					Assert.False(testList == testList2);
					// But the data in it should be still equal.
					Assert.AreEqual(testList.ToText(), testList2.ToText());
					break;
				}
				break;
			}
		}

		[Test]
		public void GetEmulatorAndResetAndClearIt()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			Assert.IsFalse(list.IsReadOnly);
			var emulator = list.GetEnumerator();
			Assert.AreEqual(emulator.Current, 0);
			emulator.MoveNext();
			emulator.MoveNext();
			emulator.MoveNext();
			emulator.MoveNext();
			emulator.Reset();
			list.Clear();
		}

		[Test]
		public void ConvertChangebleListToArray()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			var array = list.ToArray();
			Assert.AreEqual(new[] { 1, 3, 5 }, array);
		}

		[Test]
		public void RemoveItemFromList()
		{
			var list = new ChangeableList<int> { 1, 3, 5 };
			list.RemoveAt(1);
			Assert.AreEqual(new List<int>() { 1, 5 }, list);
		}

		[Test]
		public void RemoveItemFromListFromEnumerationDepth()
		{
			var list = new ChangeableList<int> { 1, 3 };
			var emulator = list.GetEnumerator();
			emulator.MoveNext();
			emulator.MoveNext();
			emulator.MoveNext();
			emulator.Reset();
			list.RemoveAt(1);
			Assert.AreEqual(new List<int>() { 1, 3 }, list);
		}
	}
}