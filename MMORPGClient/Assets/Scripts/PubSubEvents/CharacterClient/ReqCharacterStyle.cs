using System.Collections.Generic;

namespace Assets.Scripts.PubSubEvents.CharacterClient
{
	public class ReqCharacterStyle
	{
		public List<string> Names = new List<string>();

		public ReqCharacterStyle()
		{

		}

		public ReqCharacterStyle(string name)
		{
			Names.Add(name);
		}
	}
}
