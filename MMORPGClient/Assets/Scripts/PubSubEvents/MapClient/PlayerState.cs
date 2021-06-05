using System.Numerics;

namespace Assets.Scripts.PubSubEvents.MapClient
{
	public class PlayerState
	{
		public Vector2 Position { get; set; }
		public int Animation { get; set; }
		public bool IsLookingRight { get; set; }
	}
}
