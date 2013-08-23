namespace DeltaEngine.Commands
{
	/// <summary>
	/// Allows a trigger to be invoked along with any associated Command or Entity.
	/// </summary>
	public class ManualTrigger : Trigger
	{
		public void Invoke()
		{
			WasInvoked = true;
		}
	}
}