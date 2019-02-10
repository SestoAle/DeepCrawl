using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RangeSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Stats> Stats;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    for (int i = 0; i < data.Length; i++)
    {
      // Get the inventory and the stats
      Inventory inventory = data.GameObject[i].GetComponent<Inventory>();
      Stats stats = data.Stats[i];
      // Compute the actual range of the character depending on its stats
      stats.actualRange = Mathf.Max(2, inventory.rangeWeapon.range + stats.des - 2);
      data.Stats[i] = stats;
    }
  }
}
