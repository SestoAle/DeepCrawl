using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;

public class UISystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObjects;
        public ComponentDataArray<Stats> Stats;
        public ComponentDataArray<Turn> Turns;
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
        if (BoardManagerSystem.instance.noAnim)
        {
            return;
        }

        for (int i = 0; i < data.Length; i++)
        {
            if (data.GameObjects[i].tag == "Player")
            {
                // For the player that has the turn, change the game UI with his information
                GameUI gameUI = GameManager.instance.gameUI;

                // Get the stats of the player
                Stats stat = data.Stats[i];
                // Get the inventory of the player
                Inventory inventory = data.GameObjects[i].GetComponent<Inventory>();

                // Display the HP
                if (stat.hp < 10)
                {
                    gameUI.setHp(stat.hp, stat.maxHp, 1);
                }
                else if (EntityManager.HasComponent<Buff>(data.Entity[i]))
                {
                    gameUI.setHp(stat.hp, stat.maxHp, 5);
                }
                else
                {
                    gameUI.setHp(stat.hp, stat.maxHp, 0);
                }


                // Display the melee weapon
                gameUI.setMelee(inventory.meeleWeapon.itemName,
                                inventory.meeleWeapon.damageString);
                // Display the range weapon
                gameUI.setRange(inventory.rangeWeapon.itemName,
                                inventory.rangeWeapon.damageString,
                                inventory.rangeWeapon.range.ToString());

                // Change the color of the range button
                if (gameUI.isRangeMode)
                {
                    gameUI.changeImageBackground(gameUI.rangeButton.transform.parent.GetComponent<Image>(), 4);
                }
                else
                {
                    gameUI.changeImageBackground(gameUI.rangeButton.transform.parent.GetComponent<Image>(), 6);
                }

                // If the player has a potion, display it and hilight potion button
                if (inventory.potion != null)
                {
                    gameUI.setPotion(inventory.potion.itemName);
                    gameUI.changeImageAlpha(true, gameUI.potionImage);
                    if (inventory.potion.GetType() == typeof(BuffPotion))
                    {
                        gameUI.changeImagePotion(true);
                    }
                    else
                    {
                        gameUI.changeImagePotion(false);
                    }
                }
                else
                {
                    gameUI.setPotion("None");
                    gameUI.changeImageAlpha(false, gameUI.potionImage);
                    gameUI.setNullImagePotion();
                }

                // Check it the story section must be updated
                if (gameUI.hasToUptade)
                {
                    gameUI.updateText();
                }

                // Set level text
                gameUI.changeLevelText(BoardManagerSystem.instance.level);
            }
        }
    }
}
