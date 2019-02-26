using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Linq;

[UpdateAfter(typeof(InputSystem))]
public class ActionSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Turn> Turns;
    public ComponentDataArray<Stats> Stats;
    public ComponentDataArray<UserInput> UserInput;
    public ComponentArray<Transform> Transform;
  }

  [Inject] private Data data;

  // Search for a character in the tiles along the specified direction
  // for the specified range. If a tile contains a character,
  // return the tile. Return null there are no tiles with a character.
  Tile getRangeTile(int direction, int range, Tile startTile)
  {
    Vector2 startPos = new Vector2(startTile.x, startTile.y);
    Vector2 offset = GameManager.instance.directionToTile(direction);
    Tile prevTile = startTile;
    for (int i = 0; i < range; i++)
    {
      startPos += offset;
      Tile tile = BoardManagerSystem.instance.getTile((int)startPos.x, (int)startPos.y);
      if (tile != null)
      {
        if (!prevTile.isNeighbour(tile))
          return null;

        if (tile.hasCharacter())
          return tile;

        if (!tile.canMove())
          return null;

        prevTile = tile;
      }
    }
    return null;
  }

  protected override void OnUpdate()
  {
    var entityManager = World.Active.GetExistingManager<EntityManager>();
    var puc = PostUpdateCommands;
    var dt = Time.deltaTime;

    for (var i = 0; i < data.Length; i++)
    {
      // Set the movement of the camera to automatic
      GameManager.instance.cameraManual = false;
      // Get entity
      var entity = data.Entity[i];
      // Get Character object and Inventory and Stats components
      GameObject character = data.GameObject[i];
      Inventory inventory = character.GetComponent<Inventory>();
      Stats stats = data.Stats[i];

      // Get weapons and potion
      MeleeWeapon meleeWeapon = character.GetComponent<Inventory>().meeleWeapon;
      RangeWeapon rangeWeapon = character.GetComponent<Inventory>().rangeWeapon;
      Potion potion = character.GetComponent<Inventory>().potion;

      // Get the position and orientation and the tile of this character
      Vector3 oldPos = data.Transform[i].position;
      int oldRotation = (int)data.Transform[i].rotation.eulerAngles.y;
      Tile tile = BoardManagerSystem.instance.getTileFromObject(character);

      Vector3 newPos = oldPos;
      int newRotation = oldRotation;

      int totalRange = data.Stats[i].actualRange;
      Tile rangeTile = null;

      // Get User Input component and check for the action
      UserInput userInput = data.UserInput[i];

      switch (userInput.action)
      {
        // Movement actions (0-7)
        case 0:
          newPos = oldPos + new Vector3(0, 0, 1);
          newRotation = (int)DIRECTION.North;
          break;
        case 1:
          newPos = oldPos + new Vector3(1, 0, 0);
          newRotation = (int)DIRECTION.East;
          break;
        case 2:
          newPos = oldPos + new Vector3(0, 0, -1);
          newRotation = (int)DIRECTION.South;
          break;
        case 3:
          newPos = oldPos + new Vector3(-1, 0, 0);
          newRotation = (int)DIRECTION.West;
          break;
        case 4:
          newPos = oldPos + new Vector3(1, 0, 1);
          newRotation = (int)DIRECTION.NorthEast;
          break;
        case 5:
          newPos = oldPos + new Vector3(1, 0, -1);
          newRotation = (int)DIRECTION.SouthEast;
          break;
        case 6:
          newPos = oldPos + new Vector3(-1, 0, -1);
          newRotation = (int)DIRECTION.SouthWest;
          break;
        case 7:
          newPos = oldPos + new Vector3(-1, 0, 1);
          newRotation = (int)DIRECTION.NorthWest;
          break;
        // Buff Action (8)
        case 8:
          // if the character doesn't have an active buff
          if (!EntityManager.HasComponent(entity, typeof(Buff)))
          {
            // If the character has a potion
            if (potion != null)
            {
              // Create the potion effect
              IComponentData potionComponent = potion.getComponent();
              if (potionComponent is Buff)
              {
                puc.AddComponent(entity, (Buff)potionComponent);
              }
              else if (potionComponent is Damage)
              {
                puc.AddComponent(entity, (Damage)potionComponent);
              }
              // destroy the potion
              inventory.setPotion(null);
            }
            else
            {
              if(character.tag == "Player")
              {
                GameManager.instance.gameUI.addText("Player " + (data.Turns[i].index + 1) + " not have a potion", 0);
              }
            }
          }
          else
          {
            if (character.tag == "Player")
            {
              GameManager.instance.gameUI.addText("You can't use a potion with a buff!", 0);
            }
          }
          break;
        // Range actions (9,16)
        case 9:
          rangeTile = getRangeTile((int)DIRECTION.North, totalRange, tile);
          newRotation = (int)DIRECTION.North;
          break;
        case 10:
          rangeTile = getRangeTile((int)DIRECTION.East, totalRange, tile);
          newRotation = (int)DIRECTION.East;
          break;
        case 11:
          rangeTile = getRangeTile((int)DIRECTION.South, totalRange, tile);
          newRotation = (int)DIRECTION.South;
          break;
        case 12:
          rangeTile = getRangeTile((int)DIRECTION.West, totalRange, tile);
          newRotation = (int)DIRECTION.West;
          break;
        case 13:
          rangeTile = getRangeTile((int)DIRECTION.NorthEast, totalRange, tile);
          newRotation = (int)DIRECTION.NorthEast;
          break;
        case 14:
          rangeTile = getRangeTile((int)DIRECTION.SouthEast, totalRange, tile);
          newRotation = (int)DIRECTION.SouthEast;
          break;
        case 15:
          rangeTile = getRangeTile((int)DIRECTION.SouthWest, totalRange, tile);
          newRotation = (int)DIRECTION.SouthWest;
          break;
        case 16:
          rangeTile = getRangeTile((int)DIRECTION.NorthWest, totalRange, tile);
          newRotation = (int)DIRECTION.NorthWest;
          break;
      }

      // If the character has moved
      if (newPos != oldPos)
      {
        // Get the new tile and check if exists and if the character can move on it
        Tile newTile = BoardManagerSystem.instance.getTile((int)newPos.x, (int)newPos.z);

        if (newTile != null && newTile.canMove() && tile.isNeighbour(newTile))
        {
          // Add Movement component
          puc.AddComponent(entity, new Movement { x = (int)newPos.x, y = (int)newPos.z, rotation = newRotation });
        }
        else if (newTile != null && newTile.hasCharacter() && tile.isNeighbour(newTile))
        {
          // Compute the total attack (base attack of player + damage weapon)
          int damage = Random.Range(meleeWeapon.minDamage, meleeWeapon.maxDamage + 1) + stats.atk;

          // If the character is an ally, don't hit it
          if (character.tag != ((GameObject)newTile.getCharacter()).tag)
          {
            puc.AddComponent(entity, new Attack { damage = damage, attackTileX = newTile.x, attackTileY = newTile.y, type = 0 });
          }
        }
        else if (newTile != null && newTile.hasInteractable() && tile.isNeighbour(newTile))
        {
          // If the player interacts with an interactable
          if (character.tag == "Player")
          {
            GameObject interactableObject = newTile.getInteractable();
            Entity interactableEntity = interactableObject.GetComponent<GameObjectEntity>().Entity;
            // Add the interact component
            if (!entityManager.HasComponent<Interact>(interactableEntity))
              puc.AddComponent(interactableEntity, new Interact { });
          }
        }
      }

      if (rangeTile != null && rangeTile.hasCharacter() && tile.parent == rangeTile.parent)
      {
        // If is not in fight with the target
        if(rangeTile.isAround(character))
        {
          GameManager.instance.gameUI.addText("You can not shoot a nearby enemy!", 0);
        }
        else
        {
          // If is not an ally
          if(character.tag != ((GameObject)rangeTile.getCharacter()).tag)
          {
            // Compute the total attack (base attack of player + damage weapon)
            int damage = Random.Range(rangeWeapon.minDamage, rangeWeapon.maxDamage + 1) + stats.des;
            puc.AddComponent(entity, new Attack { damage = damage, attackTileX = rangeTile.x, attackTileY = rangeTile.y, type = 1 });
          }
        }
      }

      if (newRotation != oldRotation)
      {
        // Add roation component
        puc.AddComponent(entity, new Rotation { rotationY = newRotation });
      }

      // Remove UserInput component
      puc.RemoveComponent<UserInput>(entity);

      // Dehighlight all the tiles after the input
      if(!BoardManagerSystem.instance.noAnim && character.tag == "Player")
      {
        BoardManagerSystem.instance.deHighlightAll(tile.parent);
      }
        
    }
  }
}
