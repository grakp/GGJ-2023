using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BaseUnit : MonoBehaviour
{
    public enum UnitType {
        PLAYER,
        ENEMY_BASIC,
        ENEMY_BOSS
    }

    public int health;
    public UnitType unitType;
    public Vector2 dir = new Vector2(0, 1); 
    public Weapon weapon;
    public Ability[] abilities;

    protected Dictionary<string, Ability> _abilitiesMap = new Dictionary<string, Ability>();
    protected Dictionary<string, bool> _abilityOnCooldownsMap = new Dictionary<string, bool>();

    private bool _isUsingAbility;

    public void SetDirection(Vector2 newDir) {
        dir = newDir;
    }

    public Ability GetAbilityWithName(string name)
    {
        if (!_abilitiesMap.ContainsKey(name))
        {
            return null;
        }

        return _abilitiesMap[name];
    }

    public bool UseAbility(string name) {

        // Probably should move this to replicate more stuff, but good enough for now
        PhotonView view = GetComponent<PhotonView>();
        if (!GameManager.Instance.networkingManager.IsDebuggingMode && view != null && !view.IsMine)
        {
            return false;
        }

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

    public void RequestTakeDamage(int damage, BaseUnit instigator)
    {
        // Only the server has authority to take damage
        if (!GameManager.Instance.networkingManager.IsDebuggingMode && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        DoTakeDamage(damage, instigator);

        PhotonView damagedView = GetComponent<PhotonView>();
        PhotonView instigatorPhotonView = instigator.GetComponent<PhotonView>();

        DealDamagePacket packet = new DealDamagePacket();
        packet.photonViewId = damagedView.ViewID;
        packet.instigatorViewId = instigatorPhotonView.ViewID;
        packet.damageDealt = damage;
        GameManager.Instance.networkingManager.SendRequestPacket(packet);
    }

    public void DoTakeDamage(int damage, BaseUnit instigator)
    {
        // TODO: Trigger damage anims?
        health = Mathf.Max(health - damage, 0);

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Update player health if it has been hit
            if (GameManager.Instance.networkingManager.IsDebuggingMode || (playerController.photonView != null && playerController.photonView.IsMine))
            {
                GameManager.Instance.gameController.uiController.UpdateHealth();
            }
        }

        AiControllerBase aiController = GetComponent<AiControllerBase>();
        if (aiController != null)
        {
            PlayerController instigatorPlayer = instigator.GetComponent<PlayerController>();
            if (GameManager.Instance.networkingManager.IsDebuggingMode || (instigatorPlayer != null && instigatorPlayer.photonView.IsMine))
            {
                GameManager.Instance.gameController.uiController.ShowTargetHealth(this);
            }
        }
    }

    public void RequestGiveStat(ItemStat stat, int amount)
    {
        if (GameManager.Instance.networkingManager.IsDebuggingMode || PhotonNetwork.IsMasterClient)
        {
            DoGiveStat(stat, amount);
        }

        PhotonView view = GetComponent<PhotonView>();
        if (view != null)
        {
            GiveStatPacket packet = new GiveStatPacket();
            packet.photonViewId = view.ViewID;
            packet.stat = (int)stat;
            packet.amount = amount;
            GameManager.Instance.networkingManager.SendRequestPacket(packet);
        }
    }

    public void DoGiveStat(ItemStat stat, int amount)
    {
        if (stat == ItemStat.Health)
        {
            health += amount;
        }
        else if (stat == ItemStat.Attack)
        {
            // Should probably be refactored, but whatever...
            Weapon weapon = GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.attackStat += amount;
            }
            else
            {
                Debug.LogError("Cannot find weapon!");
            }
        }

        PhotonView view = GetComponent<PhotonView>();
        if (GameManager.Instance.networkingManager.IsDebuggingMode || (view != null && view.IsMine))
        {
            GameManager.Instance.gameController.uiController.UpdateHealth();
        }
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
