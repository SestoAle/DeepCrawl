using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Rotation : IComponentData
{
  public int rotationX;
  public int rotationY;
  public int rotationZ;
}
