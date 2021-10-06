using Common.GameDesign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SubScreen
{
	public class ExpBarController : MonoBehaviour
	{
		public Image Renderer;
		public TMP_Text ProgressText;
		private Material material;

		public void Awake()
		{
			material = Renderer.material;
		}

		public void UpdateExp(int currentMaxExp, int currentLevel)
		{
			int expNeededTillHere = currentLevel <= 1 ? 0 : ExpCurve.FullExp[currentLevel - 1];
			int expEarnedTillNextLevel = currentMaxExp - expNeededTillHere;
			int expNeededTillNextLevel = ExpCurve.DiffExp[currentLevel];

			if (expEarnedTillNextLevel < 0)
				expEarnedTillNextLevel = 0;

			ProgressText.text = $"{expEarnedTillNextLevel} / {expNeededTillNextLevel}";

			float percentBar = (float)expEarnedTillNextLevel / (float)expNeededTillNextLevel;
			int expBarValue = (int)(percentBar * 1000.0f);

			if (expBarValue > 1000)
				expBarValue = 1000;

			material.SetInt("Health", expBarValue);
		}
	}
}
