using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RoomSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Turn> Turn;
    public ComponentDataArray<Player> Player;
    public ComponentDataArray<Position> Position;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    if(BoardManagerSystem.instance.isTraning)
    {
      return;
    }

    for (int i = 0; i < data.Length; i++)
    {
      // If it's the beginning, obscure all the rooms
      if(BoardManagerSystem.instance.currentRoomId == 99 && BoardManagerSystem.instance.obscureAll)
      {
        BoardManagerSystem.instance.obscureAllRoom();
      }
      if(data.GameObject[i].tag == "Player")
      {
        // Get the tile of the character
        Tile tile = BoardManagerSystem.instance.getTile(data.Position[i].x, data.Position[i].y);
        // Get the room of the tile
        Room room = tile.getParent();

        // If the room isn't lighted
        if(room.getId() != BoardManagerSystem.instance.currentRoomId)
        {
          Room prevRoom = BoardManagerSystem.instance.getRoom(BoardManagerSystem.instance.currentRoomId);
          if (prevRoom != null)
          {
            //prevRoom.obscureTile();
          }
          // Light the room
          room.lightTiles();
          // Create the enemy in the room
          room.createEnemies();
          // Update the current room information
          BoardManagerSystem.instance.currentRoomId = room.getId();
        }
      }
    }
  }
}
