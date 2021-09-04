using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float Duration = 1000;
    public float ImpactDuration = 1000;
    public Transform Target;

    public List<GameObject> DeactiveOnImpact = new List<GameObject>();

    public List<GameObject> ActivateOnImpact = new List<GameObject>();

    public Transform Caster;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 direction;
    private float elapsedTime = 0;

    private void UpdateTargeting()
    {
        startPosition = Caster.position;
        endPosition = Target.position;
        direction = endPosition - startPosition;
    }

    public void Update()
    {
        UpdateTargeting();
        elapsedTime += Time.deltaTime * 1000;
        float percentToTarget = elapsedTime / Duration;
        if (percentToTarget >= 1)
        {
            ImpactDuration -= Time.deltaTime * 1000;

            DeactiveOnImpact.ForEach(x => x.SetActive(false));

            ActivateOnImpact.ForEach(x => x.SetActive(ImpactDuration > 0));
            percentToTarget = 1;
        }

        this.transform.position = startPosition + direction * percentToTarget;
    }
}
