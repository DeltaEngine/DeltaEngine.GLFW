﻿using System;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Physics2D.Tests
{
	internal class BulletTests : TestWithMocksOrVisually
	{
		[Test, CloseAfterFirstFrame]
		public void CreateBullet()
		{
			var bulletImpulse = new Point(0.3f, 0.3f);
			const int BulletDamage = 10;
			var bullet = CreateBullet(bulletImpulse, BulletDamage);
			Assert.AreEqual(bulletImpulse / 1000000f, bullet.PhysicsBody.LinearVelocity);
			Assert.AreEqual(BulletDamage, bullet.Damage);
		}

		private Bullet CreateBullet(Point impulse, int damage)
		{
			return new Bullet(Resolve<Physics>(), impulse,
				Rectangle.FromCenter(Point.Half, new Size(0.1f)), damage);
		}
	}
}