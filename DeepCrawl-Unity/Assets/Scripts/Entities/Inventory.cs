using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Inventory : MonoBehaviour
{
  public MeleeWeapon meeleWeapon;
  public RangeWeapon rangeWeapon;
  public Potion potion;

  public void setMelee(MeleeWeapon item)
  {
    this.meeleWeapon = item;
  }

  public void setRange(RangeWeapon item)
  {
    this.rangeWeapon = item;
  }

  public void setPotion(Potion item)
  {
    if (item == null)
    {
      this.potion = null;
    }
    else
    {
      this.potion = item;
    }
  }
}
