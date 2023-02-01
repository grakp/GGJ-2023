using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingEffect : Effect
{
    public override void Initialize(GameObject instigator, Vector3 direction) {
        transform.SetParent(instigator.transform);
        transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
        gameObject.SetActive(true);
    }
}
