﻿using System;
using System.Collections.Generic;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Tests.Entities
{
	public class EntityTests
	{
		[SetUp]
		public void InitializeEntitiesRunner()
		{
			entities = new MockEntitiesRunner(typeof(MockUpdateBehavior), typeof(ComponentTests.Rotate));
			entityWithTags = new MockEntity();
			entityWithTags.AddTag(Tag1);
			entityWithTags.AddTag(Tag2);
		}

		private MockEntitiesRunner entities;
		private Entity entityWithTags;
		private const string Tag1 = "Tag1";
		private const string Tag2 = "Tag2";

		[TearDown]
		public void DisposeEntitiesRunner()
		{
			entities.Dispose();
		}

		[Test]
		public void DeactivateAndActivateEntity()
		{
			entityWithTags.Start<MockUpdateBehavior>();
			entityWithTags.IsActive = false;
			Assert.IsFalse(entityWithTags.IsActive);
			Assert.AreEqual(0, entities.GetEntitiesOfType<MockEntity>().Count);
			entityWithTags.IsActive = true;
			Assert.IsTrue(entityWithTags.IsActive);
			Assert.IsTrue(entityWithTags.ContainsBehavior<MockUpdateBehavior>());
			Assert.AreEqual(1, entities.GetEntitiesOfType<MockEntity>().Count);
		}

		[Test]
		public void CheckNameAndDefaultValues()
		{
			Assert.AreEqual(0, entityWithTags.NumberOfComponents);
			Assert.IsTrue(entityWithTags.IsActive);
		}

		[Test]
		public void AddAndRemoveComponent()
		{
			Assert.AreEqual(1, entities.NumberOfEntities);
			var entity = new MockEntity().Add(new object());
			Assert.AreEqual(2, entities.NumberOfEntities);
			Assert.AreEqual(1, entity.NumberOfComponents);
			Assert.IsNotNull(entity.Get<object>());
			entity.Remove<object>();
			Assert.AreEqual(0, entity.NumberOfComponents);
			Assert.IsFalse(entity.Contains<object>());
			Assert.Throws<ArgumentNullException>(() => new MockEntity().Add<object>(null));
		}

		[Test]
		public void ContainsComponentsThatHaveBeenAdded()
		{
			entityWithTags.Add(1.0f).Add("hello").Add(Rectangle.Zero);
			Assert.IsTrue(entityWithTags.Contains<float>());
			Assert.IsTrue(entityWithTags.Contains<string>());
			Assert.IsTrue(entityWithTags.Contains<Rectangle>());
			Assert.IsFalse(entityWithTags.Contains<int>());
		}

		[Test]
		public void ToStringWithTags()
		{
			entityWithTags.ClearTags();
			Assert.AreEqual("MockEntity", entityWithTags.ToString());
			entityWithTags.AddTag("Empty");
			entityWithTags.AddTag("Empty");
			entityWithTags.AddTag("Entity");
			Assert.AreEqual("MockEntity Tags=Empty, Entity", entityWithTags.ToString());
			entityWithTags.RemoveTag("Entity");
			Assert.AreEqual("MockEntity Tags=Empty", entityWithTags.ToString());
			entityWithTags.ClearTags();
			Assert.AreEqual("MockEntity", entityWithTags.ToString());
		}

		[Test]
		public void ToStringWithComponentAndList()
		{
			entityWithTags.IsActive = false;
			Assert.AreEqual("<Inactive> MockEntity Tags=Tag1, Tag2", entityWithTags.ToString());
			var entityWithComponent = new MockEntity().Add(new object()).Add(new Point(1, 2));
			Assert.AreEqual("MockEntity: Object, Point=1, 2", entityWithComponent.ToString());
			var entityWithList = new MockEntity().Add(new List<Color>());
			Assert.AreEqual("MockEntity: List<Color>", entityWithList.ToString());
		}

		[Test]
		public void ToStringWithArrayAndBehavior()
		{
			entityWithTags.Add(new Point[2]);
			Assert.AreEqual("MockEntity Tags=Tag1, Tag2: Point[]", entityWithTags.ToString());
			var entityWithRunner =
				new MockEntity().Start<MockUpdateBehavior>().Start<ComponentTests.Rotate>();
			Assert.AreEqual("MockEntity [MockUpdateBehavior, Rotate]", entityWithRunner.ToString());
		}

		[Test]
		public void ChangeUpdatePriority()
		{
			var updateable = new UpdateableEntity().Add(1);
			Assert.AreEqual(1, updateable.Get<int>());
			Assert.AreEqual(1, entities.GetEntitiesOfType<UpdateableEntity>().Count);
			updateable.UpdatePriority = Priority.High;
			entities.RunEntities();
			Assert.AreEqual(2, updateable.Get<int>());
			Assert.AreEqual(1, entities.GetEntitiesOfType<UpdateableEntity>().Count);
		}

		private class UpdateableEntity : Entity, Updateable
		{
			public void Update()
			{
				Set(1 + Get<int>());
			}
		}

		[Test]
		public void SaveAndLoadEmptyEntityFromMemoryStream()
		{
			var data = BinaryDataExtensions.SaveToMemoryStream(new MockEntity());
			byte[] savedBytes = data.ToArray();
			Assert.AreEqual(GetShortNameLength("MockEntity") + ListLength * 3 + BooleanByte * 2 + 4,
				savedBytes.Length);
			var loadedEntity = data.CreateFromMemoryStream() as Entity;
			Assert.AreEqual(0, loadedEntity.NumberOfComponents);
			Assert.IsTrue(loadedEntity.IsActive);
		}

		private static int GetShortNameLength(string text)
		{
			const int StringLengthByte = 1;
			return StringLengthByte + text.Length;
		}

		private const int ListLength = 1 + BooleanByte;
		private const int BooleanByte = 1;

		[Test]
		public void SaveAndLoadEntityWithOneHandlerFromMemoryStream()
		{
			entityWithTags.Start<MockUpdateBehavior>();
			var data = BinaryDataExtensions.SaveToMemoryStream(entityWithTags);
			byte[] savedBytes = data.ToArray();
			Assert.AreEqual(62, savedBytes.Length);
			var loadedEntity = data.CreateFromMemoryStream() as Entity;
			Assert.IsTrue(loadedEntity.ContainsTag(Tag1));
			Assert.AreEqual(0, loadedEntity.NumberOfComponents);
			Assert.IsTrue(entityWithTags.ContainsBehavior<MockUpdateBehavior>());
			Assert.IsTrue(loadedEntity.IsActive);
		}

		[Test]
		public void SaveAndLoadEntityWithTwoComponentsFromMemoryStream()
		{
			entityWithTags.Add(1).Add(0.1f);
			var data = BinaryDataExtensions.SaveToMemoryStream(entityWithTags);
			byte[] savedBytes = data.ToArray();
			Assert.AreEqual(65, savedBytes.Length);
			var loadedEntity = data.CreateFromMemoryStream() as Entity;
			Assert.IsTrue(loadedEntity.ContainsTag(Tag1));
			Assert.AreEqual(2, loadedEntity.NumberOfComponents);
			Assert.AreEqual(1, entityWithTags.Get<int>());
			Assert.AreEqual(0.1f, entityWithTags.Get<float>());
			Assert.IsTrue(loadedEntity.IsActive);
		}

		[Test]
		public void GetAndSetComponent()
		{
			entityWithTags.Set(Color.Red);
			Assert.AreEqual(Color.Red, entityWithTags.Get<Color>());
			entityWithTags.Set(Color.Green);
			Assert.AreEqual(Color.Green, entityWithTags.Get<Color>());
		}

		[Test]
		public void SettingComponentToNullThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => entityWithTags.Set<string>(null));
		}

		[Test]
		public void CreateAndGetComponent()
		{
			Assert.AreEqual(new Color(), entityWithTags.GetOrDefault(new Color()));
			entityWithTags.Set(Color.Red);
			Assert.AreEqual(Color.Red, entityWithTags.GetOrDefault(new Color()));
		}

		[Test]
		public void GettingComponentThatDoesNotExistFails()
		{
			Assert.Throws<Entity.ComponentNotFound>(() => entityWithTags.Get<Point>());
		}

		[Test]
		public void AddingInstantiatedHandlerThrowsException()
		{
			Assert.Throws<Entity.InstantiatedHandlerAddedToEntity>(
				() => entityWithTags.Add(new MockUpdateBehavior()));
		}

		[Test]
		public void AddingComponentOfTheSameTypeTwiceErrors()
		{
			entityWithTags.Add(Size.Zero);
			Assert.Throws<Entity.ComponentOfTheSameTypeAddedMoreThanOnce>(
				() => entityWithTags.Add(Size.One));
			entityWithTags.Remove<Size>();
			entityWithTags.Add(Size.One);
		}

		[Test]
		public void StartAndStopBehavior()
		{
			entityWithTags.Start<MockUpdateBehavior>();
			Assert.IsTrue(entityWithTags.ContainsBehavior<MockUpdateBehavior>());
			Assert.AreEqual(1, entities.GetEntitiesOfType<MockEntity>().Count);
			entityWithTags.Stop<MockUpdateBehavior>();
			Assert.IsFalse(entityWithTags.ContainsBehavior<MockUpdateBehavior>());
			Assert.AreEqual(1, entities.GetEntitiesOfType<MockEntity>().Count);
			entityWithTags.Stop<MockUpdateBehavior>();
		}

		[Test]
		public void AddingTags()
		{
			Assert.IsTrue(entityWithTags.ContainsTag(Tag1));
			Assert.IsTrue(entityWithTags.ContainsTag(Tag2));
			var entitiesWithTag1 = entities.GetEntitiesWithTag(Tag1);
			Assert.AreEqual(1, entitiesWithTag1.Count);
			Assert.AreEqual(entityWithTags, entitiesWithTag1[0]);
		}

		[Test]
		public void AddingSameTagAgainDoesNothing()
		{
			entityWithTags.AddTag(Tag1);
			Assert.AreEqual(1, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void RemoveTag()
		{
			entityWithTags.RemoveTag(Tag1);
			Assert.IsFalse(entityWithTags.ContainsTag(Tag1));
			Assert.IsTrue(entityWithTags.ContainsTag(Tag2));
			Assert.AreEqual(0, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void ClearTags()
		{
			entityWithTags.ClearTags();
			Assert.IsFalse(entityWithTags.ContainsTag(Tag1));
			Assert.IsFalse(entityWithTags.ContainsTag(Tag2));
			Assert.AreEqual(0, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void GetTags()
		{
			Assert.AreEqual(2, entityWithTags.GetTags().Count);
			Assert.AreEqual(Tag1, entityWithTags.GetTags()[0]);
			Assert.AreEqual(Tag2, entityWithTags.GetTags()[1]);
		}

		[Test]
		public void InactivatingEntityClearsTags()
		{
			entityWithTags.IsActive = false;
			Assert.AreEqual(0, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void ReactivatingEntityRestoresTags()
		{
			entityWithTags.IsActive = false;
			entityWithTags.IsActive = true;
			Assert.AreEqual(1, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void TagsAddedWhileInactiveTakeEffectAfterReactivation()
		{
			entityWithTags.IsActive = false;
			entityWithTags.AddTag(Tag1);
			Assert.AreEqual(0, entities.GetEntitiesWithTag(Tag1).Count);
			entityWithTags.IsActive = true;
			Assert.AreEqual(1, entities.GetEntitiesWithTag(Tag1).Count);
		}

		[Test]
		public void TogglingVisibilityOnHiddenDrawableEntityShowsIt()
		{
			var drawable = new DrawableEntity { Visibility = Visibility.Hide };
			drawable.ToggleVisibility();
			Assert.AreEqual(Visibility.Show, drawable.Visibility);
		}

		[Test]
		public void TogglingVisibilityOnShownDrawableEntityHidesIt()
		{
			var drawable = new DrawableEntity { Visibility = Visibility.Show };
			drawable.ToggleVisibility();
			Assert.AreEqual(Visibility.Hide, drawable.Visibility);
		}
	}
}