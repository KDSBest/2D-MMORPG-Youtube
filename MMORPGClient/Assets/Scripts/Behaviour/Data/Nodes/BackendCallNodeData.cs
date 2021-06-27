using System;

namespace Assets.Scripts.Behaviour.Data.Nodes
{
	[Serializable]
    public class BackendCallNodeData : BaseNodeData
    {
        public string Call { get; set; }

        public Guid NextTrue { get; set; }
        public Guid NextFalse { get; set; }
    }
}