using System;
using System.Collections.Generic;

namespace Assets.Scripts.Behaviour.Data.Nodes
{

	[Serializable]
    public class DialogNodeData : BaseNodeData
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public List<ChoiceData> Choices { get; set; } = new List<ChoiceData>();
    }
}