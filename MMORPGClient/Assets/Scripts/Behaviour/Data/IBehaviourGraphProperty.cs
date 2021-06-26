using System;

namespace Assets.Scripts.Behaviour.Data
{
	public interface IBehaviourGraphProperty
    {
        Guid Guid { get; set; }
        string Name { get; set; }

        Type Type { get; }
    }
}