using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class CharacterBehaviour : MonoBehaviour
	{
		public SpriteRenderer BodySprite;
		public List<GameObject> Eyes = new List<GameObject>();
		public List<Color> Colors = new List<Color>();
		public TMP_Text NameText;
		public TMP_Text LvlText;
		public SpriteRenderer HPRenderer;
		public CharacterBehaviourAffinity CharacterAffinity = CharacterBehaviourAffinity.RemotePlayer;
		private Material material;
		private IPubSub pubsub;
		private string charName;

		public void Awake()
		{
			material = HPRenderer.material;
		}

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<PlayerStateMessage>(this.GetType().Name + "_" + CharacterAffinity.ToString() + "_" + charName);
		}

		public void UpdateCharInfo(CharacterInformation charInfo)
		{
			charName = charInfo.Name;
			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, this.GetType().Name + "_" + CharacterAffinity.ToString() + "_" + charName);

			UpdateStats(charInfo.Stats);
			SetStyle(charInfo);
		}

		private void OnPlayerState(PlayerStateMessage msg)
		{
			if (msg.Name != charName)
				return;

			if(CharacterAffinity == CharacterBehaviourAffinity.LocalPlayer)
			{
				pubsub.Publish(new UpdateCharacterMessage()
				{
					Name = msg.Name,
					Stats = msg.Stats
				});
			}

			UpdateStats(msg.Stats);
		}

		public void UpdateStats(EntityStats stats)
		{
			LvlText.text = $"Lv. {stats.Level}";
			material.SetInt("Health", stats.HP);
			material.SetInt("MaxHealth", stats.MaxHP);
		}

		private void SetStyle(CharacterInformation charInfo)
		{
			if (charInfo.Color >= Colors.Count || charInfo.Color < 0)
			{
				charInfo.Color = 0;
			}

			if (charInfo.Eyes >= Eyes.Count || charInfo.Eyes < 0)
			{
				charInfo.Eyes = 0;
			}

			BodySprite.color = Colors[charInfo.Color];
			for (int i = 0; i < Eyes.Count; i++)
			{
				Eyes[i].SetActive(i == charInfo.Eyes);
			}

			if (NameText != null)
				NameText.text = charInfo.Name;
		}
	}
}