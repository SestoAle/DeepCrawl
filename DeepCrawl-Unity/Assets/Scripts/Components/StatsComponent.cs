using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Stats : IComponentData
{
  public int maxHp;
  public int hp;
  public int maxMp;
  public int mp;
  public int def;
  public int atk;
  public int des;
  public int actualRange;
}

public class StatsComponent : ComponentDataWrapper<Stats> { }