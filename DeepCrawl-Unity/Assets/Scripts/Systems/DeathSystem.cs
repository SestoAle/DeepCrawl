using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DeathSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentDataArray<Death> Deaths;
    public ComponentDataArray<Turn> Turns;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    int enemiesDead = 0;
    for (int i = 0; i < data.Length; i++)
    {

      var puc = PostUpdateCommands;
      var entity = data.Entity[i];

      // For each entity that has a death component
      GameObject gameObject = data.GameObject[i];

      // Get the tile of the character and set the object in it to null
      Tile tile = BoardManagerSystem.instance.getTileFromObject(gameObject);
      if (tile != null && tile.getCharacter() == data.GameObject[i])
        tile.setCharacter(null);

      // Make the character invisible
      foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>())
      {
        r.enabled = false;
      }

      // Update the dead enemy counter
      if (data.GameObject[i].tag != "Player")
        enemiesDead++;
        
      // If the player dies, show the GameOver dialouge
      if (GameObject.FindGameObjectWithTag("GameOver") == null && !BoardManagerSystem.instance.noAnim && data.GameObject[i].tag == "Player")
      {
        GameManager.instance.gameUI.showGameOver(data.GameObject[i].tag == "Player");
      }
    }

    // If all the enemies of the level are dead, show the NextLevel dialogue
    if(GameObject.FindGameObjectWithTag("GameOver") == null && enemiesDead == BoardManagerSystem.instance.numEnemies && !BoardManagerSystem.instance.noAnim)
    {
      // If level = 10, show the EndGame dialogue
      if(BoardManagerSystem.instance.level >= 10)
        GameManager.instance.gameUI.showGameOver();
      else
        GameManager.instance.gameUI.showGameOver(false);
    }
  }
}
