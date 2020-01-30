using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BuffSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObjects;
        public ComponentDataArray<Stats> Stats;
        public ComponentDataArray<Buff> Buff;

        public SubtractiveComponent<PopupComponent> Popup;
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
        for (int i = 0; i < data.Length; i++)
        {
            // Get the stats and the buff
            Stats stats = data.Stats[i];
            Buff buff = data.Buff[i];

            // If the buff isn't start yet
            if (buff.turn == 0)
            {
                // Upgrade the stats and add a turn
                stats.maxHp += buff.hp;
                stats.hp += buff.hp;
                stats.def += buff.def;
                stats.atk += buff.atk;

                buff.turn = 1;
                data.Stats[i] = stats;
                data.Buff[i] = buff;

                // Add UI text
                GameManager.instance.gameUI.addText("Your stats are increased:", 0);
                GameManager.instance.gameUI.addText("Def + " + buff.def, 0);
                GameManager.instance.gameUI.addText("Atk + " + buff.atk, 0);
                GameManager.instance.gameUI.addText("Turns " + buff.duration, 0);

                // Create PopUp text
                if (!BoardManagerSystem.instance.noAnim)
                {
                    //PostUpdateCommands.RemoveComponent<PopupComponent>(data.Entity[i]);
                    PostUpdateCommands.AddSharedComponent(data.Entity[i], new PopupComponent
                    {
                        popupText = GameManager.instance.gameUI.createPopupText(
                        "+" + buff.def + " Def \n +" + buff.atk + " Atk \n " + buff.duration + " Turns",
                        5),
                        randomOffset = Random.Range(-0.5f, +0.5f)
                    });
                }
            }

            // If the turn is equal to the duration of the buff,
            // restore the old stats
            if (buff.turn == buff.duration + 1)
            {
                // Add UI text
                GameManager.instance.gameUI.addText("The buff is finished!", 0);
                // Create PopUp text
                if (!BoardManagerSystem.instance.noAnim)
                {
                    //PostUpdateCommands.RemoveComponent<PopupComponent>(data.Entity[i]);
                    PostUpdateCommands.AddSharedComponent(data.Entity[i], new PopupComponent
                    {
                        popupText = GameManager.instance.gameUI.createPopupText(
                        "-" + buff.def + " Def \n -" + buff.atk + " Atk",
                        5),
                        randomOffset = Random.Range(-0.5f, +0.5f)
                    });
                }

                stats.maxHp -= buff.hp;
                stats.hp -= buff.hp;
                stats.def -= buff.def;
                stats.atk -= buff.atk;

                data.Stats[i] = stats;
                if (stats.hp <= 0)
                {
                    PostUpdateCommands.AddComponent(data.Entity[i], new Death());
                }
                PostUpdateCommands.RemoveComponent<Buff>(data.Entity[i]);
            }
        }
    }
}
