using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public string abilityName;
    public float cooldown;
    public GameObject gameplayEffectPrefab;

    public GameObject Trigger(Vector3 direction) {
        GameObject gameplayEffect = null;
        
        if (GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            gameplayEffect = Instantiate(gameplayEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
            // Trigger the effect
            Effect e = gameplayEffect.gameObject.GetComponentInChildren<Effect>();
            e.Initialize(this.gameObject, direction);
        }
        else
        {
            // Need photon view to get instance of instigator
            PhotonView view = GetComponent<PhotonView>();
            if (view == null)
            {
                Debug.LogError("Cannot find photon view!");
            }

            
            object[] customAbilityData = new object[]{ view.ViewID, abilityName, direction};
            gameplayEffect = NetworkingSingleton.NetworkInstantiate(gameplayEffectPrefab, this.gameObject.transform.position, Quaternion.identity, customAbilityData);
        }

        return gameplayEffect;
    }

}
