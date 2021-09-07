using TMPro;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class DamageDisplay : MonoBehaviour
	{
		public TMP_Text Text;

		public void SetDamage(int damage)
		{
			Text.text = damage.ToString();
		}
	}
}
