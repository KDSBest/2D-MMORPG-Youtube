using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
