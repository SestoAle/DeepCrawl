using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Altar : IComponentData
{
  public int startingPoints;
  public int actualPoints;
  public int modAtk;
  public int modDef;
  public int modDes;
  public int modHp;
}

public class AltarComponent : ComponentDataWrapper<Altar> { }