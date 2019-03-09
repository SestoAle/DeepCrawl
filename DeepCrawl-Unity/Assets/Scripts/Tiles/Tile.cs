using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
  // Colors for hilights the tile
  public Color defaultMaterial;
  public Color canMoveMaterial;
  public Color canAttackMaterial;
  public Color canRangeMaterial;
  public Color canInteractMaterial;
  public GameObject tileObject;

  public bool isHighlighted;
  public bool isSelected;

  // Position of the tile in the board
  [HideInInspector]
  public int x;
  [HideInInspector]
  public int y;

  // The character that occupies the tile 
  public Object characterInTile = null;
  // The item that occupies the tile
  Item itemInTile = null;
  // The interactable in this tile
  GameObject interactableInTile = null;
  // The neighbours of this tile
  public List<Tile> neighbours = new List<Tile>();
  // If a character can move on this tile
  protected bool defaultCanMove;


  //TODO: rooms
  public Room parent;
  public bool isDoor = false;


  Material material;

  // Constructor
  public Tile()
  {
    this.defaultCanMove = true;

  }
  private void Awake()
  {

    if (tileObject == null)
      tileObject = gameObject;

    material = tileObject.GetComponent<Renderer>().material;
  }

  public void setPosition(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  // Set the room parent of this tile
  public void setParent(Room room)
  {
    this.parent = room;
  }

  public Room getParent()
  {
    return this.parent;
  }

  public Vector3 getPosition()
  {
    return new Vector3(x, 0, y);
  }

  // Set the character that is on the tile
  public void setCharacter(Object characterInTile)
  {
    this.characterInTile = characterInTile;
  }

  // Get the character that is on the tile
  public Object getCharacter()
  {
    return this.characterInTile;
  }

  // Return true if the tile has a character on it
  public bool hasCharacter()
  {
    return this.characterInTile != null;
  }

  // Set the item that is on the tile
  public void setItem(Item itemInTile)
  {
    this.itemInTile = itemInTile;
  }

  // Get the item that is on the tile
  public Item getItem()
  {
    return this.itemInTile;
  }

  // Return true if the tile has a item on it
  public bool hasItem()
  {
    return this.itemInTile != null;
  }

  // Set the interactable that is on the tile
  public void setInteractable(GameObject interactableInTile)
  {
    this.interactableInTile = interactableInTile;
  }

  // Get the interactable that is on the tile
  public GameObject getInteractable()
  {
    return this.interactableInTile;
  }

  // Return true if the tile has a interactable on it
  public bool hasInteractable()
  {
    return this.interactableInTile != null;
  }

  // Return true if a charcater can move on this tile
  public bool canMove()
  {
    return defaultCanMove && !hasCharacter() && !hasInteractable();
  }

  // Add a neighbours to the tile tile
  public void addNeighbour(Tile tile)
  {
    if (tile != null)
    {
      this.neighbours.Add(tile);
    }
  }

  // Get all the neighbours of the tile
  public List<Tile> getNeighbours()
  {
    return this.neighbours;
  }

  public void removeNeighbours()
  {
    this.neighbours.Clear();
  }

  // Reset all the variables of this tile (required if use a pool)
  public void resetTile()
  {
    isDoor = false;
    removeNeighbours();
    setCharacter(null);
    setItem(null);
    setInteractable(null);
  }

  // Hilights all the neighbours of the tile
  public void highlightNeighbours()
  {
    if (BoardManagerSystem.instance.noAnim)
      return;
    foreach (Tile t in neighbours)
    {
      if(!t.isHighlighted)
        t.highlight();
    }
  }

  // Highlight this tile if a player can attack this tile with range weapon
  public void rangeHighlight()
  {
    if (isHighlighted)
      return;
    material.color = canRangeMaterial;
    startHighlightAnimation();
  }

  // Highlight this tile, depending on its status
  public void highlight()
  {
    if (isHighlighted)
    {
      return;
    }

    if (canMove())
    {
      material.color = canMoveMaterial;
      startHighlightAnimation();
    }
    else if (hasCharacter())
    {
      material.color = canAttackMaterial;
      startHighlightAnimation();
    }
    else if(hasInteractable())
    {
      material.color = canInteractMaterial;
      startHighlightAnimation();
    }
  }

  // Restore the default material of the tile
  public void deHighlight()
  {
    if (!isHighlighted && !isSelected)
      return;
    material.color = defaultMaterial;
    stopHighlightAnimation();
  }

  // Restore the default material of the neighbours
  public void deHighlightNeighbours()
  {
    foreach (Tile t in neighbours)
    {
      t.deHighlight();
    }
  }

  // Return true if there is an enemy around the tile
  public bool isEnemyAround()
  {
    foreach (Tile t in neighbours)
    {
      if (t.hasCharacter())
      {
        return true;
      }
    }
    return false;
  }

  // Return true if there is character around this tile
  public bool isAround(GameObject character)
  {
    foreach (Tile t in neighbours)
    {
      if (t.hasCharacter())
      {
        if ((GameObject)t.getCharacter() == character)
          return true;
      }
    }
    return false;
  }

  // Start the highlight animation
  public void startHighlightAnimation()
  {
    isHighlighted = true;
    float threshold = GameManager.instance.gameUI.highlightIntensity;
    float time = GameManager.instance.gameUI.highlightTime;

    Color animatedColor = new Color(
      Mathf.Clamp01(material.color.r * threshold),
      Mathf.Clamp01(material.color.g * threshold),
      Mathf.Clamp01(material.color.b * threshold)
    );

    if(!BoardManagerSystem.instance.isTraning)
      {
        iTween.ColorTo(tileObject, iTween.Hash(
        "color", animatedColor,
        "time", time,
        "easetype", iTween.EaseType.linear,
        "looptype", iTween.LoopType.pingPong
      ));
    }
  }

  // Stop the highlight animation
  public void stopHighlightAnimation()
  {
    isHighlighted = false;
    isSelected = false;
    if(!BoardManagerSystem.instance.isTraning)
      iTween.Stop(tileObject);
  }

  // Start the select animation
  public void selectTile()
  {
    if (isHighlighted || isSelected)
      return;
    isSelected = true;
    float time = GameManager.instance.gameUI.highlightTime;
    Color animatedColor = new Color(
      Mathf.Clamp01(material.color.r * 0.5f),
      Mathf.Clamp01(material.color.g * 0.5f),
      Mathf.Clamp01(material.color.b * 0.5f)
    );

    if (!BoardManagerSystem.instance.isTraning)
    {
      iTween.ColorTo(tileObject, iTween.Hash(
        "color", animatedColor,
        "time", time,
        "easetype", iTween.EaseType.linear,
        "looptype", iTween.LoopType.pingPong
      ));
    }
  }

  // Return true if tile is a neighbour of this tile
  public bool isNeighbour(Tile tile)
  {
    bool isNeigh = false;
    foreach(Tile t in neighbours)
    {
      if(t == tile)
      {
        return true;
      }
    }
    return isNeigh;
  }

  // Stop select animation
  public void deSelectTile()
  {
    if (isSelected)
    {
      deHighlight();
    }
  }

  public bool canMoveFromHere()
  {
    int count = 0;
    foreach(Tile n in neighbours)
    {
      if(n.canMove() || n.hasCharacter())
      {
        break;
      }
      else
      {
        count++;
      }
    }
    if(count >= neighbours.Count)
    {
      return false;
    }
    return true;
  }
}
