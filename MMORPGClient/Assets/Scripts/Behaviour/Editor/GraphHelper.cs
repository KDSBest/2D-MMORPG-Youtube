using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{
	public static class GraphHelper
	{
		public static List<T> SearchForUserData<T>(VisualElement element) where T : class
		{
			List<T> result = new List<T>();

			if (element.userData is T)
			{
				result.Add(element.userData as T);
				return result;
			}

			var children = element.Children();

			foreach (var child in children)
			{
				result.AddRange(SearchForUserData<T>(child));
			}

			return result;
		}

	}
}
