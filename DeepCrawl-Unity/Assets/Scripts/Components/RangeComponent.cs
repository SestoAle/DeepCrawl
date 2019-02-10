using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Range : IComponentData
{
  public int actualRange;
}

public class RangeComponent : ComponentDataWrapper<Range> { }
