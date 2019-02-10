using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class HealthPotion : Potion
{
  // Return the component attach to it
  public override IComponentData getComponent()
  {
    return new Damage { damage = -hp };
  }
}
