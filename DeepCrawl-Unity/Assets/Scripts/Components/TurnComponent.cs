using Unity.Entities;
using System;

[Serializable]
public struct Turn : IComponentData
{
  public int index;
  public int hasTurn;
  public int hasEndedTurn;
}

public class TurnComponent : ComponentDataWrapper<Turn> { }
