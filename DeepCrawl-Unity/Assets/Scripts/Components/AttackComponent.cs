using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct Attack : IComponentData
{
  public int damage;
  public int attackTileX;
  public int attackTileY;
  public int type;
}
