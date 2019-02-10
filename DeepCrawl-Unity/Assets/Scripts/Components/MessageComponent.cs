using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct MessageComponent : ISharedComponentData
{
  public string text;
}