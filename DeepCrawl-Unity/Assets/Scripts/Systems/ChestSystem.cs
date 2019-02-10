using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Linq;

public class ChestSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Interact> Interact;
    public ComponentDataArray<Chest> Chest;
  }

  // Define the various effects of the chest
  public void chestEffect(int type)
  {
    switch(type)
    {
      default:
        GameManager.instance.gameUI.addText("You found the level map!");
        BoardManagerSystem.instance.lightAllRooms();
        break;
    }
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    EntityManager em = EntityManager;
    var puc = PostUpdateCommands;

    for (int i = 0; i < data.Length; i++)
    {
      puc.RemoveComponent<Chest>(data.Entity[i]);
      puc.RemoveComponent<Interact>(data.Entity[i]);
      // Rotate the chest cover
      GameObjectEntity entityCover = data.GameObject[i].transform.GetChild(0).GetComponent<GameObjectEntity>();
      puc.AddComponent(entityCover.Entity, new Rotation { rotationZ = -90 });

      // Choose a random chest effect
      chestEffect(Random.Range(0, 10));
    }
  }
}