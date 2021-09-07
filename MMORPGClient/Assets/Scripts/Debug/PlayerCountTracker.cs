using UnityEngine;
using TMPro;
using Assets.Scripts.Remoting;

namespace Assets.Scripts.Debug
{

	public class PlayerCountTracker : MonoBehaviour
    {
        public TMP_Text DebugText;
        public RemotePlayerManagement PlayerManagement;

        public void Update()
        {
            DebugText.text = $"{PlayerManagement.PlayerCount}";
        }
    }
}