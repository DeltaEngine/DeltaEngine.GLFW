using System;
using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Mocks;
using DeltaEngine.Platforms.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Tests.Commands
{
	public class CommandTests
	{
		[SetUp]
		public void InitializeResolver()
		{
			resolver = new MockResolver();
		}

		private MockResolver resolver;

		[TearDown]
		public void RunTestAndDisposeResolverWhenDone()
		{
			resolver.Dispose();
		}

		[Test]
		public void CreateCommandWithManualTrigger()
		{
			var entities = new MockEntitiesRunner(typeof(MockUpdateBehavior));
			customTrigger = new MockTrigger();
			new Command(() => pos += Time.Delta).Add(customTrigger);
			InvokingTriggerWithoutRunningEntitiesDoesNotCauseAction();
			InvokingTriggerOnceWithMultipleRunsOnlyCausesOneAction(entities);
		}

		private MockTrigger customTrigger;
		private float pos;

		private void InvokingTriggerWithoutRunningEntitiesDoesNotCauseAction()
		{
			customTrigger.Invoke();
			Assert.AreEqual(0.0f, pos);
		}

		private void InvokingTriggerOnceWithMultipleRunsOnlyCausesOneAction(
			MockEntitiesRunner entities)
		{
			customTrigger.Invoke();
			entities.RunEntities();
			Assert.AreEqual(0.05f, pos);
			entities.RunEntities();
			Assert.AreEqual(0.05f, pos);
		}

		[Test]
		public void UnableToRegisterCommandWithoutNameOrTriggers()
		{
			Assert.Throws<ArgumentNullException>(() => Command.Register("", null));
			Assert.Throws<Command.UnableToRegisterCommandWithoutTriggers>(
				() => Command.Register("a", null));
		}

		[Test]
		public void RegisteringSameCommandTwiceOverwritesIt()
		{
			new MockEntitiesRunner(typeof(MockUpdateBehavior));
			var exitTrigger = new MockTrigger();
			Command.Register("Exit", exitTrigger);
			Command.Register("Exit", exitTrigger);
		}

		[Test]
		public void CommandNameMustBeRegisteredToCreateANewCommand()
		{
			new MockEntitiesRunner(typeof(MockUpdateBehavior));
			Assert.Throws<Command.CommandNameWasNotRegistered>(
				() => new Command("UnregisteredCommand", (Action)null));
		}

		[Test]
		public void CommandWithPositionAction()
		{
			const string CommandName = "PositionActionCommand";
			var entities = new MockEntitiesRunner(typeof(MockUpdateBehavior));
			var trigger = new MockTrigger();
			Command.Register(CommandName, trigger);
			actionPerformed = false;
			new Command(CommandName, (Point point) => actionPerformed = true);
			AssertActionPerformed(trigger, entities);
		}

		private bool actionPerformed;

		private void AssertActionPerformed(MockTrigger trigger, MockEntitiesRunner entities)
		{
			Assert.IsFalse(actionPerformed);
			trigger.Invoke();
			entities.RunEntities();
			Assert.IsTrue(actionPerformed);
		}

		[Test]
		public void CommandWithDrawAreaAction()
		{
			const string CommandName = "DrawAreaActionCommand";
			var entities = new MockEntitiesRunner(typeof(MockUpdateBehavior));
			var trigger = new MockTrigger();
			Command.Register(CommandName, trigger);
			actionPerformed = false;
			new Command(CommandName, (start, end, dragDone) => actionPerformed = true);
			AssertActionPerformed(trigger, entities);
		}

		[Test]
		public void RegisterCommandWithSeveralTriggers()
		{
			const string CommandName = "CommandWithSeveralTriggers";
			new MockEntitiesRunner(typeof(MockUpdateBehavior));
			var trigger1 = new MockTrigger();
			var trigger2 = new MockTrigger();
			Command.Register(CommandName, trigger1, trigger2);
			var command = new Command(CommandName, (Action)null);
			Assert.AreEqual(2, command.GetTriggers().Count);
		}
	}
}