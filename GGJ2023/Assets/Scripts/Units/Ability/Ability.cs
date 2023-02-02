using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public string abilityName;
    public float cooldown;
    public GameObject gameplayEffectPrefab;

    public GameObject Trigger(Vector3 direction) {
        GameObject gameplayEffect = Instantiate(gameplayEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
        // Trigger the effect
        Effect e = gameplayEffect.gameObject.GetComponentInChildren<Effect>();
        e.Initialize(this.gameObject, direction);

        return gameplayEffect;
    }
}
