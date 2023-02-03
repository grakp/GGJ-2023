using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemStat 
{
    Attack,
    Health
};

[System.Serializable]
public class ShopPoolItemParams
{
    public int minCost = 5;
    public int maxCost = 50;
    public List<Sprite> possibleSprites;
    public ItemStat stat;

    public int baseStatPerCost = 1;
    public int statVariance = 20;
};

[System.Serializable]
public class ShopPoolParams
{
    public List<Sprite> possibleShopKeepers;

    public List<ShopPoolItemParams> possibleItemTypes;
};

public class ShopItemInstance
{
    public int magnifier;
    public ItemStat stat;
    public Sprite itemSprite;
    public string description;

    public int woodCost;
    public int waterCost;
    public int rockCost;
}

public class ShopInstance
{
    public Sprite shopkeeperSprite;

    public List<ShopItemInstance> items = new List<ShopItemInstance>();
};

public class StaticResourceManager : MonoBehaviour
{

    public ShopPoolParams shopPoolParams;


    public ShopInstance CreateNewShopInstance(int numItems)
    {
        ShopInstance shopInstance = new ShopInstance();

        int rand = Random.Range(0, shopPoolParams.possibleShopKeepers.Count);
        shopInstance.shopkeeperSprite = shopPoolParams.possibleShopKeepers[rand];

        for (int i = 0; i < numItems; i++)
        {
            rand = Random.Range(0, shopPoolParams.possibleItemTypes.Count);
            ShopPoolItemParams itemParams = shopPoolParams.possibleItemTypes[rand];

            ShopItemInstance newItem = new ShopItemInstance();

            int totalCost = Random.Range(itemParams.minCost, itemParams.maxCost);
            for (int j = 0; j < totalCost; j++)
            {
                rand = Random.Range(0, 3);
                if (rand == 0)
                {
                    // Wood
                    newItem.woodCost++;
                }
                else if (rand == 1)
                {
                    // water
                    newItem.waterCost++;
                }
                else
                {
                    // Rock
                    newItem.rockCost++;
                }
            }

            int magnifier = Mathf.Max(totalCost * itemParams.baseStatPerCost + Random.Range(-itemParams.statVariance, itemParams.statVariance), 1);
            newItem.magnifier = magnifier;
            newItem.stat = itemParams.stat;

            rand = Random.Range(0, itemParams.possibleSprites.Count);
            newItem.itemSprite = itemParams.possibleSprites[rand];

            string statName = "";
            if (newItem.stat == ItemStat.Attack)
            {
                statName = "Attack";
            }
            else if (newItem.stat == ItemStat.Health)
            {
                statName = "Health";
            }

            newItem.description = string.Format("+ {0} {1}", newItem.magnifier, statName);

            shopInstance.items.Add(newItem);
        }


        return shopInstance;
    }

}
