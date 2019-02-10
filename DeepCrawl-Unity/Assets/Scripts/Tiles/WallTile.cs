using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTile : Tile
{
  // Change the defaultCanMove for wall tile to false
  public WallTile()
  {
    this.defaultCanMove = false;
  }
}
