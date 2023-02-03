using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ResourceType
{
    Wood,
    Water,
    Rock
};

public class ResourceTiledGameObject : TiledGameObject
{

    public ResourceType resourceType;
    public int amountToGive = 1;

    public int totalAmount = 1;

    public bool hasLimit = true;

    private int currentAmount = 0;

    void Start()
    {
        currentAmount = totalAmount;
    }

    public override void Interact(PlayerController player)
    {
        player.Give(resourceType, amountToGive);

        currentAmount -= amountToGive;
        if (hasLimit && currentAmount <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
