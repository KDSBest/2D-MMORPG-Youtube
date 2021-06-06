using System;
using UnityEngine;

public class RemotePlayerRenderer : MonoBehaviour
{
	public Transform Renderer;

	public void SetLooking(bool isRight)
	{
		float xScaleAbs = Math.Abs(Renderer.localScale.x);
		if (isRight)
			Renderer.localScale = new Vector3(xScaleAbs, Renderer.localScale.y, Renderer.localScale.z);
		else
			Renderer.localScale = new Vector3(-xScaleAbs, Renderer.localScale.y, Renderer.localScale.z);
	}
}
