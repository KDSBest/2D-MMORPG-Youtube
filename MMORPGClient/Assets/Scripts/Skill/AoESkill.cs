using Common.GameDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
	public class AoESkill : MonoBehaviour
	{
		public AoESkillRenderer IndicatorRenderer;
		public List<AoESkillRenderer> SkillRenderer;

		public Vector2 WorldSize = new Vector2(24, 24);
		public SkillCastType Type;
		public float IndicatorDelay;
		public float RenderingDelay = 1.0f;

		public void Start()
		{
			IndicatorRenderer.WorldSize = WorldSize;
			foreach(var renderer in SkillRenderer)
			{
				renderer.WorldSize = WorldSize;
			}
		}

		public void Update()
		{
			IndicatorDelay -= Time.deltaTime;

			if(IndicatorDelay <= 0)
			{
				IndicatorRenderer.gameObject.SetActive(false);
				foreach (var renderer in SkillRenderer)
				{
					renderer.gameObject.SetActive(true);
				}

				RenderingDelay -= Time.deltaTime;

				if(RenderingDelay <= 0)
				{
					GameObject.Destroy(this.gameObject);
				}
			}
		}
	}
}