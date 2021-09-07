using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
	public class LightningBolt : MonoBehaviour
	{
		private float duration = 1500;
		private float elapsedTime = 0;

		public void Update()
		{
			elapsedTime += Time.deltaTime * 1000;
			if (elapsedTime > duration)
				GameObject.Destroy(this.gameObject);
		}
	}
}