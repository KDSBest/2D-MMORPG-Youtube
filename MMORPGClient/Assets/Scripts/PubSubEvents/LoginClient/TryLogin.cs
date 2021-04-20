using Common.Protocol.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.PubSubEvents.LoginClient
{
	public class TryLogin
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
	
}
