using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class ShopkeeperUIItemParams
{
    public int index = 0;
    public int numWood;
    public int numWater;
    public int numRock;
    public string description;
    public bool soldOut;
    public Sprite itemSprite;

    public ShopkeeperUIItemParams(int index, int wood, int water, int rock, string desc, bool sold, Sprite sprite)
    {
        this.index = index;
        numWood = wood;
        numWater = water;
        numRock = rock;
        description = desc;
        soldOut = sold;
        itemSprite = sprite;
    }
};

public class ShopkeeperUIItemOption : MonoBehaviour
{
    public Button button;
    public TMP_Text woodText;
    public TMP_Text waterText;
    public TMP_Text rockText;

    public RectTransform woodTransform;
    public RectTransform waterTransform;
    public Transform rockTransform;

    public TMP_Text descriptionText;

    public RectTransform buyableTransform;
    public RectTransform soldoutTransform;

    
    public Image itemImage;

    private bool isSoldOut = false;

    private ShopkeeperUIItemParams cachedShopParams;
    private ShopkeeperUIController cachedParentController;

    private int index;

    public void Initialize(int index, ShopkeeperUIItemParams shopParams, ShopkeeperUIController parentController)
    {
        this.index = index;
        cachedShopParams = shopParams;
        cachedParentController = parentController;
        woodText.text = shopParams.numWood.ToString();
        waterText.text = shopParams.numWater.ToString();
        rockText.text = shopParams.numRock.ToString();

        if (shopParams.numWood == 0)
        {
            woodTransform.gameObject.SetActive(false);
        }

        if (shopParams.numWater == 0)
        {
            waterTransform.gameObject.SetActive(false);
        }

        if (shopParams.numRock == 0)
        {
            rockTransform.gameObject.SetActive(false);
        }

        descriptionText.text = shopParams.description;
        itemImage.sprite = shopParams.itemSprite;

        isSoldOut = shopParams.soldOut;
        if (shopParams.soldOut)
        {
            soldoutTransform.gameObject.SetActive(true);
            buyableTransform.gameObject.SetActive(false);
        }
        else
        {
            soldoutTransform.gameObject.SetActive(false);
            buyableTransform.gameObject.SetActive(true);
        }

        UpdateAffordability();
    }

    public void UpdateAffordability()
    {
        if (CanAfford())
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    public void SwitchToSoldOut()
    {
        isSoldOut = true;
        soldoutTransform.gameObject.SetActive(true);
        buyableTransform.gameObject.SetActive(false);
    }

    public void PlayerBuy()
    {
        if (CanAfford())
        {
            ShopTiledGameObject tileObject = cachedParentController.GetTileObject();
            tileObject.BuyItemAtIndex(index);
        }
    }

    private bool CanAfford()
    {
        PlayerController player = GetPlayerController();
        bool hasEnoughWood = cachedShopParams.numWood <= player.amountWood;
        bool hasEnoughWater = cachedShopParams.numWater <= player.amountWater;
        bool hasEnoughRock = cachedShopParams.numRock <= player.amountRock;
        return hasEnoughWood && hasEnoughWater && hasEnoughRock;
    }

    private PlayerController GetPlayerController()
    {
        return GameManager.Instance.gameController.GetMyPlayer();
    }
}
