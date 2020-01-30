using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Random = UnityEngine.Random;

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
                if (character.CompareTag("Player"))
                    GameManager.instance.gameUI.addText("You get -" + Math.Abs(damage.damage) + " hp!", 1);
                else
                    GameManager.instance.gameUI.addText(character.name + " gets -" + Math.Abs(damage.damage) + " hp!", 3);
            }
            else
            {
                if (character.CompareTag("Player"))
                    GameManager.instance.gameUI.addText("You get +" + Math.Abs(damage.damage) + " hp!", 2);
                else
                    GameManager.instance.gameUI.addText(character.name + " gets +" + Math.Abs(damage.damage) + " hp!", 3);
            }

            // Create PopupText
            if (!BoardManagerSystem.instance.noAnim)
            {
                if (EntityManager.HasComponent<PopupComponent>(entity))
                {
                    puc.RemoveComponent<PopupComponent>(entity);
                }
                
                // Create the correct string based on the sign of damage
                String text = damage.damage > 0 ? "-" : "+";
                text += Math.Abs(damage.damage).ToString();
                int color = damage.damage >= 0 ? 1 : 2; 

                puc.AddSharedComponent(entity, new PopupComponent
                {
                    popupText = GameManager.instance.gameUI.createPopupText(text, color),
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
                // IRL Settings
                puc.AddComponent(entity, new Death());
                GameManager.instance.gameUI.addText(character.name + " is dead!", 1);
            }

            // At the end, remove the character component
            puc.RemoveComponent<Damage>(entity);
        }
    }
}
