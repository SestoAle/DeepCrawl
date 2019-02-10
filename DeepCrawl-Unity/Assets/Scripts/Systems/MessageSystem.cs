using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class MessageSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    [ReadOnly] public SharedComponentDataArray<MessageComponent> Messages;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    var gameUI = GameManager.instance.gameUI;

    for (int i = 0; i < data.Length; i++)
    {

      var entity = data.Entity[i];
      var character = data.GameObject[i];

      if (character.gameObject.tag == "Player")
      {
        gameUI.addText(data.Messages[i].text, 2);
      }
      else
      {
        gameUI.addText(data.Messages[i].text, 1);
      }


      puc.RemoveComponent<MessageComponent>(entity);
    }
  }
}