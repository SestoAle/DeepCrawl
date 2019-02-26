using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateAfter(typeof(ActionSystem))]
public class MovementSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObjects;
    public ComponentDataArray<Movement> Movements;
    public ComponentDataArray<Turn> Turns;
    public ComponentArray<Transform> Transforms;
    public ComponentArray<Animator> Animators;
    public ComponentDataArray<Position> Position;
  }

  [Inject] private Data data;
  public bool isAnim = false;

  protected override void OnUpdate()
  {
    // Get character speed
    float characterSpeed = GameManager.instance.characterSpeed;
    var puc = PostUpdateCommands;
    var dt = Time.deltaTime;

    for (int i = 0; i < data.Length; i++)
    {
    
      if (data.Movements[i].animation == 0)
      {
        Tile startTile = BoardManagerSystem.instance.getTileFromObject(data.GameObjects[i]);
        if(startTile != null && startTile.getCharacter() == data.GameObjects[i])
        {
          startTile.setCharacter(null);
          Movement m = data.Movements[i];
          m.animation = 1;
          data.Movements[i] = m;
        }
      }
        
      // Get new position from the movement component
      Vector3 newPos = new Vector3(data.Movements[i].x, 0, data.Movements[i].y);

      if(BoardManagerSystem.instance.noAnim)
      {
        data.Transforms[i].position = newPos;
      }
      else
      {
        data.Transforms[i].position = Vector3.Lerp(data.Transforms[i].position, newPos, characterSpeed * dt);
        // Start run animation
        data.Animators[i].SetBool("isMoving", true);
      }

      // When the movement is complete
      if (Vector3.Distance(data.Transforms[i].position, newPos) < 0.1)
      {
        // Set the int position
        data.Transforms[i].position = newPos;

        // End run animation if is not LongMovement
        if(!EntityManager.HasComponent<MovementElementBuffer>(data.Entity[i]) || 
           EntityManager.GetBuffer<MovementElementBuffer>(data.Entity[i]).Length == 0)
          data.Animators[i].SetBool("isMoving", false);

        // Remove movement component
        var entity = data.Entity[i];
        // Update the tile informations
        Tile newTile = BoardManagerSystem.instance.getTileFromObject(data.GameObjects[i]);
        newTile.setCharacter(data.GameObjects[i]);
        PostUpdateCommands.SetComponent(entity, new Position { x = data.Movements[i].x, y = data.Movements[i].y});
        puc.RemoveComponent<Movement>(entity);
        // Update turn component
        puc.AddComponent(data.Entity[i], new EndTurn { });
      }
    }
  }
}
