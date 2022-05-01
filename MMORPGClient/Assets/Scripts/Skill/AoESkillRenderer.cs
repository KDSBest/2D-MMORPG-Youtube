using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
	public class AoESkillRenderer : MonoBehaviour
	{
		public SpriteRenderer sr;

		public Vector2 WorldSize = new Vector2(24, 24);

		void Start()
		{
			Vector2 size = sr.bounds.extents * 2;

			this.transform.localScale = new Vector3(WorldSize.x / size.x, WorldSize.x / size.y, 1);

		}
	}
}