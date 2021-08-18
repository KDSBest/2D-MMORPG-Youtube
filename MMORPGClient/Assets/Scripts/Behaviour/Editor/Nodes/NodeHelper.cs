using Assets.Scripts.Behaviour.Editor.Nodes.Interfaces;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{
	public static class NodeHelper
	{
		public static void SetNodeText(ITextfieldNode node, string text)
		{
			node.Text = text;
			node.title = text.Split('\r', '\n')[0];
			node.TextField.SetValueWithoutNotify(text);
		}

		public static void SetNodeText(ITextfieldNode node, string text, string titlePrefix)
		{
			SetNodeText(node, text);
			node.title = titlePrefix + node.title;
		}
	}
}
