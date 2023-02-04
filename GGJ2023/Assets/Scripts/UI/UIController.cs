using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public ShopkeeperUIController shopkeeperUIController;
    
    public Slider playerHealthSlider;
    public TMP_Text woodResourceText;
    public TMP_Text waterResourceText;
    public TMP_Text stoneResourceText;

    private PlayerController player;

    private BaseUnit playerBaseUnit;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.gameController.GetMyPlayer();
        playerBaseUnit = player.GetComponent<BaseUnit>();
        // Should probably have max health later...
        playerHealthSlider.maxValue = playerBaseUnit.health;

        UpdateResourceText();
    }

    public void UpdateHealth()
    {
        playerHealthSlider.value = playerBaseUnit.health;
    }

    public void UpdateResourceText()
    {
        woodResourceText.text = player.amountWood.ToString();
        waterResourceText.text = player.amountWater.ToString();
        stoneResourceText.text = player.amountRock.ToString();
    }
}
