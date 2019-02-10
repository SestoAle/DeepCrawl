using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Position : IComponentData
{
  public int x;
  public int y;
}

public class PositionComponent : ComponentDataWrapper<Position> { }
