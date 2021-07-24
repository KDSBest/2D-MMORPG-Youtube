using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.Redis.Model
{
	public class LoginQueueItem
	{
		public string PlayerId { get; set; }
		public DateTime LoginTime { get; set; }
	}
}
