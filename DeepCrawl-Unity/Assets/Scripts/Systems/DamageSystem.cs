using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateAfter(typeof(ActionSystem))]
public class DamageSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Damage> Damage;
    public ComponentDataArray<Stats> Stats;
    public ComponentDataArray<Turn> Turns;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    for (int i = 0; i < data.Length; i++)
    { 
      // For each entity that has a damage component
      var entity = data.Entity[i];
      var stats = data.Stats[i];
      var damage = data.Damage[i];
      var character = data.GameObject[i];

      // Decrease the hp of the character
      stats.hp -= damage.damage;
      if (stats.hp > stats.maxHp)
      {
        stats.hp = stats.maxHp;
      }
      // Add text UI
      if (damage.damage >= 0)
      {
        if(character.tag == "Player")
          GameManager.instance.gameUI.addText("You get " + damage.damage + " damages!", 1);
        else
          GameManager.instance.gameUI.addText(character.name + " gets " + damage.damage + " damages!", 3);
      }
      else
      {
        if (character.tag == "Player")
          GameManager.instance.gameUI.addText("You get " + damage.damage + " damages!", 2);
        else
          GameManager.instance.gameUI.addText(character.name + " gets " + damage.damage + " damages!", 3);
      }

      // Create PopupText
      if(!BoardManagerSystem.instance.noAnim)
      {
        if (EntityManager.HasComponent<PopupComponent>(entity))
        {
          puc.RemoveComponent<PopupComponent>(entity);
        }

        int color = damage.damage >= 0 ? 1 : 2;

        puc.AddSharedComponent(entity, new PopupComponent {
          popupText = GameManager.instance.gameUI.createPopupText(damage.damage.ToString(), color),
          randomOffset = Random.Range(-0.5f, +0.5f)
        });
      }

      // If the hp is <= 0, add a DeathComponent
      if (stats.hp > 0)
      {
        data.Stats[i] = stats;
      }
      else if (stats.hp <= 0)
      {
        stats.hp = 0;
        data.Stats[i] = stats;
        puc.AddComponent(entity, new Death());
        GameManager.instance.gameUI.addText(character.name + " is dead!", 1);
      }

      // At the end, remove the character component
      puc.RemoveComponent<Damage>(entity);
    }
  }
}
