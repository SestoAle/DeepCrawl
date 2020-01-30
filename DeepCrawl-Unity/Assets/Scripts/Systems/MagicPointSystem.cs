using System;
using System.Net.Mail;
using UnityEngine;
using Unity.Entities;

public class MagicPointSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObject;
        public ComponentDataArray<MagicPoint> MagicPoints;
        public ComponentDataArray<Stats> Stats;
        public ComponentDataArray<Position> Positions;
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
        var puc = PostUpdateCommands;
        for (int i = 0; i < data.Length; i++)
        {
            // Get entity and stats of the character
            var entity = data.Entity[i];
            var stats = data.Stats[i];
            
            // Get the mp to modify
            var mp = data.MagicPoints[i];
            
            // Set the new MagicPoint
            stats.mp = Mathf.Clamp(stats.mp + mp.mp, 0, stats.maxMp);
            data.Stats[i] = stats;

            if (!BoardManagerSystem.instance.noAnim)
            {
                if (EntityManager.HasComponent<PopupComponent>(entity))
                {
                    puc.RemoveComponent<PopupComponent>(entity);
                }

                String text = mp.mp >= 0 ? "+" : "";
                text += mp.mp.ToString();
                
                puc.AddSharedComponent(entity, new PopupComponent
                {
                    popupText = GameManager.instance.gameUI.createPopupText(text, 7),
                    randomOffset = UnityEngine.Random.Range(-0.5f, +0.5f)
                });
                
                GameManager.instance.gameUI.addText("You get " + text + " mp!", 7);
            }
            
            // Remove component
            puc.RemoveComponent<MagicPoint>(entity);
        }
    }
}