using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopkeeperUIParams
{
    public List<ShopkeeperUIItemParams> itemParams = new List<ShopkeeperUIItemParams>();
    public Sprite shopkeeperSprite;
    public ShopTiledGameObject shopTile;
};

public class ShopkeeperUIController : MonoBehaviour
{
    public List<ShopkeeperUIItemOption> shopKeeperUIOptions;

    public Image shopkeeperImage;

    public RectTransform containerTranform;

    private ShopkeeperUIParams cachedShopParams;

    public bool isShopOpened{get; private set;}

    public void OpenShop(ShopkeeperUIParams shopParams)
    {
        isShopOpened = true;
        cachedShopParams = shopParams;
        containerTranform.gameObject.SetActive(true);
        Initialize(shopParams);
    }

    public void CloseShop()
    {
        isShopOpened = false;
        containerTranform.gameObject.SetActive(false);
    }

    private void Initialize(ShopkeeperUIParams shopParams)
    {
        shopkeeperImage.sprite = shopParams.shopkeeperSprite;
        for (int i = 0; i < shopKeeperUIOptions.Count; i++)
        {
            if (i >= shopParams.itemParams.Count)
            {
                break;
            }

            shopKeeperUIOptions[i].Initialize(shopParams.itemParams[i], this);
        }
    }

    public bool IsTileUsingShop(ShopTiledGameObject tileObject)
    {
        return isShopOpened && cachedShopParams != null && cachedShopParams.shopTile == tileObject;
    }

    public ShopTiledGameObject GetTileObject()
    {
        return cachedShopParams.shopTile;
    }

    public void UpdateItems()
    {
        // I think we only need to update affordability for now..
        for (int i = 0; i < shopKeeperUIOptions.Count; i++)
        {
            shopKeeperUIOptions[i].UpdateAffordability();
        }
    }
}
