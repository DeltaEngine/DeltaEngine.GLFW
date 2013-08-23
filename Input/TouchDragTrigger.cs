using DeltaEngine.Commands;
using DeltaEngine.Datatypes;

namespace DeltaEngine.Input
{
	/// <summary>
	/// Allows a touch drag to be detected.
	/// </summary>
	public class TouchDragTrigger : DragTrigger
	{
		public TouchDragTrigger()
		{
			Start<Touch>();
		}
	}
}