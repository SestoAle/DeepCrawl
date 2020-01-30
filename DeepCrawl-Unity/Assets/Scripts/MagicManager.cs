using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using MLAgents;
using UnityEngine.SceneManagement;

public class MagicManager : MonoBehaviour
{

    public List<GameObject> MagicEffects;
    
    [HideInInspector]
    public static MagicManager instance;
    
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }
    
    public List<Tile> MagicTypeToTiles(int type, Position position)
    {
        List<Tile> hitTiles = new List<Tile>();
        switch (type)
        {
            case 0:
                // Get the tiles surrounding the character
                for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    hitTiles.Add(BoardManagerSystem.instance.getTile(position.x + i, position.y + j));
                }
                break;
            case 1:
                // Get tiles in the orizontal direction from the character
                for (int i = -2; i < 3; i++)
                {
                    if(i == 0)
                        continue;

                    hitTiles.Add(BoardManagerSystem.instance.getTile(position.x + i, position.y));
                    hitTiles.Add(BoardManagerSystem.instance.getTile(position.x, position.y + i));
                }
                break;
        }

        return hitTiles;
    }
}
