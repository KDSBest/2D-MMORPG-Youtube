using System;
using System.Collections.Generic;
using System.Numerics;

namespace Assets.Scripts.Behaviour.Data
{
	[Serializable]
    public class DialogNodeData
    {
        public Guid Guid { get; set; }
        public string Text { get; set; }
        public List<ChoiceData> Choices { get; set; } = new List<ChoiceData>();
        public Vector2 Position;
    }
}