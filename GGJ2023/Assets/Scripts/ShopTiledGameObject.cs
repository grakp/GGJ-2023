using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


public class ShopItemState
{
    public ShopItemInstance item;

    public bool soldOut = false;


}; 

public class ShopTiledGameObject : TiledGameObject
{
    public int numItems = 3; // only support 3 for now
    public List<ShopItemState> itemStates {get; set;}
    public ShopInstance shopInstance{get; set;}

    public PlayerController shopOpener = null;

    public override void Initialize(TileInfo tileInfo)
    {
        base.Initialize(tileInfo);

        itemStates = new List<ShopItemState>();

        shopInstance = GameManager.Instance.resourceManager.CreateNewShopInstance(numItems);

        for (int i = 0; i < numItems; i++)
        {
            ShopItemState newState = new ShopItemState();
            newState.item = shopInstance.items[i];
            newState.soldOut = false;
            itemStates.Add(newState);
        }

    }

    public override void DoInteract(PlayerController player)
    {
        if (!player.photonView.IsMine && !GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            return;
        }

        shopOpener = player;
        
        ShopkeeperUIParams shopParams = ConvertToUIParams();
        GetShopController().OpenShop(shopParams);
    }
    
    private ShopkeeperUIParams ConvertToUIParams()
    {
        ShopkeeperUIParams result = new ShopkeeperUIParams();
        result.shopkeeperSprite = shopInstance.shopkeeperSprite;
        result.shopTile = this;

        for (int i = 0; i < numItems; i++)
        {
            ShopItemState state = itemStates[i];
            ShopkeeperUIItemParams itemParams = new ShopkeeperUIItemParams(i, state.item.woodCost, state.item.waterCost, state.item.rockCost, state.item.description, state.soldOut, state.item.itemSprite);
            result.itemParams.Add(itemParams);
        }

        return result;
    }

    public override void UnInteract()
    {
        if (GetShopController().IsTileUsingShop(this))
        {
            GetShopController().CloseShop();
        }
    }

    private ShopkeeperUIController GetShopController()
    {
        return GameManager.Instance.gameController.uiController.shopkeeperUIController;
    }

    public void SetSold(int index)
    {
        itemStates[index].soldOut = true;
    }

    public ShopItemInstance GetShopItemInstance(int index)
    {
        return itemStates[index].item;
    }

    public void BuyItemAtIndex(int index)
    {
        if (GameManager.Instance.networkingManager.IsDebuggingMode || PhotonNetwork.IsMasterClient)
        {
            DoBuyItemAtIndex(index, shopOpener);
        }

        ShopPurchasePacket packet = new ShopPurchasePacket();
        packet.actorNumber = shopOpener.actorNumber;
        packet.locationX = originTile.positionInArray.x;
        packet.locationY = originTile.positionInArray.y;
        packet.itemIndexToBuy = index;
        
        GameManager.Instance.networkingManager.SendRequestPacket(packet);
    }

    public void DoBuyItemAtIndex(int index, PlayerController buyer)
    {
        if (buyer == null)
        {
            return;
        }

        ShopItemInstance item = GetShopItemInstance(index);

        buyer.Take(item.woodCost, item.waterCost, item.rockCost);
        buyer.Give(item);
        SetSold(index);

        ShopkeeperUIController shopController = GetShopController();

        if (shopController.IsTileUsingShop(this))
        {
            shopController.SetSoldOut(index);
            shopController.UpdateItems();
        }
    }


}
