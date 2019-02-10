using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameOverSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public ComponentDataArray<Death> Deaths;
    public ComponentDataArray<Turn> Turns;
    public ComponentDataArray<Player> Player;
    public ComponentDataArray<Death> Death;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {

  }
}
