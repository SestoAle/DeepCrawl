using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Room
{

    Board board;

    int size = 8;
    int[] coords = new int[2];
    int[] roomMapCoords = new int[2];
    Tile[,] tiles;
    int[,] tileMap;
    int id;
    List<Item> items;
    List<GameObject> walls;
    List<GameObject> enemies;
    List<GameObject> others;
    bool isLighted = true;
    bool isEntered = false;
    int numEnemies = 0;


    Dictionary<string, Room> roomNeigh = new Dictionary<string, Room>()
  {
    {"east", null},
    {"west", null},
    {"north", null},
    {"south", null}
  };

    Dictionary<string, List<int[]>> doorTiles = new Dictionary<string, List<int[]>>()
  {
    {"east", null},
    {"west", null},
    {"north", null},
    {"south", null}
  };

    // Constructor
    public Room(int id, Board board, int size)
    {
        this.size = size;
        this.id = id;
        tiles = new Tile[size, size];
        this.board = board;
        items = new List<Item>();
        walls = new List<GameObject>();
        enemies = new List<GameObject>();
        others = new List<GameObject>();
    }

    public int getSize()
    {
        return size;
    }

    public int getId()
    {
        return id;
    }

    public int[] getCoords()
    {
        return coords;
    }

    public int[] getRoomMapCoords()
    {
        return roomMapCoords;
    }

    public List<Item> getItems()
    {
        return items;
    }

    public Dictionary<string, Room> getRoomNeigh()
    {
        return roomNeigh;
    }

    public Dictionary<string, List<int[]>> getDoorTiles()
    {
        return doorTiles;
    }

    public Tile[,] getTiles()
    {
        return tiles;
    }

    public void setSize(int size)
    {
        this.size = size;
    }

    public void setCoords(int[] coords)
    {
        this.coords = coords;
    }

    public void setRoomMapCords(int[] coords)
    {
        this.roomMapCoords = coords;
    }

    public void addRoomNeigh(int[] direction, Room room)
    {
        string dirString = directionToString(direction);
        roomNeigh[dirString] = room;
    }

    public void addDoors(int[] direction, List<int[]> tiles)
    {
        string dirString = directionToString(direction);
        doorTiles[dirString] = tiles;
    }

    public void resetDoors()
    {
        doorTiles = new Dictionary<string, List<int[]>>()
    {
      {"east", null},
      {"west", null},
      {"north", null},
      {"south", null}
    };
    }

    public string directionToString(int[] direction)
    {
        if (direction[0] == 0 && direction[1] == 1)
        {
            return "east";
        }
        if (direction[0] == 0 && direction[1] == -1)
        {
            return "west";
        }
        if (direction[0] == 1 && direction[1] == 0)
        {
            return "south";
        }
        if (direction[0] == -1 && direction[1] == 0)
        {
            return "north";
        }
        return "";
    }

    public int[] stringToDirection(string direction)
    {
        switch (direction)
        {
            case "east":
                return new int[] { 0, 1 };
            case "west":
                return new int[] { 0, -1 };
            case "north":
                return new int[] { -1, 0 };
            case "south":
                return new int[] { 1, 0 };
        }
        return new int[] { 0, 0 };
    }

    public int convertDirection(int cord, bool seconDoor)
    {
        if (cord == 0)
        {
            if (seconDoor)
                return (int)(size / 2) - 1;
            else
                return (int)(size / 2);
        }
        else if (cord == 1)
        {
            return size - 1;
        }
        else
        {
            return 0;
        }
    }

    // Generate room tiles 
    public void generateTiles()
    {
        // Get a template of the map
        // IRL Settings
        if (BoardManagerSystem.instance.isGuided)
        //if(false)
        {
            tileMap = TemplateMap.generateStaticWallMap(size, size);
        }
        else
        {
            tileMap = TemplateMap.generateWallMap(size, size);
        }

        if (id == 0 && !BoardManagerSystem.instance.isTraning && BoardManagerSystem.instance.sceneName != "StartScene")
            tileMap = TemplateMap.generateStartingRoom();

        resetDoors();

        bool reset = false;
        // Generate doors
        foreach (KeyValuePair<string, Room> rn in roomNeigh)
        {
            if (rn.Value == null)
                continue;

            int[] cords = stringToDirection(rn.Key);

            int[] doorCords1 = new int[2];
            int[] doorCords2 = new int[2];

            doorCords1[0] = convertDirection(cords[0], false);
            doorCords1[1] = convertDirection(cords[1], false);

            doorCords2[0] = convertDirection(cords[0], true);
            doorCords2[1] = convertDirection(cords[1], true);

            // If the room has not the tiles for the door, set reset to true
            if (tileMap[doorCords1[0], doorCords1[1]] <= 0 || tileMap[doorCords2[0], doorCords2[1]] <= 0)
            {
                reset = true;
                break;
            }

            tileMap[doorCords1[0], doorCords1[1]] = 2;
            tileMap[doorCords2[0], doorCords2[1]] = 2;
            // Instantiate door
            addDoors(cords, new List<int[]> { doorCords1, doorCords2 });
        }

        // if reset is true, restart the generation
        if (reset)
        {
            generateTiles();
            return;
        }
        else
        {
            // If is not the starting room, create the items
            if (id != 0 || BoardManagerSystem.instance.isTraning || BoardManagerSystem.instance.sceneName == "StartScene")
            {
                // IRL setting
                // if(!BoardManagerSystem.instance.isGuided)
                spawnItems();
            }


            // Remove the room errors
            removeError();
            // Instantiate the tile and the objects
            instantiateTiles();
            // Instantiate the wall of the room
            if (!BoardManagerSystem.instance.isTraning)
                instantiateRoomWall();

            // If is the starting room and is not level 1, add the altar
            if (id == 0 && BoardManagerSystem.instance.level != 1 && !BoardManagerSystem.instance.isTraning && BoardManagerSystem.instance.sceneName != "StartScene")
                createAltar();

            // If the level > 3, create the chest
            if (id != 0 && BoardManagerSystem.instance.level >= 3)
                createChest();
        }
    }

    // Create and instantiate the altar
    public void createAltar()
    {
        GameObject altar = BoardManagerSystem.instance.altarPool.getPooledObject();
        Entity altarEntity = altar.GetComponent<GameObjectEntity>().Entity;
        Vector3 spawnPosition = new Vector3((size / 2) - 1 + coords[0], 0, size / 2 + 2 + coords[1]);
        altar.transform.position = spawnPosition;
        getTile((size / 2) - 1, (size / 2) + 2).setInteractable(altar);
        getTile(size / 2, (size / 2) + 2).setInteractable(altar);
        var em = World.Active.GetExistingManager<EntityManager>();
        // The number of point depends on the difficulty chosen
        em.SetComponentData(altarEntity, new Altar { actualPoints = BoardManagerSystem.difficulty, startingPoints = BoardManagerSystem.difficulty });
    }

    // Create and instantiate the chest with some probabilty
    public void createChest()
    {

        if (Random.Range(0f, 1f) > BoardManagerSystem.instance.chestProbability)
        {
            return;
        }

        int x = Random.Range(0, size);
        int y = Random.Range(0, size);

        while (tileMap[x, y] != 1)
        {
            x = Random.Range(0, size);
            y = Random.Range(0, size);
        }

        GameObject chest = BoardManagerSystem.instance.instantiateChest();
        Vector3 spawnPosition = new Vector3(x + coords[0], 0, y + coords[1]);
        chest.transform.position = spawnPosition;
        getTile(x, y).setInteractable(chest);
        others.Add(chest);
    }

    // Instantiate tile
    public void instantiateTiles()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (tileMap[x, y] < 0)
                {
                    continue;
                }
                Tile tile = BoardManagerSystem.instance.instantiateTile(x, y, tileMap[x, y], this);
                board.addTile(tile, x, y, this);
                tiles[x, y] = tile;

                if (tileMap[x, y] == 4)
                {
                    items.Add(tile.getItem());
                }
            }
        }
    }

    // Add to each tile all of its neghbours
    public void GenerateNeighbours()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Tile tile = getTile(x, y);
                if (tile == null)
                    continue;

                bool wallUp = false;
                bool wallDown = false;
                bool wallRight = false;
                bool wallLeft = false;

                if (getTile(x + 1, y) == null)
                {
                    wallUp = true;
                }

                if (getTile(x, y + 1) == null)
                {
                    wallRight = true;
                }

                if (getTile(x, y - 1) == null)
                {
                    wallLeft = true;
                }

                if (getTile(x - 1, y) == null)
                {
                    wallDown = true;
                }

                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        if (getTile(x + i, y + j) == null)
                            continue;

                        if ((wallUp && i > 0) || (wallDown && i < 0)
                            || (wallLeft && j < 0) || (wallRight && j > 0))
                            continue;

                        tile.addNeighbour(getTile(x + i, y + j));
                    }
            }
        }
    }

    // Add to this room all of its neighbours
    public void generateRoomLink()
    {
        foreach (KeyValuePair<string, List<int[]>> d in doorTiles)
        {
            if (d.Value == null)
            {
                continue;
            }

            string currentDirString = d.Key;
            int[] currentDir = stringToDirection(currentDirString);
            List<int[]> currentDoors = d.Value;

            Room nextRoom = roomNeigh[currentDirString];

            int[] nextDir = new int[2];
            nextDir[0] = currentDir[0] * -1;
            nextDir[1] = currentDir[1] * -1;

            List<int[]> nextDoors = nextRoom.getDoorTiles()[directionToString(nextDir)];

            foreach (int[] coords in currentDoors)
            {
                foreach (int[] nextCoords in nextDoors)
                {
                    Tile t = getTile(coords[0], coords[1]);
                    Tile nextT = nextRoom.getTile(nextCoords[0], nextCoords[1]);
                    t.addNeighbour(nextT);
                }
            }
        }
    }

    public Tile getTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size || y >= size)
        {
            return null;
        }
        return tiles[x, y];
    }

    // Get a random tile from the room
    public Tile getRandomTile()
    {
        Tile t = getTile(Random.Range(0, size), Random.Range(0, size));
        return t;
    }

    // Spawn objects and loots
    public void spawnItems()
    {
        int roomSize = 0;
        foreach (int t in tileMap)
        {
            if (t == 1)
            {
                roomSize++;
            }
        }

        // Get the max and min number of both objects and loots
        float maxNumberObject = BoardManagerSystem.instance.maxNumberObject;
        float minNumberObject = BoardManagerSystem.instance.minNumberObject;
        float maxNumberLoot = BoardManagerSystem.instance.maxNumberLoot;
        float minNumberLoot = BoardManagerSystem.instance.minNumberLoot;

        int maxObject = (int)Mathf.Ceil((float)roomSize * maxNumberObject);
        int minObject = (int)Mathf.Ceil((float)roomSize * minNumberObject);

        int maxLoot = (int)Mathf.Ceil((float)roomSize * maxNumberLoot);
        int minLoot = (int)Mathf.Ceil((float)roomSize * minNumberLoot);

        int numberOfObjects = Random.Range(minObject, maxObject + 1);
        int numberOfLoot = Random.Range(minLoot, maxLoot + 1);

        // Create the objects in freed tiles
        for (int i = 0; i < numberOfObjects; i++)
        {
            int x = Random.Range(1, size - 1);
            int y = Random.Range(1, size - 1);

            int tries = 0;

            while (tileMap[x, y] != 1 && tries < roomSize)
            {
                x = Random.Range(1, size - 1);
                y = Random.Range(1, size - 1);
                tries++;
            }

            if (tries >= roomSize - 1)
            {
                continue;
            }

            tileMap[x, y] = 3;

        }

        // Create the loots in freed tiles
        for (int i = 0; i < numberOfLoot; i++)
        {

            int x = Random.Range(1, size - 1);
            int y = Random.Range(1, size - 1);

            int tries = 0;

            while (tileMap[x, y] != 1 && tries < roomSize)
            {
                x = Random.Range(1, size - 1);
                y = Random.Range(1, size - 1);
                tries++;
            }
            if (tries >= roomSize - 1)
            {
                continue;
            }

            tileMap[x, y] = 4;
        }
    }

    // Return true if the room has an active enemy
    public bool hasEnemy()
    {
        var em = World.Active.GetExistingManager<EntityManager>();
        foreach (GameObject enemy in enemies)
        {
            Entity enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
            if (!em.HasComponent<Death>(enemyEntity))
            {
                return true;
            }
        }
        return false;
    }

    // Compute the number of enemies that will spawn in this room; the number
    // depend on the free tiles of the room
    public void computeNumEnemies()
    {
        if (id == 0)
            return;

        // Count the free tiles
        int freeTiles = 0;
        foreach (int t in tileMap)
        {
            if (t == 1)
            {
                freeTiles++;
            }
        }

        float freeTilesPerc = (float)freeTiles / ((float)size * size);

        numEnemies = 1;

        // If level >= 3 and the free tiles are the 70% of all the tiles, then 
        // spawn 2 enemies
        if (BoardManagerSystem.instance.level >= 3)
        {
            if (freeTilesPerc > 0.7f)
            {
                numEnemies = 2;
            }
        }

        // If level >= 6 and the free tiles are the 80% of all the tiles, then 
        // spawn 3 enemies; if free tiles are the 60% of all the tiles, then 
        // spawn 2 enemies
        if (BoardManagerSystem.instance.level >= 6)
        {
            if (freeTilesPerc > 0.8f)
            {
                numEnemies = 3;
            }
            else if (freeTilesPerc > 0.6f)
            {
                numEnemies = 2;
            }
        }

        BoardManagerSystem.instance.numEnemies += numEnemies;
    }

    // Instantiate the walls and the doors of the room
    public void instantiateRoomWall()
    {
        var em = World.Active.GetExistingManager<EntityManager>();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool wallUp = false;
                bool wallRight = false;
                bool wallLeft = false;
                bool wallDown = false;
                int realX = x + coords[0];
                int realY = y + coords[1];
                Tile t = tiles[x, y];

                if (t == null)
                {
                    continue;
                }

                if (getTile(x + 1, y) == null)
                {
                    if ((t.isDoor && (y != 0 && y != size - 1)))
                    {
                        if (y == size / 2 - 1)
                        {
                            GameObject door = BoardManagerSystem.instance.rightDoorPool.getPooledObject();
                            for (int c = 0; c < door.transform.childCount; c++)
                            {
                                StandardShaderUtils.ChangeRenderMode(door.transform.GetChild(c).GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
                            }
                            Vector3 spawnPosition = new Vector3(realX + 0.5f, door.transform.position.y, realY + 0.5f);
                            door.transform.position = spawnPosition;
                            walls.Add(door);

                            board.doors[realX, realY] = door;
                            board.doors[realX, realY + 1] = door;
                        }
                    }
                    else
                    {
                        wallUp = true;
                        GameObject wall = BoardManagerSystem.instance.rightWallPool.getPooledObject();
                        Vector3 spawnPosition = new Vector3(realX, wall.transform.position.y, realY);
                        wall.transform.position = spawnPosition;
                        foreach (var gameObjectEntity in wall.GetComponentsInChildren<GameObjectEntity>())
                        {
                            em.SetComponentData(gameObjectEntity.Entity, new WallPosition { x = realX, y = realY });
                        }
                        walls.Add(wall);
                    }
                }

                if (getTile(x, y + 1) == null)
                {
                    if ((t.isDoor && (x != 0 && x != size - 1)))
                    {
                        if (x == size / 2 - 1)
                        {
                            GameObject door = BoardManagerSystem.instance.leftDoorPool.getPooledObject();
                            for (int c = 0; c < door.transform.childCount; c++)
                            {
                                StandardShaderUtils.ChangeRenderMode(door.transform.GetChild(c).GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
                            }
                            Vector3 spawnPosition = new Vector3(realX + 0.5f, door.transform.position.y, realY + 0.5f);
                            door.transform.position = spawnPosition;
                            walls.Add(door);

                            board.doors[realX, realY] = door;
                            board.doors[realX + 1, realY] = door;
                        }
                    }
                    else
                    {
                        wallRight = true;
                        GameObject wall = BoardManagerSystem.instance.leftWallPool.getPooledObject();
                        Vector3 spawnPosition = new Vector3(realX, wall.transform.position.y, realY);
                        wall.transform.position = spawnPosition;


                        foreach (var gameObjectEntity in wall.GetComponentsInChildren<GameObjectEntity>())
                        {
                            em.SetComponentData(gameObjectEntity.Entity, new WallPosition { x = realX, y = realY });
                        }

                        walls.Add(wall);
                    }
                }

                if (getTile(x, y - 1) == null)
                {
                    if ((t.isDoor && (x != 0 && x != size - 1)))
                    {
                        if (x == size / 2)
                        {
                            GameObject door = BoardManagerSystem.instance.leftDoorPool.getPooledObject();
                            for (int c = 0; c < door.transform.childCount; c++)
                            {
                                StandardShaderUtils.ChangeRenderMode(door.transform.GetChild(c).GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
                            }
                            Vector3 spawnPosition = new Vector3(realX - 0.5f, door.transform.position.y, realY - 0.5f);
                            door.transform.position = spawnPosition;
                            door.transform.Rotate(new Vector3(0, 180, 0));
                            walls.Add(door);

                            board.doors[realX, realY] = door;
                            board.doors[realX - 1, realY] = door;
                        }
                    }
                    wallLeft = true;
                }

                if (getTile(x - 1, y) == null)
                {
                    if ((t.isDoor && (y != 0 && y != size - 1)))
                    {
                        if (y == size / 2)
                        {
                            GameObject door = BoardManagerSystem.instance.rightDoorPool.getPooledObject();
                            for (int c = 0; c < door.transform.childCount; c++)
                            {
                                StandardShaderUtils.ChangeRenderMode(door.transform.GetChild(c).GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
                            }

                            Vector3 spawnPosition = new Vector3(realX - 0.5f, door.transform.position.y, realY - 0.5f);
                            door.transform.Rotate(new Vector3(0, 180, 0));
                            door.transform.position = spawnPosition;

                            board.doors[realX, realY] = door;
                            board.doors[realX, realY - 1] = door;

                            walls.Add(door);
                        }
                    }
                    wallDown = true;
                }

                GameObject corner = null;
                Vector3 cornerPosition;

                if (wallUp && wallRight)
                {
                    corner = BoardManagerSystem.instance.cornerWallPool.getPooledObject();
                    cornerPosition = new Vector3(realX + 0.5f, corner.transform.position.y, realY + 0.5f);
                    corner.transform.position = cornerPosition;
                    walls.Add(corner);
                }

                if (wallLeft && wallUp)
                {
                    corner = BoardManagerSystem.instance.cornerWallPool.getPooledObject();
                    cornerPosition = new Vector3(realX + 0.5f, corner.transform.position.y, realY - 0.5f);
                    corner.transform.position = cornerPosition;
                    walls.Add(corner);
                }

                if (wallRight && wallDown)
                {
                    corner = BoardManagerSystem.instance.cornerWallPool.getPooledObject();
                    cornerPosition = new Vector3(realX - 0.5f, corner.transform.position.y, realY + 0.5f);
                    corner.transform.position = cornerPosition;
                    walls.Add(corner);
                }
            }
        }
    }

    // Obscure all the tiles of the room. To obscure all the tiles, disable the
    // renderer of all the objects 
    public void obscureTiles()
    {
        if (!isLighted)
        {
            return;
        }
        foreach (Tile t in tiles)
        {
            if (t == null)
            {
                continue;
            }
            t.GetComponent<Renderer>().enabled = false;
            foreach (var r in t.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            //t.gameObject.SetActive(false);
            if (t.isDoor)
            {
                foreach (Tile door in t.getNeighbours())
                {
                    door.GetComponent<Renderer>().enabled = false;
                    foreach (var r in door.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = false;
                    }
                }
            }
        }

        foreach (Item i in items)
        {
            i.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        foreach (GameObject w in walls)
        {
            for (int o = 0; o < w.transform.childCount; o++)
            {
                w.transform.GetChild(o).GetComponent<MeshRenderer>().enabled = false;
            }
        }

        foreach (GameObject e in enemies)
        {
            e.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }

        foreach (GameObject o in others)
        {
            o.gameObject.SetActive(false);
        }

        isLighted = false;
    }

    // Light all the tiles of the room. To light them, enable the
    // renderer of all the objects 
    public void lightTiles()
    {
        if (isLighted)
        {
            return;
        }

        foreach (Tile t in tiles)
        {
            if (t == null)
            {
                continue;
            }
            t.GetComponent<Renderer>().enabled = true;
            foreach (var r in t.GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
            //t.gameObject.SetActive(false);
            if (t.isDoor)
            {
                foreach (Tile door in t.getNeighbours())
                {
                    door.GetComponent<Renderer>().enabled = true;
                    foreach (var r in door.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = true;
                    }
                }
            }
        }

        foreach (Item i in items)
        {
            i.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }

        foreach (GameObject w in walls)
        {
            for (int o = 0; o < w.transform.childCount; o++)
            {
                w.transform.GetChild(o).GetComponent<MeshRenderer>().enabled = true;
            }
        }

        foreach (GameObject e in enemies)
        {
            e.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }

        foreach (GameObject o in others)
        {
            o.gameObject.SetActive(true);
        }

        isLighted = true;
    }

    public void createEnemies()
    {
        if (isEntered)
            return;

        if (id != 0)
        {
            enemies = BoardManagerSystem.instance.createEnemies(this, numEnemies);
        }

        isEntered = true;
    }

    // Remove all the errors in the tile map for which a character can remain stuck
    public void removeError()
    {

        for (int x = 1; x < size - 1; x++)
            for (int y = 1; y < size - 1; y++)
            {
                int tile = tileMap[x, y];
                if (tile == 3)
                {

                    if (tileMap[x + 1, y - 1] <= 0 || tileMap[x, y - 1] <= 0 || tileMap[x - 1, y - 1] <= 0 ||
                        tileMap[x + 1, y - 1] == 3 || tileMap[x, y - 1] == 3 || tileMap[x - 1, y - 1] == 3)
                    {
                        if (tileMap[x, y + 1] == 3 || tileMap[x, y + 1] <= 0)
                        {
                            tileMap[x, y] = 1;
                        }
                    }
                }
            }

        for (int y = 1; y < size - 1; y++)
            for (int x = 1; x < size - 1; x++)
            {
                int tile = tileMap[x, y];
                if (tile == 3)
                {
                    if (tileMap[x + 1, y - 1] <= 0 || tileMap[x + 1, y] <= 0 || tileMap[x + 1, y + 1] <= 0 ||
                        tileMap[x + 1, y - 1] == 3 || tileMap[x + 1, y] == 3 || tileMap[x + 1, y + 1] == 3)

                    {
                        if (tileMap[x - 1, y] == 3 || tileMap[x - 1, y] <= 0)
                        {
                            tileMap[x, y] = 1;
                        }
                    }
                }
            }
    }
}
