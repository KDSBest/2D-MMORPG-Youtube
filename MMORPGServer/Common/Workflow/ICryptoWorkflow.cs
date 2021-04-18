using Common.Crypto;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;

namespace Common.Workflow
{
	public interface ICryptoWorkflow : IWorkflow
	{
		CryptoProvider Crypto { get; set; }
	}
}
