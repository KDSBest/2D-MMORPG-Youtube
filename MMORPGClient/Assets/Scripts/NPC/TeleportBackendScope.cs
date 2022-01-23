using Assets.Scripts.Character;
using Common;
using Common.IoC;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.NPC
{
	public class TeleportBackendScope : IVMScope
	{
		private readonly IPubSub pubsub;
		private readonly ICurrentContext context;

		public string Scope => "Teleport";

		public TeleportBackendScope()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			context = DI.Instance.Resolve<ICurrentContext>();
		}

		public VMVar Execute(string functionName, List<VMVar> parameters)
		{
			switch (functionName)
			{
				case "To":
					return TeleportTo(parameters[0].ValueString);
			}

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 0
			};
		}



		private VMVar TeleportTo(string teleportname)
		{
			if (!context.NPC.ContainsKey(teleportname))
			{
				return new VMVar()
				{
					Type = VMType.Number,
					ValueNumber = 0
				};
			}

			context.PlayerController.SetForcePosition(context.NPC[teleportname].transform.position);

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			};
		}
	}
}
