using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public ShopkeeperUIController shopkeeperUIController;
    
    public TMP_Text playerHealthText;
    public TMP_Text playerAttackText;

    public TMP_Text woodResourceText;
    public TMP_Text waterResourceText;
    public TMP_Text stoneResourceText;

    private PlayerController player;

    private BaseUnit playerBaseUnit;

    public TMP_Text targetHealthText;
    public float showTargetTime = 3.0f;

    private float currentShowTargetTime = 0.0f;
    private BaseUnit cachedTarget = null;

    private bool isShowingTargetHealth = false;
    private Weapon weapon;

    // Start is called before the first frame update
    void Start()
    {
        targetHealthText.gameObject.SetActive(false);
        player = GameManager.Instance.gameController.GetMyPlayer();
        if (player == null)
        {
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            Initialize();
        }

    }

    void Initialize()
    {
        player = GameManager.Instance.gameController.GetMyPlayer();
        playerBaseUnit = player.GetComponent<BaseUnit>();
        weapon = player.GetComponent<Weapon>();
        UpdatePlayerStats();

        UpdateResourceText();
    }

    public void UpdatePlayerStats()
    {
        SetPlayerHealth(playerBaseUnit.health);
        SetPlayerAtack(weapon.attackStat);
    }

    public void UpdateResourceText()
    {
        woodResourceText.text = player.amountWood.ToString();
        waterResourceText.text = player.amountWater.ToString();
        stoneResourceText.text = player.amountRock.ToString();
    }

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance.gameController.GetMyPlayer() == null)
        {
            yield return null;
        }

        Initialize();
    }

    private void SetPlayerHealth(int health)
    {
        playerHealthText.text = "HP: " + health;
    }

    private void SetPlayerAtack(int attack)
    {
        playerAttackText.text = "+ " + attack + " ATK";
    }

    public void ShowTargetHealth(BaseUnit target)
    {
        cachedTarget = target;
        currentShowTargetTime = 0.0f;
        targetHealthText.gameObject.SetActive(true);
        if (isShowingTargetHealth)
        {
            // do nothing else
        }
        else
        {
            StartCoroutine(ShowTarget());
        }
    }

    private void UpdateTargetHealth()
    {
        if (cachedTarget == null)
        {
            return;
        }

        targetHealthText.text = cachedTarget.name + " " + "HP: " + cachedTarget.health;
    }

    private IEnumerator ShowTarget()
    {
        isShowingTargetHealth = true;
        while (cachedTarget != null && currentShowTargetTime < showTargetTime)
        {
            UpdateTargetHealth();
            currentShowTargetTime += Time.deltaTime;
            yield return null;
        }

        currentShowTargetTime = 0.0f;
        targetHealthText.gameObject.SetActive(false);
        isShowingTargetHealth = false;

    }

    
}
