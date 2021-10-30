using Assets.Scripts.GameDesign;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
	[CreateAssetMenu(fileName = "NewItem", menuName = "KDSBest/CreateItem")]
    public class ItemData : ScriptableObject
    {
        public string DisplayName;

        [Multiline(10)]
        public string TooltipContent;

        public Rarity Rarity;
    }
}
