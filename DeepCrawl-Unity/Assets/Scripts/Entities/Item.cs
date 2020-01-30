using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Item : GameObjectEntity
{

    public int id;
    public int x;
    public int y;
    public float spawnProbability;
    public string itemName;
    public string damageString;

    public bool isGrounded = true;

    public Item createCopy()
    {
        return (Item)this.MemberwiseClone();
    }
}
