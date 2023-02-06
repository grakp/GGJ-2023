using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class Effect : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public GameObject instigator{get; set;}
    public GameObject target {get; set;}

    public abstract void Initialize(GameObject instigator, Vector3 direction);
    public abstract void TriggerEntered(Collider2D other);

    public Weapon GetWeaponFromGameObject(GameObject unit) {
        return unit.GetComponentInChildren<Weapon>();
    }

    public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // TODO make sure that self is/not included in list
        if (info.photonView.gameObject == null)
        {
            return;
        }

        object[] customData = info.photonView.InstantiationData;
        int viewId = (int)customData[0];
        System.String abilityName = (System.String)customData[1];
        Vector3 direction = (Vector3)customData[2];

        PhotonView view = PhotonView.Find(viewId);
        if (view == null)
        {
            Debug.LogWarning("Unable to find view! (Maybe the object died?)");
            return;
        }

        BaseUnit baseUnit = view.GetComponent<BaseUnit>();
        Ability ability = baseUnit.GetAbilityWithName(abilityName);

        Initialize(ability.gameObject, direction);
    }

    public virtual void DoOnPhotonInstantiate(PhotonMessageInfo info)
    {

    }
}
