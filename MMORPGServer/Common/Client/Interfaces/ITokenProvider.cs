using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Client.Interfaces
{
	public interface ITokenProvider
	{
		string Token { get; set; }
	}
}
