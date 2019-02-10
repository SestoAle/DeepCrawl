using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Entities;

public class PickUpSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public GameObjectArray GameObjects;
    public EntityArray Entity;
    public ComponentDataArray<Pickable> Pickables;
    public ComponentDataArray<Position> Positions;
    public ComponentArray<MeshRenderer> Renderers;
  }

  public struct CharacterData
  {
    public readonly int Length;
    public GameObjectArray GameObjects;
    public ComponentArray<Transform> Transforms;
    public ComponentDataArray<Turn> Turns;
    public ComponentDataArray<Position> Positions;

    public SubtractiveComponent<Death> Deaths;
  }

  [Inject] private Data data;
  [Inject] private CharacterData characterData;

  protected override void OnUpdate()
  {

    var puc = PostUpdateCommands;
    var gameUI = GameManager.instance.gameUI;
    for (int i = 0; i < data.Length; i++)
    {
      // To avoid an null reference error
      if (!data.GameObjects[i].activeInHierarchy)
      {
        continue;
      }

      GameObject character = null;
      Vector3 itemPosition = new Vector3(data.Positions[i].x, 0, data.Positions[i].y);

      // Check if exists a character with the same position on the tile
      // For each character
      for (int j = 0; j < characterData.Length; j++)
      {
        // Get the item and the character position
        Vector3 characterPosition = new Vector3(characterData.Positions[j].x, 0, characterData.Positions[j].y);

        // Compute the distance from the object and the character
        if(Vector3.Distance(characterPosition, itemPosition) < 0.5f)
        {
          character = characterData.GameObjects[j];
          break;
        }
      }

      if (character != null)
      {
        // Get the item object
        Item newItem = data.GameObjects[i].GetComponent<Item>();
        // Create a copy of the item
        Item equipItem = newItem.createCopy();
        // Pickup the object depending on the type of the item
        Inventory inventory = character.GetComponent<Inventory>();
        if (newItem.GetType() == typeof(MeleeWeapon))
        {
          inventory.setMelee((MeleeWeapon)equipItem);
        }
        if (newItem.GetType() == typeof(RangeWeapon))
        {
          inventory.setRange((RangeWeapon)equipItem);
        }
        if (newItem.GetType().IsSubclassOf(typeof(Potion)))
        {
          inventory.setPotion((Potion)equipItem);
          if(character.tag == "Player" && !BoardManagerSystem.instance.noAnim)
          {
            GameManager.instance.gameUI.potionImage.GetComponent<Animator>().SetTrigger("magnify");
          }
        }

        // Add text UI
        if(character.tag == "Player")
          gameUI.addText("You collect a " + newItem.itemName, 0);
        else
          gameUI.addText(character.name + " collects a " + newItem.itemName, 0);
        // After that, remove the object from the screen
        BoardManagerSystem.instance.getTileFromObject(data.GameObjects[i]).setItem(null);
        puc.RemoveComponent<Pickable>(data.Entity[i]);
        //GameManager.instance.DestroyGameObject(data.GameObjects[i]);
        data.Renderers[i].enabled = false;
      }
    }
  }
}
