using Assets.Scripts;
using System;
using UnityEngine;

namespace Assets.Scripts.Character
{

	public class RemotePlayerRenderer : MonoBehaviour
	{
		public Transform MirrorTransform;
		public Animator Animator;

		public void SetAnimation(int animation)
		{
			Animator.SetInteger(Constants.AnimationStateName, animation);
		}

		public void SetLooking(bool isRight)
		{
			float xScaleAbs = Math.Abs(MirrorTransform.localScale.x);
			if (isRight)
				MirrorTransform.localScale = new Vector3(xScaleAbs, MirrorTransform.localScale.y, MirrorTransform.localScale.z);
			else
				MirrorTransform.localScale = new Vector3(-xScaleAbs, MirrorTransform.localScale.y, MirrorTransform.localScale.z);
		}
	}
}