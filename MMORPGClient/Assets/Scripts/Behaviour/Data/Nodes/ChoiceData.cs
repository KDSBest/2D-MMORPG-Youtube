using System;

namespace Assets.Scripts.Behaviour.Data.Nodes
{
	[Serializable]
    public class ChoiceData
	{
        public string Text { get; set; }
        public Guid GuidNext { get; set; }
	}
}