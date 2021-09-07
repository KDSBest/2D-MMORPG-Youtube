using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SubScreen
{
	public class InventoryChangeEntry : MonoBehaviour
	{
		public TMP_Text ValueText;
		public Image Image;

		public void DestryMyself()
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
