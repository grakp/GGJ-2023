using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTrigger : MonoBehaviour
{
    public Effect effect;

    void OnTriggerEnter2D(Collider2D other) {
        effect.TriggerEntered(other);
    }
}
