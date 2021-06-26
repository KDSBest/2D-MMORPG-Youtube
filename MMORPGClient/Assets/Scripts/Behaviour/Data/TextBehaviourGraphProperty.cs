using System;

namespace Assets.Scripts.Behaviour.Data
{

	[Serializable]
	public class TextBehaviourGraphProperty : IBehaviourGraphProperty
	{
		public Guid Guid { get; set; }
		public Type Type
		{
			get
			{
				return typeof(string);
			}
		}

		public string Name { get; set; }
		public string DefaultValue { get; set; }
	}
}