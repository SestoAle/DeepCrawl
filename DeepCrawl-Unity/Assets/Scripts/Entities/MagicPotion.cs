using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MagicPotion : Potion
{
    public int mp;
    
    // Return the component attach to it
    public override IComponentData getComponent()
    {
        return new MagicPoint { mp = mp };
    }
}
