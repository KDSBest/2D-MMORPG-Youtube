﻿using Common.Client.Interfaces;
using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client
{

	public class LoginClient : BaseClient<LoginWorkflow>, ILoginClient
	{

	}
}
