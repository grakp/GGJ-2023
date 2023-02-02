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
    protected Dictionary<string, bool> _abilityOnCooldownsMap;

    private bool _isUsingAbility;

    public void SetDirection(Vector2 newDir) {
        dir = newDir;
    }

    public bool UseAbility(string name) {
        if(_abilitiesMap.ContainsKey(name) && !_isUsingAbility) {
            if(!_abilityOnCooldownsMap[name]) {
                _isUsingAbility = true;
                _abilityOnCooldownsMap[name] = true;
                GameObject effect = _abilitiesMap[name].Trigger(dir);
                StartCoroutine(WaitForAbilityToComplete(effect));
                StartCoroutine(WaitForAbilityCooldown(name, _abilitiesMap[name].cooldown));
            }
            return true;
        }
        return false;
    }

    void Start() {
        _isUsingAbility = false;
        _abilitiesMap = new Dictionary<string, Ability>();
        _abilityOnCooldownsMap = new Dictionary<string, bool>();
        foreach(Ability ability in abilities) {
            _abilitiesMap.Add(ability.abilityName, ability);
            _abilityOnCooldownsMap.Add(ability.abilityName, false);
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

    IEnumerator WaitForAbilityToComplete(GameObject effect) {
        while(true) {
            if(effect == null) {
                _isUsingAbility = false;
                break;                
            }
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    IEnumerator WaitForAbilityCooldown(string abilityName, float cooldown) {
        yield return new WaitForSeconds(cooldown);
        _abilityOnCooldownsMap[abilityName] = false;
        yield return null;
    }
}
