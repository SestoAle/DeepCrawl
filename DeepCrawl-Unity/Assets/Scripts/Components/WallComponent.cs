using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

[Serializable]
public struct Wall : IComponentData
{
  public int type;
}

public class WallComponent : ComponentDataWrapper<Wall> { }