using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor.Nodes.Interfaces
{
	public interface ITextfieldNode
	{
		string Text { get; set; }
		string title { get; set; }
		TextField TextField { get; set;  }
	}
}
