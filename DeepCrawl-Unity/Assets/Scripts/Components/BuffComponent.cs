using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Buff : IComponentData
{
  public int hp;
  public int def;
  public int atk;
  public int duration;
  public int turn;
}
