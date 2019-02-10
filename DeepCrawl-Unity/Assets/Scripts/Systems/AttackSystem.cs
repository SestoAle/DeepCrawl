using UnityEngine;
using System.Collections;
using Unity.Entities;


public class AttackSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObjects;
    public ComponentArray<Animator> Animators;
    public ComponentDataArray<Attack> Attacks;
    public ComponentDataArray<Turn> Turns;
  }

  [Inject] private Data data;

  bool isAnim = false;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    for (int i = 0; i < data.Length; i++)
    {
      // If the character animation is finished or is it in training mode
      if (isAnim)
      {
        // Wait for the attack animation to finish
        if (data.Animators[i].GetCurrentAnimatorStateInfo(0).IsName("Attack") ||
           data.Animators[i].GetCurrentAnimatorStateInfo(0).IsName("RangeAttack"))
        {
          return;
        }

        // Get entity
        var entity = data.Entity[i];
        isAnim = false;

        // End attack animation
        data.Animators[i].SetBool("isAttacking", false);

        // Add EndTurn component
        puc.AddComponent(entity, new EndTurn { });

        // Remove Attack component
        puc.RemoveComponent<Attack>(data.Entity[i]);
      }
      else
      {
        // Get entity
        var entity = data.Entity[i];
        // Get Attack component
        var attack = data.Attacks[i];
        // Get entity gameobject
        var character = data.GameObjects[i];

        // Get the attack tile and the character that is attacked
        Tile tile = BoardManagerSystem.instance.getTile(attack.attackTileX, attack.attackTileY);
        GameObject attackedCharacter = null;
        if (tile != null)
        {
          attackedCharacter = (GameObject)tile.getCharacter();
        }
        // Add Damage component
        if (attackedCharacter != null)
        {
          // Get the the entity attacked
          var attackEntity = attackedCharacter.GetComponent<GameObjectEntity>().Entity;
          Stats attackStats = EntityManager.GetComponentData<Stats>(attackEntity);
          // Add text in the UI
          if (character.tag == "Player")
          {
            if (attack.type == 0)
              GameManager.instance.gameUI.addText("You attack " + attackedCharacter.name + " with a sword", 3);
            else
              GameManager.instance.gameUI.addText("You attack " + attackedCharacter.name + " with a bow", 3);
          }
          else
          {
            if (attack.type == 0)
              GameManager.instance.gameUI.addText(character.name + " attacks you with a sword", 1);
            else
              GameManager.instance.gameUI.addText(character.name + " attacks you with a bow", 1);
          }

          // Add damage component
          // If player tag, add the normal damage
          if (data.GameObjects[i].tag == "Player")
          {
            int actualDamage = Mathf.Max(attack.damage - attackStats.def, 0);
            puc.AddComponent(attackEntity, new Damage { damage = actualDamage });
          }
          else
          {
            // Compute normal damage
            int actualDamage = Mathf.Max(attack.damage - attackStats.def, 0);
            puc.AddComponent(attackEntity, new Damage { damage = actualDamage });
          }
        }

        if (BoardManagerSystem.instance.noAnim)
        {
          // Add EndTurn component
          puc.AddComponent(entity, new EndTurn { });

          // Remove Attack component
          puc.RemoveComponent<Attack>(data.Entity[i]);
        }
        else
        {
          // Start attack animation
          if (attack.type == 0)
            data.Animators[i].SetTrigger("isAttacking");
          else
            data.Animators[i].SetTrigger("isRangeAttacking");
          isAnim = true;
        }
      }
    }
  }
}
