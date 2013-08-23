﻿using DeltaEngine;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Multimedia;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Sprites;

namespace GhostWars
{
	public class Tree : Sprite, Updateable
	{
		public Tree(Point position, Team team)
			: base(new Material(Shader.Position2DColorUv, TreeImageName[0]), position)
		{
			Level = 1;
			Team = team;
			NumberText = new FontText(MainMenu.Font, "",
				Rectangle.FromCenter(position + NumberTextPositionPerLevel[0], new Size(0.1f)))
			{
				RenderLayer = 4
			};
			NumberOfGhosts = team == Team.None ? 0 : 25;
			RenderLayer = 3;
			UpdateImage();
		}

		private static readonly string[] TreeImageName = new[] { "Tree1", "Tree2", "Tree3" };
		public int Level { get; private set; }
		public Team Team { get; set; }

		public int NumberOfGhosts
		{
			get { return numberOfGhosts; }
			set
			{
				numberOfGhosts = value;
				if (numberOfGhosts > Level * 50)
					numberOfGhosts = Level * 50;
				NumberText.Color = Team.ToColor();
				NumberText.Text = numberOfGhosts + "";
			}
		}

		private int numberOfGhosts;
		public FontText NumberText;
		private static readonly Point[] NumberTextPositionPerLevel =
		{
			new Point(0.002f, -0.0475f), new Point(0.002f, -0.075f), new Point(0.0015f, -0.085f)
		};

		public bool IsAi
		{
			get { return Team == Team.ComputerPurple || Team == Team.ComputerTeal; }
		}

		public void Update()
		{
			if (Team == Team.None || MainMenu.State != GameState.Game ||
				!Time.CheckEvery(Level == 1 ? 1.5f : Level == 2 ? 1.0f : 0.75f))
				return;
			NumberOfGhosts++;
			Effects.CreateSparkleEffect(Team, Center +
				new Point(Randomizer.Current.Get(-0.04f, 0.04f), Randomizer.Current.Get(-0.04f, 0.04f)), 1);
		}

		private void UpdateImage()
		{
			Material = new Material(Shader.Position2DColorUv, TreeImageName[Level - 1]);
			Color = Team.ToColor();
			DrawArea = Rectangle.FromCenter(Center, Material.DiffuseMap.PixelSize / 1920.0f);
			NumberText.Center = Center + NumberTextPositionPerLevel[Level - 1];
		}

		public void Attack(Team attackerTeam, int numberOfAttackerGhosts)
		{
			if (Team == Team.None)
				GrabEmptyTree(attackerTeam);
			else if (Team == attackerTeam)
				MoveToOwnTree(numberOfAttackerGhosts);
			else
				AttackEnemyTree(attackerTeam, numberOfAttackerGhosts);
		}

		private void GrabEmptyTree(Team attackerTeam)
		{
			Team = attackerTeam;
			if (attackerTeam == Team.HumanYellow)
				ContentLoader.Load<Sound>("WinningTree").Play();
			UpdateImage();
			Effects.CreateHitEffect(Center);
		}

		private void MoveToOwnTree(int numberOfAttackerGhosts)
		{
			NumberOfGhosts += numberOfAttackerGhosts;
			if (Team == Team.HumanYellow)
				ContentLoader.Load<Sound>("GhostWaveStart").Play(0.7f);
			Effects.CreateSparkleEffect(Team, Center +
				new Point(Randomizer.Current.Get(-0.04f, 0.04f), Randomizer.Current.Get(-0.04f, 0.04f)),
				numberOfAttackerGhosts);
		}

		private void AttackEnemyTree(Team attackerTeam, int numberOfAttackerGhosts)
		{
			ShowAttackEffects(numberOfAttackerGhosts);
			NumberOfGhosts -= DeterminateAttackerGhostsLeft(numberOfAttackerGhosts);
			if (NumberOfGhosts < 0)
			{
				if (attackerTeam == Team.HumanYellow)
					ContentLoader.Load<Sound>("WinningTree").Play();
				else if (Team == Team.HumanYellow)
					ContentLoader.Load<Sound>("LosingTree").Play();
				Team = attackerTeam;
				UpdateImage();
				NumberOfGhosts = 0;
			}
			else
				ContentLoader.Load<Sound>("MalletHit").Play(0.5f);
		}

		private void ShowAttackEffects(int numberOfAttackerGhosts)
		{
			for (int num = 0; num < numberOfAttackerGhosts; num++)
				Effects.CreateDeathEffect(Center +
					new Point(Randomizer.Current.Get(-0.03f, 0.03f), Randomizer.Current.Get(-0.03f, 0.03f)));
			Effects.CreateHitEffect(Center);
		}

		private int DeterminateAttackerGhostsLeft(int numberOfAttackerGhosts)
		{
			if (Time.Total - lastAttackerTime > 1)
				numberOfAttackerGhosts--;
			lastAttackerTime = Time.Total;
			if (NumberOfGhosts > 20)
				numberOfAttackerGhosts--;
			if (NumberOfGhosts > 60)
				numberOfAttackerGhosts--;
			return numberOfAttackerGhosts;
		}

		private float lastAttackerTime;

		public void TryToUpgrade()
		{
			if (Level > 2 || NumberOfGhosts < Level * 30 || Team != Team.HumanYellow)
				return;
			NumberOfGhosts -= Level * 30;
			Level++;
			UpdateImage();
			ContentLoader.Load<Sound>("Upgrading").Play();
			Effects.CreateSparkleEffect(Team, Center +
				new Point(Randomizer.Current.Get(-0.04f, 0.04f), Randomizer.Current.Get(-0.04f, 0.04f)), 8);
		}
	}
}