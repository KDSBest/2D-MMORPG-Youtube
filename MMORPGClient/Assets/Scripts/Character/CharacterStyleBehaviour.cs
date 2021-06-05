using Common.Protocol.Character;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterStyleBehaviour : MonoBehaviour
{
    public SpriteRenderer BodySprite;
	public List<GameObject> Eyes = new List<GameObject>();
	public List<Color> Colors = new List<Color>();
	public TMP_Text Text;

    public void SetStyle(CharacterInformation charStyle)
	{
		if (charStyle.Color >= Colors.Count || charStyle.Color < 0)
		{
			charStyle.Color = 0;
		}

		if(charStyle.Eyes >= Eyes.Count || charStyle.Eyes < 0)
		{
			charStyle.Eyes = 0;
		}

		BodySprite.color = Colors[charStyle.Color];
		for(int i = 0; i < Eyes.Count; i++)
		{
			Eyes[i].SetActive(i == charStyle.Eyes);
		}

		if(Text != null)
			Text.text = charStyle.Name;
	}
}
