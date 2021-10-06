using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.SubScreen
{
	public class AutoHideInfoController : MonoBehaviour
	{
		public TMP_Text Text;
		private Coroutine currentCoroutine;

		public void Show(string text, float durationInSeconds)
		{
			Text.text = text;
			this.gameObject.SetActive(true);

			if (currentCoroutine != null)
				this.StopCoroutine(currentCoroutine);

			currentCoroutine = this.StartCoroutine(DeactivateMyselfAfter(durationInSeconds));
		}

		private IEnumerator DeactivateMyselfAfter(float durationInSeconds)
		{
			yield return new WaitForSeconds(durationInSeconds);
			this.gameObject.SetActive(false);
		}
	}
}
