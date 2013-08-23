using System.Collections.Generic;
using System.Reflection;
using DeltaEngine.Platforms;
using DeltaEngine.Scenes.UserInterfaces.Terminal;
using NUnit.Framework;

namespace DeltaEngine.Scenes.Tests.UserInterfaces.Terminal
{
	public class CommandManagerTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void SetUp()
		{
			manager = new CommandManager();
			manager.AddCommand(typeof(TestCommands).GetMethod("AddFloats"), new TestCommands());
		}

		private CommandManager manager;

		[Test]
		public void AddCommandWithNoAttribute()
		{
			Assert.Throws<CommandManager.ConsoleCommandAttributeMissingFromMethod>(
				() => manager.AddCommand(typeof(TestCommands).GetMethod("NoAttribute"), new TestCommands()));
		}

		[Test]
		public void ExecuteUnknownCommand()
		{
			string result = manager.ExecuteCommand("NotRegistered");
			Assert.AreEqual("Error: Unknown console command 'NotRegistered'", result);
		}

		[Test]
		public void ExecuteCommand()
		{
			string result = manager.ExecuteCommand("AddFloats 1 2");
			Assert.AreEqual("Result: '3'", result);
		}

		[Test]
		public void ExecuteCommandWithWrongNumberOfParameters()
		{
			string result = manager.ExecuteCommand("AddFloats 1");
			Assert.AreEqual("Error: The command has 2 parameters, but you entered 1", result);
		}

		[Test]
		public void ExecuteCommandWithAnInvalidParameter()
		{
			string result = manager.ExecuteCommand("AddFloats 1 a");
			Assert.AreEqual(
				"Error: Can't process parameter no. 2: 'Input string was not in a correct format.'", result);
		}

		[Test]
		public void ExecuteCommandThatThrowsException()
		{
			manager.AddCommand(typeof(TestCommands).GetMethod("ThrowsException"), new TestCommands());
			string result = manager.ExecuteCommand("ThrowsException");
			Assert.AreEqual(
				"Error: Exception while invoking the command: " +
					"'" + new TargetInvocationException(null).Message + "'", result);
		}

		[Test]
		public void GetAutoCompletionList()
		{
			manager.AddCommand(typeof(TestCommands).GetMethod("AddInts"), new TestCommands());
			List<string> autoCompletions = manager.GetAutoCompletionList("add");
			Assert.AreEqual(2, autoCompletions.Count);
			Assert.AreEqual("AddFloats Single Single", autoCompletions[0]);
			Assert.AreEqual("AddInts Int32 Int32", autoCompletions[1]);
		}

		[Test]
		public void AutoCompletingNonMatchingStringReturnsInput()
		{
			Assert.AreEqual("z", manager.AutoCompleteString("z"));
		}

		[Test]
		public void AutoCompletingAmbiguousStringReturnsMatchingPartOfInput()
		{
			manager.AddCommand(typeof(TestCommands).GetMethod("AddInts"), new TestCommands());
			Assert.AreEqual("Add", manager.AutoCompleteString("add"));
		}

		[Test]
		public void AutoCompletingUnambiguousStringReturnsMethod()
		{
			manager.AddCommand(typeof(TestCommands).GetMethod("AddInts"), new TestCommands());
			Assert.AreEqual("AddFloats", manager.AutoCompleteString("addf"));
		}
	}
}