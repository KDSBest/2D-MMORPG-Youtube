using Assets.Scripts.Behaviour.Data.Nodes;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{
	public class CommentGroup : Group
	{
		public GroupData GetData()
		{
			var cData = new GroupData();
			cData.Title = this.title;
			var pos = this.GetPosition().position;
			cData.Position = new System.Numerics.Vector2(pos.x, pos.y);

			foreach (var child in this.containedElements.Where(x => x is BaseNode).Cast<BaseNode>())
			{
				cData.ChildNodes.Add(child.Guid);
			}

			return cData;
		}
	}
}
