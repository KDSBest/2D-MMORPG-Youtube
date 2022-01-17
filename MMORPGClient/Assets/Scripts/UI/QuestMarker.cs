using Assets.Scripts;
using Assets.Scripts.Character;
using Common;
using Common.IoC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class QuestMarker : MonoBehaviour
{
    private Image uiImage;
    public List<string> TrackedQuests;
    public Sprite QuestFinish;
    public Sprite QuestAvailable;
    public Sprite QuestRunning;
    public Sprite QuestAvailableButNotHighEnough;
    private ICurrentContext context;

	public void Awake()
    {
        this.uiImage = this.GetComponent<Image>();
        DILoader.Initialize();
        context = DI.Instance.Resolve<ICurrentContext>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (context.QuestTracking == null || context.QuestTracking.Inventory == null)
            return;

        uiImage.enabled = true;

        foreach (var q in TrackedQuests)
		{
            if(!QuestLoader.Quests.ContainsKey(q))
			{
                continue;
			}

            if (!context.QuestTracking.AcceptedQuests.Contains(q))
            {
                continue;
            }

            if(QuestLoader.Quests[q].Task.IsFinished(q, context.QuestTracking))
			{
                uiImage.sprite = QuestFinish;
                return;
            }
		}

        foreach (var q in TrackedQuests)
        {
            if (!QuestLoader.Quests.ContainsKey(q))
            {
                continue;
            }

            if (context.QuestTracking.AcceptedQuests.Contains(q))
            {
                continue;
            }

            if (QuestLoader.Quests[q].IsAvailable(context.QuestTracking, context.Character.Level))
            {
                uiImage.sprite = QuestAvailable;
                return;
            }
        }

        foreach (var q in TrackedQuests)
        {
            if (!QuestLoader.Quests.ContainsKey(q))
            {
                continue;
            }

            if (context.QuestTracking.AcceptedQuests.Contains(q))
            {
                uiImage.sprite = QuestRunning;
                return;
            }
        }

        foreach (var q in TrackedQuests)
        {
            if (!QuestLoader.Quests.ContainsKey(q))
            {
                continue;
            }

            if (context.QuestTracking.AcceptedQuests.Contains(q))
            {
                continue;
            }

            if (context.QuestTracking.FinishedQuests.Contains(q))
            {
                continue;
            }

            uiImage.sprite = QuestAvailableButNotHighEnough;
            return;
        }

        uiImage.sprite = null;
        uiImage.enabled = false;
    }
}
