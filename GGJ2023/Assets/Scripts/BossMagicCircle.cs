using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMagicCircle : MonoBehaviour
{
    public HashSet<BaseUnit> collidedUnits = new HashSet<BaseUnit>();

    void OnTriggerEnter2D(Collider2D other)
    {
        BaseUnit unit = other.gameObject.GetComponent<BaseUnit>();
        if (unit != null)
        {
            if (unit.GetComponent<PlayerController>() != null)
            {
                collidedUnits.Add(unit);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        BaseUnit unit = other.gameObject.GetComponent<BaseUnit>();
        if (unit != null)
        {
            if (collidedUnits.Contains(unit))
            {
                collidedUnits.Remove(unit);
            }
        }
    }
}
