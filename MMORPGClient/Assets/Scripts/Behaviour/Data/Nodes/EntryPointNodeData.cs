using System;
using System.Collections.Generic;

namespace Assets.Scripts.Behaviour.Data.Nodes
{

	[Serializable]
    public class EntryPointNodeData : BaseNodeData
    {
        public Guid Start { get; set; }
    }
}