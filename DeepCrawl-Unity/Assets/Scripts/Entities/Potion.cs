using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract class Potion : Item
{

  public int hp = 0;
  public int def = 0;
  public int atk = 0;
  public int duration = 0;

  public virtual List<Tile> tileAffected(Tile startTile)
  {
    List<Tile> tiles = new List<Tile>();
    tiles.Add(startTile);
    return tiles;
  }

  public abstract IComponentData getComponent();
}
