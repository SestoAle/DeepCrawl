using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TurnSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObjects;
    public ComponentDataArray<Turn> Turns;

    public SubtractiveComponent<Movement> Movements;
    public SubtractiveComponent<Attack> Attacks;
    public SubtractiveComponent<UserInput> UserInput;
    public SubtractiveComponent<EndTurn> EndTurns;
    public SubtractiveComponent<Damage> Damage;
    public SubtractiveComponent<DoneComponent> Done;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    // Number of the players
    int numPlayers = BoardManagerSystem.instance.totalPlayers;

    for (int i = 0; i < data.Length; i++)
    {
      var entity = data.Entity[i];
      var turn = data.Turns[i];
      Tile tile = BoardManagerSystem.instance.getTileFromObject(data.GameObjects[i]);

      // If the turn component has the currentIndex
      if (turn.index == BoardManagerSystem.instance.currentTurn)
      {
        if (EntityManager.HasComponent(data.Entity[i], typeof(Death)) && 
            data.GameObjects[i].GetComponent<BaseAgent>() != null && 
            BoardManagerSystem.instance.isTraning)
        {
          // Give the reward base on the hp lost in this turn
          data.GameObjects[i].GetComponent<BaseAgent>().giveHpReward();
          // When the agent is Done, it must RequestDecision
          data.GameObjects[i].GetComponent<BaseAgent>().RequestDecision();
          return;
        }
        // If the current Player has ended his turn or is dead or is not in the 
        // same room as the player
        if (turn.hasEndedTurn == 1 || EntityManager.HasComponent(entity, typeof(Death)) 
            || data.GameObjects[i].tag == "Puppet" || 
            (tile.parent.getId() != BoardManagerSystem.instance.currentRoomId && 
             !BoardManagerSystem.instance.noAnim))
        {
          // Update the current player turn component
          turn.hasTurn = 0;
          turn.hasEndedTurn = 0;
          data.Turns[i] = turn;
          BoardManagerSystem.instance.currentTurn = (BoardManagerSystem.instance.currentTurn + 1) % numPlayers;

        }
        // If is not the current player and the current player has 
        // ended his turn, pass the turn to the next player
        else if (turn.hasTurn == 0)
        {
          // Remove range mode
          GameManager.instance.gameUI.isRangeMode = false;

          // If the Entity has a buff, decrement its turn
          if (EntityManager.HasComponent(entity, typeof(Buff)))
          {
            Buff buff = EntityManager.GetComponentData<Buff>(entity);
            buff.turn += 1;
            EntityManager.SetComponentData(entity, buff);
          }
          // Finally, set the turn
          turn.hasTurn = 1;
          data.Turns[i] = turn;
        }
      }
    }
  }
}
