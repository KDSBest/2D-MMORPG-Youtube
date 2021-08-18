using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{
	public class ConditionPort
	{
		public string Condition { get; set; } = "true";
		public Port Port { get; set; }
	}
}
