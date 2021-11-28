using System;
using UnityEngine;
//using System.Numerics;

namespace Assets.Scripts.Behaviour.Data.Nodes
{
	[Serializable]
	public class BaseNodeData
	{
        public Guid Guid { get; set; }

        public System.Numerics.Vector2 Position;
    }
}