using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.SubScreen
{
	public class InventoryChangeEntry : MonoBehaviour
	{
		public TMP_Text ValueText;

		public void DestryMyself()
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
