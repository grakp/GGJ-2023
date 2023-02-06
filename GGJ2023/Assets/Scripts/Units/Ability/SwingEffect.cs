using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SwingEffect : Effect
{
    [Tooltip("Sync with animation duration")]
    public float durationSec;

    // Weapon the instigator is holding
    private Weapon _wep;

    // List of already handled targets
    private HashSet<int> _triggeredObjects;

    bool hasInitialized = false;

    public override void Initialize(GameObject instigator, Vector3 direction) {
        hasInitialized = true;
        _triggeredObjects = new HashSet<int>();
        this.instigator = instigator;
        _wep = instigator.GetComponentInChildren<Weapon>();

        transform.SetParent(instigator.transform);
        transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), direction);
        GetComponentInChildren<SpriteRenderer>().sprite = GetWeaponFromGameObject(instigator).sprite;        
        gameObject.SetActive(true);
    }

    int GetDamage() {
        return _wep.attackStat;
    }

    void Start() {
        // NOTE: Set Active can potentially be called after a trigger
        StartCoroutine(HandleFinishEffect());
    }

    IEnumerator HandleFinishEffect() {
        yield return new WaitForSeconds(durationSec);
        Destroy(gameObject);
    }

    public override void TriggerEntered(Collider2D other) {

        if (!GameManager.Instance.networkingManager.IsDebuggingMode && !PhotonNetwork.IsMasterClient)
        {
            // All effects are to be handled by the server and then replicated.
            return;
        }

        GameObject possibleEnemy = other.gameObject;
        if (possibleEnemy == null)
        {
            return;
        }

        if (instigator == null)
        {
            return;
        }

        // if (possibleEnemy == instigator)
        // {
        //     Debug.LogWarning("A" + GetInstanceID());
        //     return;
        // }

        if(_triggeredObjects.Contains(possibleEnemy.GetInstanceID())) {
            return;
        }
        _triggeredObjects.Add(other.gameObject.GetInstanceID());

        BaseUnit instigatorUnit = instigator.GetComponent<BaseUnit>();
        if (instigatorUnit == null)
        {
            return;
        }

        BaseUnit enemyUnit = possibleEnemy.GetComponent<BaseUnit>();

        if (enemyUnit == null)
        {
            return;
        }

        if (_wep == null)
        {
            return;
        }

        if(enemyUnit != null && enemyUnit.unitType != instigatorUnit.unitType) {            
            enemyUnit.RequestTakeDamage(GetDamage(), instigatorUnit);
        }
    }

}
