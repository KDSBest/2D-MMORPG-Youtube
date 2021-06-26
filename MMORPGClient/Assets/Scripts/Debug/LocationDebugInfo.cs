using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Assets.Scripts.Debug
{
    public class LocationDebugInfo : MonoBehaviour
    {
        public TMP_Text DebugText;
        public Transform TrackTransform;

        public void Update()
        {
            DebugText.text = $"({TrackTransform.position.x.ToString("0.00")}, {TrackTransform.position.y.ToString("0.00")}, {TrackTransform.position.z.ToString("0.00")})";
        }
    }
}