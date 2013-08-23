using DeltaEngine.Datatypes;

namespace $safeprojectname$
{
	public enum Team
	{
		None,
		HumanYellow,
		ComputerPurple,
		ComputerTeal,
	}

	public static class TeamExtensions
	{
		public static Color ToColor(this Team team)
		{
			return team == Team.None ? Color.Grey : team == Team.HumanYellow ? new Color(204, 255, 153) 
				: team == Team.ComputerPurple ? new Color(214, 166, 238) : new Color(106, 233, 238);
		}
	}
}