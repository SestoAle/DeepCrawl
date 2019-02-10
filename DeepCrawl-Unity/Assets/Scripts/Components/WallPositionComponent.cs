using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct WallPosition : IComponentData
{
  public int x;
  public int y;
}

public class WallPositionComponent : ComponentDataWrapper<WallPosition> { }