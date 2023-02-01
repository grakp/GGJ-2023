using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public float health;
    public Vector2 dir = new Vector2(0, 1); 
    public Weapon weapon;
    public Ability[] abilities;

    protected Dictionary<string, Ability> _abilitiesMap;

    public void SetDirection(Vector2 newDir) {
        dir = newDir;
    }

    public bool UseAbility(string name) {
        if(_abilitiesMap.ContainsKey(name)) {
            _abilitiesMap[name].Trigger(dir);
            return true;
        }

        return false;
    }

    void Start() {        
        _abilitiesMap = new Dictionary<string, Ability>();
        foreach(Ability ability in abilities) {
            _abilitiesMap.Add(ability.abilityName, ability);
        }
        weapon = GetComponentInChildren<Weapon>();
        StartCoroutine(DestroyOnDeath());
    }

    IEnumerator DestroyOnDeath() {
        while(true) {
            if(health <= 0) {
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // TODO (Trigger some death animations)
        //
        Destroy(gameObject);
    }
}
