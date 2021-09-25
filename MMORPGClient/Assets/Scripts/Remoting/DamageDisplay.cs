using Common.Protocol.Combat;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class DamageDisplay : MonoBehaviour
	{
		public TMP_Text Text;
		public Color[] NonCritColors = new Color[4];
		public Color[] CritColors = new Color[4];

		public void SetDamage(DamageInfo damageInfo)
		{
			Text.text = damageInfo.Damage.ToString();
			Color[] colors = damageInfo.IsCrit ? CritColors : NonCritColors;
			Text.colorGradient = new VertexGradient(colors[0], colors[1], colors[2], colors[3]);
		}
	}
}
