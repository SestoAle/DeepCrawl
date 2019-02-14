using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PopupSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    [ReadOnly] public SharedComponentDataArray<PopupComponent> Popup;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    if (BoardManagerSystem.instance.isTraning)
      return;
    // Manage the Popup Text to follow the entity that owns it
    for (int i = 0; i < data.Length; i++)
    {
      // Get the text and the character
      GameObject popupText = data.Popup[i].popupText;
      GameObject character = data.GameObject[i];

      // If it's destroyed, remove the component
      if(popupText == null)
      {
        PostUpdateCommands.RemoveComponent<PopupComponent>(data.Entity[i]);
        return;
      }

      // Update the text position to follow the entity. The component comes with
      // a random x offset to be more pleasant
      Vector3 newPosition = character.transform.position;
      Vector3 screenPosition = Camera.main.WorldToScreenPoint(new Vector3(newPosition.x + data.Popup[i].randomOffset, newPosition.y+GameManager.instance.gameUI.popupOffset, newPosition.z));
      popupText.transform.position = screenPosition;

    }
  }
}