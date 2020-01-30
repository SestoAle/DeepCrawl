using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Properties;

public class MagicSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObject;
        public ComponentDataArray<Magic> Magics;
        public ComponentDataArray<Stats> Stats;
        public ComponentDataArray<Position> Positions;
    }

    [Inject] private Data data;
    private GameObject effect = null;

    protected override void OnUpdate()
    {
        var puc = PostUpdateCommands;
        for (int i = 0; i < data.Length; i++)
        {
            // Get entity and the character who spells the magic
            var entity = data.Entity[i];
            var position = data.Positions[i];
            
            // Get the magic component and the stats of the agent
            var magic = data.Magics[i];
            var stats = data.Stats[i];
            
            // TODO: change this to a new ParticleSystem
            // If there is at least one ParticleSystem that is playing, continue the system to let it finish
            if (effect != null && effect.GetComponent<ParticleSystem>().isPlaying)
            {
                continue;
            }
            // If there is an effect but it is finished, remove the effect and the Component
            else if (effect != null && !effect.GetComponent<ParticleSystem>().isPlaying)
            {
                effect = null;
            }
            // If not enough mp, return
            else if (stats.mp < magic.mp)
            {
                GameManager.instance.gameUI.addText("Not enough mp!", 0);
            }
            // Else, spell the magic
            else
            {
                // Low the mp based on the Magic component
                puc.AddComponent(entity, new MagicPoint{mp = -magic.mp});
                GameManager.instance.gameUI.addText("You spell a magic!", 0);
                // Get the hit tiles based on the magic type
                List<Tile> hitTiles = MagicManager.instance.MagicTypeToTiles(magic.type, position);
                // For each tile, check whether there is a character
                foreach (var tile in hitTiles)
                {
                    if (tile != null)
                    {
                        // Instantiate effect
                        if (!BoardManagerSystem.instance.noAnim)
                        {
                            effect = GameObject.Instantiate(MagicManager.instance.MagicEffects[0]);
                            effect.transform.position = new Vector3(tile.x, 0, tile.y);
                            // HighLight the hit tiles with a attack Material
                            // TODO: change this to a new ParticleSystem
                            tile.startHighlightAnimation(tile.canAttackMaterial);
                        }
                    }
                    // If there is a character in the hit tile
                    if (tile != null && tile.hasCharacter())
                    {
                        // Add damage to that character
                        var characterEntity = ((GameObject) tile.getCharacter()).GetComponent<Character>().Entity;
                        puc.AddComponent(characterEntity, new Damage{ damage = magic.damage});
                        
                    }
                }

                continue;
            }
            
            // End the turn
            puc.AddComponent(entity, new EndTurn());
            puc.RemoveComponent<Magic>(entity);
        }
        
        

    }
}