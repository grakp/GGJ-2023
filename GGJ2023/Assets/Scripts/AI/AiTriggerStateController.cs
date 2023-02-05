using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class AiTriggerStateController : MonoBehaviour // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private AiController _aiController;
    private Dictionary<int, GameObject> _targetsInRange;

    // Start is called before the first frame update
    void Start()
    {
        _aiController = GetComponentInParent<AiController>();
        _targetsInRange = new Dictionary<int, GameObject>();
    }

    void Update() {

    }

    public GameObject GetClosestTargetInRange() {
        if(_targetsInRange.Count == 0) {
            return null;
        }

        float closestDistance = (_targetsInRange.Values.First().transform.position - transform.position).magnitude;
        GameObject closestObject = _targetsInRange.Values.First();
        foreach (KeyValuePair<int, GameObject> kv in _targetsInRange) {
            float distance = (kv.Value.transform.position - transform.position).magnitude;
            if(distance < closestDistance) {
                closestObject = kv.Value;
            }
        }
        return closestObject;
    }

    void OnTriggerEnter2D(Collider2D other) {
        GameObject potentialTarget = other.gameObject;
        if (other.gameObject == null)
        {
            return;
        }

        BaseUnit unit = potentialTarget.GetComponentInChildren<BaseUnit>();
        if (unit == null)
        {
            return;
        }

        if(unit.unitType == BaseUnit.UnitType.PLAYER) {
            _targetsInRange.Add(potentialTarget.GetInstanceID(), potentialTarget);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        GameObject potentialTarget = other.gameObject;
        if (potentialTarget == null)
        {
            return;
        }

        if(_targetsInRange.ContainsKey(potentialTarget.GetInstanceID())) {
            _targetsInRange.Remove(potentialTarget.GetInstanceID());
        }
    }
}
