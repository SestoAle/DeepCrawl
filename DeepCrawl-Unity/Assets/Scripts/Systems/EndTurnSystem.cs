using UnityEngine;
using System.Collections;
using Unity.Entities;

public class EndTurnSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObjects;
    public ComponentDataArray<EndTurn> EndTurns;
    public ComponentDataArray<Turn> Turns;

    public SubtractiveComponent<Damage> Damages;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    for (int i = 0; i < data.Length; i++)
    {
      // End turn of this charcater
      var turn = data.Turns[i];
      if (turn.hasTurn == 1)
      {
        turn.hasEndedTurn = 1;
        data.Turns[i] = turn;
      }
      // Remove EndTurn component
      puc.RemoveComponent<EndTurn>(data.Entity[i]);
    }
  }
}
