using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.PubSubEvents.StartUI
{
	public class ControlLoadingScreen
	{
		public bool Visible { get; set; }
		public string CurrentAction { get; set; }
		public float Percentage { get; set; }
	}
}
