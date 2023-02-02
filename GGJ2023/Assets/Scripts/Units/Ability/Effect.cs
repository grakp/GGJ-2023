using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    public GameObject instigator;
    public GameObject target;

    public abstract void Initialize(GameObject instigator, Vector3 direction);
    public abstract void TriggerEntered(Collider2D other);

    public Weapon GetWeaponFromGameObject(GameObject unit) {
        return unit.GetComponentInChildren<Weapon>();
    }
}
