using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
	public class Fireball : MonoBehaviour
	{
		private float duration = 1000;
		public float ImpactDuration = 1000;
		public Vector3 Target;

		public List<GameObject> DeactiveOnImpact = new List<GameObject>();

		public List<GameObject> ActivateOnImpact = new List<GameObject>();

		public Vector3 Caster;

		private Vector3 direction;
		private float elapsedTime = 0;

		private void UpdateTargeting()
		{
			direction = Target - Caster;
		}

		public void Start()
		{
			this.transform.position = Caster;
		}

		public void Update()
		{
			UpdateTargeting();
			elapsedTime += Time.deltaTime * 1000;
			float percentToTarget = elapsedTime / duration;
			if (percentToTarget >= 1)
			{
				ImpactDuration -= Time.deltaTime * 1000;

				DeactiveOnImpact.ForEach(x => x.SetActive(false));

				ActivateOnImpact.ForEach(x => x.SetActive(ImpactDuration > 0));
				percentToTarget = 1;

				if (ImpactDuration <= 0)
					GameObject.Destroy(this.gameObject);
			}

			this.transform.position = Caster + direction * percentToTarget;
		}
	}
}