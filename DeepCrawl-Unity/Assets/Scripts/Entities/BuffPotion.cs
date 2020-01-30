using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BuffPotion : Potion
{
    // Return the component attach to it
    public override IComponentData getComponent()
    {
        return new Buff { hp = hp, def = def, duration = duration, atk = atk, turn = 0 };
    }
}
