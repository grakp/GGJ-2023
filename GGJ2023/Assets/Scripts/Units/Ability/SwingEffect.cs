using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingEffect : Effect
{
    [Tooltip("Sync with animation duration")]
    public float durationSec;

    // Weapon the instigator is holding
    private Weapon _wep;

    public override void Initialize(GameObject instigator, Vector3 direction) {
        transform.SetParent(instigator.transform);
        transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
        GetComponentInChildren<SpriteRenderer>().sprite = GetWeaponFromGameObject(instigator).sprite;        
        gameObject.SetActive(true);
    }

    void Start() {
        StartCoroutine(HandleFinishEffect());
    }

    IEnumerator HandleFinishEffect() {
        yield return new WaitForSeconds(durationSec);
        Destroy(gameObject);
    }
}
