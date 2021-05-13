using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapService.WorldManagement
{

	public interface IPlayerWorldManagement
	{
		void Initialize();
		void OnDisconnectedPlayer(string name);
	}
}
