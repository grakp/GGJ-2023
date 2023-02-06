using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class AiControllerBase : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public abstract void OnPhotonInstantiate(PhotonMessageInfo info);

    public void FixUpName()
    {
        string oldName = this.name;
        oldName = oldName.Replace("(Clone)", "");
        gameObject.name = oldName;
    }
}
