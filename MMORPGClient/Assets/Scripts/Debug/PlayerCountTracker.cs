using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.Character;

public class PlayerCountTracker : MonoBehaviour
{
    public TMP_Text DebugText;
    public RemotePlayerManagement PlayerManagement;

    public void Update()
    {
        DebugText.text = $"{PlayerManagement.PlayerCount}"; 
    }
}
