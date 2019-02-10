using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateAfter(typeof(MovementSystem))]
public class DoorSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Door;
    public GameObjectArray GameObject;
    public ComponentArray<Transform> Transforms;
    public ComponentDataArray<Door> DoorComponent;
    public ComponentDataArray<Fadeble> Fadeble;
  }

  public struct PlayerGroup
  {
    public readonly int Length;
    public ComponentDataArray<Player> Player;
    public ComponentDataArray<Position> Position;
  }

  [Inject] private Data data;
  [Inject] private PlayerGroup playerGroup;

  protected override void OnUpdate()
  {
    if(BoardManagerSystem.instance.isTraning)
    {
      return;
    }

    // Get the player position
    Vector3 playerPosition = new Vector3(playerGroup.Position[0].x, 0, playerGroup.Position[0].y);

    for (int i = 0; i < data.Length; i++)
    {
      // Get the door position
      Transform doorTransform = data.Transforms[i];

      // If the player is near the door
      if (Vector3.Distance(doorTransform.position, playerPosition) < 1f)
      {
        // Add a fade component to the door elements
        Fadeble fadeble = data.Fadeble[i];
        if (fadeble.isFade == 0)
        {
          fadeble.isFade = 1;
          data.Fadeble[i] = fadeble;
          Transform transform = data.Transforms[i];
          for (int c = 0; c < transform.childCount; c++)
          {
            GameObject childObject = transform.GetChild(c).gameObject;
            Entity childEntity = childObject.GetComponent<GameObjectEntity>().Entity;
            PostUpdateCommands.AddComponent(childEntity, new Fade { });
          }

        }
      }
    }
  }
}
