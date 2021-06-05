using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.PubSubEvents.MapClient
{
	public class PlayerState
	{
		public Vector2 Position { get; set; }
		public int Animation { get; set; }
		public bool IsLookingRight { get; set; }
	}
}
