using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct MovementElementBuffer : IBufferElementData
{
  public int x;
  public int y;

  public int rotation;
}
