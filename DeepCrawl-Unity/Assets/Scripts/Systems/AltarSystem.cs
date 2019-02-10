using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Linq;

public class AltarSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Interact> Interact;
    public ComponentDataArray<Altar> Altar;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    // Get ECS utilities
    EntityManager em = EntityManager;
    GameUI gameUI = GameManager.instance.gameUI;
    var puc = PostUpdateCommands;

    for (int i = 0; i < data.Length; i++)
    {
      // Get player GameObject and Entity
      GameObject player = BoardManagerSystem.instance.activePlayer;
      Entity playerEntity = player.GetComponent<Character>().Entity;

      // Get the player stats
      Stats stats = EntityManager.GetComponentData<Stats>(playerEntity);

      // Add Wait component to not make the player move
      if (!EntityManager.HasComponent<Wait>(playerEntity))
        puc.AddComponent(playerEntity, new Wait());

      Altar altar = data.Altar[i];

      // Display altar dialouge
      gameUI.setAltarText(altar, stats);
      if (!gameUI.altarPanel.gameObject.activeInHierarchy)
      {
        gameUI.showAltarDialog();
      }

      // Disable or enable the modifiers buttons depending on the remaining points
      if (altar.modAtk <= 0)
      {
        gameUI.decreaseAtk.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.decreaseAtk.GetComponent<ButtonManager>().enable();
      }
      if (altar.modHp <= 0 || stats.hp <= 1)
      {
        gameUI.decreaseHp.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.decreaseHp.GetComponent<ButtonManager>().enable();
      }
      if (altar.modDef <= 0)
      {
        gameUI.decreaseDef.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.decreaseDef.GetComponent<ButtonManager>().enable();
      }
      if (altar.modDes <= 0)
      {
        gameUI.decreaseDes.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.decreaseDes.GetComponent<ButtonManager>().enable();
      }

      if (stats.atk >= 10 || altar.actualPoints <= 0)
      {
        gameUI.increaseAtk.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.increaseAtk.GetComponent<ButtonManager>().enable();
      }

      if (stats.maxHp >= 30 || altar.actualPoints <= 0)
      {
        gameUI.increaseHp.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.increaseHp.GetComponent<ButtonManager>().enable();
      }

      if (stats.def >= 10 || altar.actualPoints <= 0)
      {
        gameUI.increaseDef.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.increaseDef.GetComponent<ButtonManager>().enable();
      }

      if (stats.des >= 10 || altar.actualPoints <= 0)
      {
        gameUI.increaseDes.GetComponent<ButtonManager>().disable();
      }
      else
      {
        gameUI.increaseDes.GetComponent<ButtonManager>().enable();
      }

      // If the altar has remianing points 
      if (altar.actualPoints > 0)
      {
        if (gameUI.increaseAtk.GetComponent<ButtonManager>().GetButtonDown())
        {
          // Increase the ATK point below cap
          if (stats.atk >= 10)
          {
            continue;
          }
          altar.actualPoints -= 1;
          altar.modAtk += 1;
          data.Altar[i] = altar;
          stats.atk += 1;
        }

        if (gameUI.increaseDes.GetComponent<ButtonManager>().GetButtonDown())
        {
          // Increase the DES point below cap
          if (stats.des >= 10)
          {
            continue;
          }
          altar.actualPoints -= 1;
          altar.modDes += 1;
          data.Altar[i] = altar;
          stats.des += 1;
        }

        if (gameUI.increaseDef.GetComponent<ButtonManager>().GetButtonDown())
        {
          // Increase the DEF point below cap
          if (stats.def >= 10)
          {
            continue;
          }
          altar.actualPoints -= 1;
          altar.modDef += 1;
          data.Altar[i] = altar;
          stats.def += 1;
        }

        if (gameUI.increaseHp.GetComponent<ButtonManager>().GetButtonDown())
        {
          // Increase the HP point below cap
          if (stats.maxHp >= 30)
          {
            continue;
          }
          altar.actualPoints -= 1;
          altar.modHp += 1;
          data.Altar[i] = altar;
          stats.maxHp += 1;
          stats.hp += 1;
        }
      }

      if (gameUI.decreaseAtk.GetComponent<ButtonManager>().GetButtonDown())
      {
        // Decrease the ATK point
        if (altar.modAtk > 0)
        {
          altar.actualPoints += 1;
          altar.modAtk -= 1;
          data.Altar[i] = altar;
          stats.atk -= 1;
        }
      }

      if (gameUI.decreaseHp.GetComponent<ButtonManager>().GetButtonDown())
      {
        // Decrease the HP point if not 0
        if (altar.modHp > 0 && stats.hp > 1)
        {
          altar.actualPoints += 1;
          altar.modHp -= 1;
          data.Altar[i] = altar;
          stats.maxHp -= 1;
          stats.hp -= 1;
        }
      }

      if (gameUI.decreaseDef.GetComponent<ButtonManager>().GetButtonDown())
      {
        // Decrease the DEF point 
        if (altar.modDef > 0)
        {
          altar.actualPoints += 1;
          altar.modDef -= 1;
          data.Altar[i] = altar;
          stats.def -= 1;
        }
      }

      if (gameUI.decreaseDes.GetComponent<ButtonManager>().GetButtonDown())
      {
        // Decrease the DES point 
        if (altar.modDes > 0)
        {
          altar.actualPoints += 1;
          altar.modDes -= 1;
          data.Altar[i] = altar;
          stats.des -= 1;
        }
      }

      // Reset all the points of the altar
      if (gameUI.resetProgress.GetComponent<ButtonManager>().GetButtonDown())
      {
        stats.atk -= altar.modAtk;
        stats.des -= altar.modDes;
        stats.def -= altar.modDef;
        stats.maxHp -= altar.modHp;
        stats.hp -= altar.modHp;
        altar.actualPoints = altar.startingPoints;
        altar.modAtk = 0;
        altar.modDef = 0;
        altar.modDes = 0;
        altar.modHp = 0;
        data.Altar[i] = altar;
      }

      // Close the altar dialogue and system
      if (gameUI.finishProgress.GetComponent<ButtonManager>().GetButtonDown())
      {
        puc.RemoveComponent<Wait>(playerEntity);
        puc.RemoveComponent<Interact>(data.Entity[i]);
        gameUI.showAltarDialog();
      }

      // Update player stats
      puc.SetComponent(playerEntity, stats);
    }
  }
}