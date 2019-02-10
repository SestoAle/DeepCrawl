using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Fadeble : IComponentData
{
  public int isFade;
}

public class FadebleComponent : ComponentDataWrapper<Fadeble> { }
