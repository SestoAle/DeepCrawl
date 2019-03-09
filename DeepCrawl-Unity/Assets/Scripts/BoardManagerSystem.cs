using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using MLAgents;

enum TileRotation
{
  North = 0,
  East = 90,
  South = 180,
  West = 270
}

public enum EnemyClass
{
  Archer,
  Warrior,
  Ranger
}

public class BoardManagerSystem : MonoBehaviour
{
  Board board;
  [Header("---Board information---")]
  public int numRooms = 1;
  public int currentRoomId = 99;
  public bool obscureAll = true;
  public static int difficulty = 4;
  //public GameObject outerWallPrefab;
  //public GameObject cornerWallPrefab;
  //public GameObject doorPrefab;
  //public GameObject windowPrefab;
  public Material doorOpaqueMaterial;
  public Material doorFadeMaterial;
  [Space(10)]

  [Header("---Progress information---")]
  public int level = 1;
  public int currentTurn = 0;
  [Space(10)]

  // Entity Information
  EntityManager entityManager;

  [Header("---Objects information---")]
  public float maxNumberObject = 0.08f;
  public float minNumberObject = 0.04f;
  public float maxNumberLoot = 0.08f;
  public float minNumberLoot = 0.04f;
  public float chestProbability = 0.10f;
  public List<GameObject> loots;
  public GameObject chestPrefab;
  [Space(10)]

  // Pools
  [Header("---Pools---")]
  public ObjectPool tilePool;
  public RandomObjectPool tileDirtPool;
  public ObjectPool wallPool;
  public ObjectPool leftWallPool;
  public ObjectPool rightWallPool;
  public ObjectPool leftDoorPool;
  public ObjectPool rightDoorPool;
  public ObjectPool leftWindowPool;
  public ObjectPool rightWindowPool;
  public ObjectPool cornerWallPool;
  public ObjectPool roomHorWallPool;
  public ObjectPool roomVerWallPool;
  public ObjectPool cornerHorWallPool;
  public ObjectPool cornerVerWallPool;
  public ObjectPool altarPool;
  public ObjectPool chestPool;
  public GameObject lootPoolsContainer;
  public List<ObjectPool> lootPools;
  public RandomObjectPool itemPool;
  public EnemyPool enemyPool;
  [Space(10)]

  // Item informations
  [HideInInspector]
  public GameObject OuterContainer;
  [HideInInspector]
  public GameObject PlayersContainer;
  [HideInInspector]
  public GameObject Level;

  //ML informations
  [Header("---ML Informations---")]
  public Brain trainBrain;
  public Brain doubleAgentBrain;
  public RogueAcademy academy;
  public bool isTraning = false;
  public bool player = false;
  public int doubleSize = 5;
  public int tripleSize = 3;
  public bool doubleAgent = false;
  public bool noAnim = true;
  [Space(10)]

  // TODO: Change this
  [Header("---Enemies Informations---")]
  public Brain ArcherBrain;
  public Brain WarriorBrain;
  public Brain RangerBrain;

  // TODO: Change this
  // Enemies Prefab
  public GameObject ArcherPrefab;
  public GameObject WarriorPrefab;
  public GameObject RangerPrefab;
  [Space(10)]

  [Header("---Start Inventory Informations---")]
  public MeleeWeapon[] startSwords;
  public RangeWeapon[] startBows;
  public Potion[] startPotions;
  public MeleeWeapon sword;
  public RangeWeapon bow;
  [Space(10)]

  [Header("---Characters Informations---")]
  public GameObject playerPrefab;
  public GameObject trainingPrefab;
  public GameObject enemyPrefab;
  [HideInInspector]
  public GameObject activePlayer;
  [HideInInspector]
  public Stats playerStats;
  public int numPlayers = 1;
  public int numAgents = 0;
  // TODO: change this
  [HideInInspector]
  public int numEnemies = 0;
  [HideInInspector]
  public int deadEnemies = 0;
  [HideInInspector]
  public int turnCount = 0;
  [HideInInspector]
  public int totalPlayers = 1;
  [Space(10)]

  [HideInInspector]
  public static BoardManagerSystem instance;

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

    // Create the loot pools only once the game started
    if (lootPools.Count <= 0)
    {
      createLootPools();
    }

    // In training, disable job system, they are useless
    if (isTraning)
    {
      World.Active.GetExistingManager<WallSystem>().Enabled = false;
      World.Active.GetExistingManager<ItemSystem>().Enabled = false;
      World.Active.GetExistingManager<WallBarrier>().Enabled = false;
      World.Active.GetExistingManager<ItemBarrier>().Enabled = false;
      World.Active.GetExistingManager<LongMovementSystem>().Enabled = false;
      World.Active.GetExistingManager<DoorSystem>().Enabled = false;
    }
  }

  // Use this for initialization
  void Start()
  {
    if (!isTraning)
      PlayersContainer = new GameObject("PlayersContainer");
    // Initialize all the variables used in the systems
    numEnemies = 0;
    turnCount = 0;
    currentTurn = 0;
    deadEnemies = 0;
    instance = this;
    // Get the entityManager
    entityManager = World.Active.GetExistingManager<EntityManager>();
    // Create Level object and all of containers
    Level = new GameObject("Level");
    OuterContainer = new GameObject("OuterContainer");
    OuterContainer.transform.parent = Level.transform;
    if (isTraning)
    {
      numPlayers = 0;
      level = 0;
    }
    else
    {
      noAnim = false;
      numAgents = 0;
      doubleAgent = false;
    }
    // Get the total number of characters
    totalPlayers = numPlayers + numAgents;
    // Create battle board
    generateBoard();
    if (!isTraning)
    {
      deSelectAll();
      deHighlightAll();
    }
    // Create characters on the board
    generateCharacters();
    GameManager.instance.gameUI.resetText();
    if (!isTraning)
      GameManager.instance.gameUI.moveCameraAtPosition(activePlayer.transform.position.x, activePlayer.transform.position.y);
    else
      GameManager.instance.gameUI.moveCameraAtPosition(15, 15);
  }

  void generateBoard()
  {
    // The number of the rooms is equal to the level + 1
    numRooms = level + 1;
    // Create all the container
    if (PlayersContainer == null)
      PlayersContainer = new GameObject("PlayersContainer");
    PlayersContainer.transform.parent = Level.transform;
    if (!isTraning)
      currentRoomId = 99;
    else
      currentRoomId = 0;
    // Create the board
    board = new Board(numRooms, 10);
  }

  // Create the loot pools
  public void createLootPools()
  {
    // Initialize list of loot objects pools
    lootPools = new List<ObjectPool>();
    // Sort the loots by their spawn probability
    loots = loots.OrderBy(o => o.GetComponent<Item>().spawnProbability).ToList();
    foreach (GameObject loot in loots)
    {
      // For each loots, create the pool
      GameObject lootPoolObject = new GameObject();
      lootPoolObject.name = loot.name + "Pool";
      lootPoolObject.transform.parent = lootPoolsContainer.transform;
      ObjectPool lootPool = lootPoolObject.AddComponent<ObjectPool>() as ObjectPool;

      // Initialize the pool
      lootPool.count = 10;
      lootPool.prefab = loot;
      lootPool.initialize();

      lootPools.Add(lootPool);
    }
  }

  // Get a random loot from the loot lists with cumulative probs technique 
  // (each loot must have a spawn probability)
  public GameObject getRandomLootWithProb()
  {
    // Compute the cumulative prob
    float total = loots.Sum(i => i.GetComponent<Item>().spawnProbability);
    // Choose a random number between 0 and the sum
    float r = Random.Range(0.0f, total);
    float sum = 0f;
    // For each item in list
    for (int i = 0; i < loots.Count; i++)
    {
      // Get GameObject from loots pool
      GameObject loot = loots[i];

      // Add to sum its spawn prob
      sum += loot.GetComponent<Item>().spawnProbability;

      // If the random number is less then the sum at this point
      if (r <= sum)
      {
        // Get the pool of the loot
        ObjectPool pool = lootPools[i];
        // return the loot from the previous pool
        return pool.getPooledObject();
      }
    }
    return null;
  }

  // Get a total random loot
  public GameObject getRandomLoot()
  {
    // Get a random pool from loot pools
    ObjectPool pool = lootPools[Random.Range(0, lootPools.Count)];

    // Return the gameobject from the previous pool
    return pool.getPooledObject();
  }

  // Get a random tile from the board
  Tile getRandomTile()
  {
    return board.getRandomTile();
  }

  // Generate both enemies and players
  void generateCharacters()
  {
    if (!isTraning)
    {
      // Instantiate the players in the starting room
      for (int player = 0; player < numPlayers; player++)
      {
        Room startingRoom = getRoom(0);

        Tile tile = startingRoom.getTile(startingRoom.getSize() / 2, startingRoom.getSize() / 2);

        while (tile == null || !tile.canMove())
        {
          tile = startingRoom.getRandomTile();
        }

        int x = tile.x;
        int y = tile.y;

        activePlayer = createCharacter(playerPrefab, x, y, player, DIRECTION.North);
        if (level != 1)
        {
          playerStats.hp = playerStats.maxHp;
          entityManager.SetComponentData(activePlayer.GetComponent<Character>().Entity, playerStats);
        }

        entityManager.SetComponentData(activePlayer.GetComponent<Character>().Entity, new Position { x = x, y = y });

        activePlayer.name = "Player " + (player + 1);
        activePlayer.tag = "Player";
      }
    }
    else
    {
      // Instantiate the actors of the training in random tiles
      for (int enemy = 0; enemy < numAgents; enemy++)
      {

        // TODO: move this to an other (new) method (only for training)?
        Tile tile = getRandomTile();

        while (!tile.canMove())
        {
          tile = getRandomTile();
        }

        int x = tile.x;
        int y = tile.y;

        GameObject character = null;

        if (enemy == 1 && !doubleAgent)
        {
          character = createCharacter(trainingPrefab, x, y, numPlayers + enemy, DIRECTION.South);
          activePlayer = character;
        }
        else
        {
          character = createCharacter(enemyPrefab, x, y, numPlayers + enemy, DIRECTION.South);
        }

        character.name = "Enemy " + (enemy + 1);
        numEnemies++;

      }
      // Set the target to each character
      GameObject.Find("Enemy 1").GetComponent<Character>().target = GameObject.Find("Enemy 2");
      GameObject.Find("Enemy 2").GetComponent<Character>().target = GameObject.Find("Enemy 1");

      if (player)
        GameObject.Find("Enemy 2").tag = "Player";
      else
        GameObject.Find("Enemy 2").tag = "Random";

      GameObject.Find("Enemy 1").tag = "BrainedAgent";
      // Change position of the character
      randomSpawn(GameObject.Find("Enemy 1"));
      randomSpawn(GameObject.Find("Enemy 2"));

      GameObject.Find("Enemy 1").GetComponent<Agent>().GiveBrain(trainBrain);

      // Use this for agent vs agent
      if (doubleAgent)
      {
        GameObject.Find("Enemy 2").GetComponent<Agent>().GiveBrain(doubleAgentBrain);
      }
    }
  }

  // Move a character in a random position of the map. It is used only for 
  // training.
  public void randomSpawn(GameObject character)
  {
    // Get starting Room coordinates
    Room startRoom = board.getStartRoom();
    int[] startRoomCoords = startRoom.getCoords();

    // Get 2 random positions and the tile
    int x = Random.Range(startRoomCoords[0], startRoomCoords[0] + startRoom.getSize());
    int y = Random.Range(startRoomCoords[1], startRoomCoords[1] + startRoom.getSize());
    Tile t = getTile(x, y);

    // Get a new random tile if the previous was occupied or can't move from there
    while (t == null || !t.canMove() || t.hasItem() || !t.canMoveFromHere())
    {
      x = Random.Range(startRoomCoords[0], startRoomCoords[0] + startRoom.getSize());
      y = Random.Range(startRoomCoords[1], startRoomCoords[1] + startRoom.getSize());

      t = getTile(x, y);
    }

    character.transform.position = new Vector3(x, 0, y);
    t.setCharacter(character);
    entityManager.SetComponentData(character.GetComponent<Character>().Entity, new Position { x = x, y = y });
  }

  // Instatiate character object
  public GameObject createCharacter(GameObject prefab, int x, int y, int turn, DIRECTION dir)
  {
    // Instantiate the character prefab with the GameObjectEntity Component
    GameObject character = Instantiate(prefab);

    // Get the spawn position of the character
    Vector3 spawnPosition = new Vector3(x, character.transform.position.y, y);
    character.transform.position = spawnPosition;

    // Set parent
    character.transform.parent = PlayersContainer.transform;
    // Set the character as object in the start tile
    getTile(x, y).setCharacter(character);

    // Get GameObjectEntity and add components
    var entity = character.GetComponent<GameObjectEntity>().Entity;

    // Add the component to the entity (the first player has the turn)
    entityManager.AddComponentData(entity, new Turn { index = turn, hasTurn = 0, hasEndedTurn = 0 });
    entityManager.AddBuffer<MovementElementBuffer>(entity);

    // Rotate the character to face its enemy
    entityManager.AddComponentData(entity, new Rotation { rotationY = (int)dir });

    return character;
  }

  // Get the tile where an object stands
  public Tile getTileFromObject(GameObject Object)
  {
    Transform objTransform = Object.GetComponent<Transform>();
    return getTile((int)objTransform.position.x, (int)objTransform.position.z);
  }

  // Get tile at position
  public Tile getTile(int x, int y)
  {
    return board.getTile(x, y);
  }

  // Get a room with ID
  public Room getRoom(int id)
  {
    foreach (Room r in board.rooms)
    {
      if (r != null && r.getId() == id)
        return r;
    }
    return null;
  }

  // Get a door with position
  public GameObject getDoor(int x, int y)
  {
    return board.getDoor(x, y);
  }

  // Oscure all the rooms
  public void obscureAllRoom()
  {
    foreach (Room r in board.rooms)
    {
      if (r != null)
        r.obscureTiles();
    }
  }

  // Set default material to all tile of the map
  public void deHighlightAll()
  {
    foreach (Tile tile in board.map)
    {
      if (tile != null)
        tile.deHighlight();
    }
  }

  // Set default material to all tile of the room 
  public void deHighlightAll(Room room)
  {
    foreach (Tile tile in room.getTiles())
    {
      if (tile != null)
      {
        tile.deHighlight();
        if (tile.isDoor)
        {
          foreach (Tile door in tile.getNeighbours())
          {
            door.deHighlight();
          }
        }
      }
    }
  }

  // Deselect all the tile in the map
  public void deSelectAll()
  {
    foreach (Tile tile in board.map)
    {
      if (tile != null)
        tile.deSelectTile();
    }
  }

  // Reset the set-up of the train. To use only when in the training phase
  public void resetTraining()
  {
    minNumberLoot = academy.resetParameters["minNumLoot"];
    maxNumberLoot = academy.resetParameters["maxNumLoot"];

    // Initialize all the pools
    tilePool.destroyAllObjects();
    tileDirtPool.destroyAllObjects();
    wallPool.destroyAllObjects();
    itemPool.destroyAllObjects();
    leftWallPool.destroyAllObjects();
    rightWallPool.destroyAllObjects();
    leftWindowPool.destroyAllObjects();
    rightWindowPool.destroyAllObjects();
    cornerWallPool.destroyAllObjects();
    foreach (ObjectPool lootPool in lootPools)
    {
      lootPool.destroyAllObjects();
    }

    foreach(Tile t in board.getTiles())
    {
      if(t != null)
        t.setCharacter(null);
    }

    // Generate a new board
    generateBoard();

    // Reset UI
    if (!noAnim)
      GameManager.instance.gameUI.resetText();



    // Reset characters;
    for (int i = 0; i < numAgents; i++)
    {
      GameObject character = GameObject.Find("Enemy " + (i + 1)).gameObject;

      // Remove all action components to not get stuck or to not do a useless action
      if (entityManager.HasComponent<UserInput>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<UserInput>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<Movement>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Movement>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<Attack>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Attack>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<Wait>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Wait>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<Damage>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Damage>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<EndTurn>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<EndTurn>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<PopupComponent>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<PopupComponent>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<Buff>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Buff>(character.GetComponent<Character>().Entity);
      }
      if (entityManager.HasComponent<DoneComponent>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<DoneComponent>(character.GetComponent<Character>().Entity);
      }

      // Remove the Death component, if there is any
      if (entityManager.HasComponent<Death>(character.GetComponent<Character>().Entity))
      {
        entityManager.RemoveComponent<Death>(character.GetComponent<Character>().Entity);
        //character.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
      }

      Stats stats;

      if (character.tag == "BrainedAgent")
      {
        // Get the agent stats from the Academy parameters
        int hp = (int)academy.resetParameters["minAgentHp"];
        int maxHp = (int)academy.resetParameters["maxAgentHp"];
        int atk = (int)academy.resetParameters["agentAtk"];
        int def = (int)academy.resetParameters["agentDef"];
        int des = (int)academy.resetParameters["agentDes"];
        int actualHp = Random.Range(hp, maxHp + 1);
        stats = new Stats { hp = actualHp, atk = atk, def = def, des = des, maxHp = 20 };
        // Create a random inventory
        createStartInventory(character.GetComponent<Inventory>(), true);
      }
      else
      {
        // Get the target stats from the Academy parameters
        int hp = (int)academy.resetParameters["minTargetHp"];
        int maxHp = (int)academy.resetParameters["maxTargetHp"];
        int actualHp = Random.Range(hp, maxHp + 1);
        stats = new Stats { hp = actualHp, atk = 3, def = 3, des = 3, maxHp = 20 };
        // Create a random inventory
        createStartInventory(character.GetComponent<Inventory>(), true);
        //entityManager.SetComponentData(character.GetComponent<Character>().Entity, new Player { });
      }

      entityManager.SetComponentData(character.GetComponent<Character>().Entity,
                                     new Turn { index = i, hasTurn = 0, hasEndedTurn = 0 });

      entityManager.SetComponentData(character.GetComponent<Character>().Entity, stats);

      // Move the character in a random position
      randomSpawn(character);
    }

    currentTurn = 0;

    // Restart the agent (to avoid accademy error)
    if (GameObject.Find("Enemy 1").gameObject.GetComponent<BaseAgent>())
    {
      GameObject.Find("Enemy 1").gameObject.gameObject.GetComponent<BaseAgent>().AgentRestart();
    }

    // Use this for agent vs agent
    if (doubleAgent)
    {
      GameObject.Find("Enemy 2").GetComponent<Agent>().GiveBrain(doubleAgentBrain);
    }
  }

  // Reset all the game
  public void resetAllGame()
  {
    playerStats = entityManager.GetComponentData<Stats>(activePlayer.GetComponent<Character>().Entity);
    if (entityManager.HasComponent<Buff>(activePlayer.GetComponent<GameObjectEntity>().Entity))
    {
      Buff buff = entityManager.GetComponentData<Buff>(activePlayer.GetComponent<GameObjectEntity>().Entity);
      playerStats = removeBuff(playerStats, buff);
    }
    Destroy(PlayersContainer);
    tilePool.destroyAllObjects();
    tileDirtPool.destroyAllObjects();
    wallPool.destroyAllObjects();
    itemPool.destroyAllObjects();
    leftWallPool.destroyAllObjects();
    rightWallPool.destroyAllObjects();
    leftWindowPool.destroyAllObjects();
    rightWindowPool.destroyAllObjects();
    cornerWallPool.destroyAllObjects();
    leftDoorPool.destroyAllObjects();
    rightDoorPool.destroyAllObjects();
    altarPool.destroyAllObjects();
    foreach (ObjectPool lootPool in lootPools)
    {
      lootPool.destroyAllObjects();
    }

    Start();
  }

  // Create the start inventory, randomly or not
  void createStartInventory(Inventory inventory, bool random)
  {
    if (!random)
    {
      inventory.setMelee(sword);
      inventory.setRange(bow);
      inventory.setPotion(null);
    }
    else
    {
      inventory.setMelee(startSwords[Random.Range(0, startSwords.Length)]);
      inventory.setRange(startBows[Random.Range(0, startBows.Length)]);
      inventory.setPotion(startPotions[Random.Range(0, startPotions.Length)]);
    }
  }

  // Instantiate tile of the room at position x,y of the type
  public Tile instantiateTile(int x, int y, int type, Room room)
  {
    int[] tileRotations = { 0, 90, 180, 270 };
    int realX = x + room.getCoords()[0];
    int realY = y + room.getCoords()[1];
    GameObject tile = null;
    switch (type)
    {
      // Column tile
      case 0:
        tile = wallPool.getPooledObject();
        foreach (var goe in tile.GetComponentsInChildren<GameObjectEntity>())
        {
          entityManager.SetComponentData(goe.Entity, new WallPosition { x = realX, y = realY });
        }
        break;
      // canMove tile
      case 1:
        tile = tilePool.getPooledObject();
        tile.GetComponent<BoxCollider>().enabled = true;
        tile.transform.rotation = Quaternion.Euler(new Vector3(0, tileRotations[Random.Range(0, tileRotations.Length)], 0));
        break;
      // canMove tile
      case 2:
        tile = tilePool.getPooledObject();
        tile.GetComponent<BoxCollider>().enabled = true;
        break;
      // Item tile
      case 3:
        tile = itemPool.getPooledObject();
        break;
      // Collectible tile
      case 4:
        tile = tilePool.getPooledObject();
        tile.GetComponent<Tile>().resetTile();
        tile.GetComponent<BoxCollider>().enabled = true;
        // Instantiate a random item
        GameObject spawnItem;
        if (isTraning)
        {
          spawnItem = getRandomLootWithProb();
        }
        else
        {
          spawnItem = getRandomLootWithProb();
        }
        // Set item in the tile
        tile.GetComponent<Tile>().setItem(spawnItem.GetComponent<Item>());
        // Set item spawn position
        Vector3 itemPosition = new Vector3(realX, spawnItem.transform.position.y, realY);
        spawnItem.GetComponent<Transform>().position = itemPosition;
        Entity itemEntity = spawnItem.GetComponent<GameObjectEntity>().Entity;
        entityManager.AddComponentData(itemEntity, new Position { x = (int)itemPosition.x, y = (int)itemPosition.z });
        break;
    }

    if (type != 4)
      tile.GetComponent<Tile>().resetTile();

    if (type == 2)
      tile.GetComponent<Tile>().isDoor = true;

    // Spawn at x,z position without touching y coordinate
    Vector3 spawnPosition = new Vector3(realX, tile.transform.position.y, realY);
    tile.transform.position = spawnPosition;

    // Set parent
    tile.name = "Tile " + realX + " " + realY;

    tile.GetComponent<Tile>().setParent(room);

    // Update position attribute in new tile
    tile.GetComponent<Tile>().setPosition(realX, realY);
    return tile.GetComponent<Tile>();
  }

  // Instatiate and return a list of count enemies
  public List<GameObject> createEnemies(Room room, int count)
  {
    List<GameObject> enemies = new List<GameObject>();
    System.Array classes = System.Enum.GetValues(typeof(EnemyClass));

    for (int i = 0; i < count; i++)
    {
      // Instantiate the character prefab with the GameObjectEntity Component
      GameObject character = null;
      EnemyClass enemyClass = (EnemyClass)classes.GetValue(Random.Range(0, classes.Length));
      // Switch depending of the class randomly chosen
      switch (enemyClass)
      {
        case (EnemyClass.Archer):
          character = Instantiate(ArcherPrefab);
          character.GetComponent<BaseAgent>().GiveBrain(ArcherBrain);
          character.name = "Archer Skeleton";
          break;
        case (EnemyClass.Warrior):
          character = Instantiate(WarriorPrefab);
          character.GetComponent<BaseAgent>().GiveBrain(WarriorBrain);
          character.name = "Warrior Skeleton";
          break;
        case (EnemyClass.Ranger):
          character = Instantiate(RangerPrefab);
          character.GetComponent<BaseAgent>().GiveBrain(RangerBrain);
          character.name = "Ranger Skeleton";
          break;
        default:
          character = Instantiate(RangerPrefab);
          character.GetComponent<BaseAgent>().GiveBrain(RangerBrain);
          break;
      }

      // Get the entity and add the Turn component
      Entity entity = character.GetComponent<Character>().Entity;
      entityManager.AddComponentData(entity, new Turn { index = turnCount + 1, hasEndedTurn = 0, hasTurn = 0 });

      // Get the spawn position of the character
      Tile spawnTile = null;
      while (spawnTile == null)
      {
        spawnTile = getEnemySpawnPosition(room);
      }
      Vector3 spawnPosition = new Vector3(spawnTile.x, character.transform.position.y, spawnTile.y);
      character.transform.position = spawnPosition;

      // Set parent
      character.transform.parent = PlayersContainer.transform;
      // Set the character as object in the start tile
      spawnTile.setCharacter(character);

      character.GetComponent<Character>().target = GameObject.Find("Player 1");

      turnCount++;
      totalPlayers++;

      // Modify the class stats depending on the level
      Stats defStats = entityManager.GetComponentData<Stats>(entity);
      Stats newStats = getStatsByLevel(1, defStats, count);
      // Update the stats component
      entityManager.SetComponentData(entity, newStats);
      entityManager.SetComponentData(entity, new Position { x = (int)spawnPosition.x, y = (int)spawnPosition.z });
      enemies.Add(character);
    }

    return enemies;
  }

  // Get the enemy spawn position depending on the player position; choose a 
  // random free tile in the row opposite to the player entrance
  public Tile getEnemySpawnPosition(Room room)
  {
    int playerX = 99;
    int playerY = 99;
    int size = room.getSize();
    for (int x = 0; x < size; x++)
      for (int y = 0; y < size; y++)
      {
        Tile t = room.getTile(x, y);
        if (t == null)
          continue;
        GameObject character = (GameObject)t.getCharacter();
        if (t.hasCharacter() && character.tag == "Player")
        {
          playerX = x;
          playerY = y;
        }
      }

    Tile enemyTile = null;

    if (playerX == 0 || playerX == size - 1)
    {
      enemyTile = room.getTile(size - 1 - playerX, Random.Range(0, size));
      int i = 1;
      int count = 0;
      while (enemyTile == null || !enemyTile.canMove())
      {
        if (count > size * size)
        {
          count = 0;
          if (playerX == 0)
            i++;
          else
            i--;
        }
        enemyTile = room.getTile(size - i - playerX, Random.Range(0, size));
        count++;
      }
    }

    if (playerY == 0 || playerY == size - 1)
    {
      enemyTile = room.getTile(Random.Range(0, size), size - 1 - playerY);
      int i = 1;
      int count = 0;
      while (enemyTile == null || !enemyTile.canMove())
      {
        if (count > size * size)
        {
          count = 0;
          if (playerY == 0)
            i++;
          else
            i--;
        }
        enemyTile = room.getTile(Random.Range(0, size), size - i - playerY);
        count++;
      }
    }

    return enemyTile;
  }

  // Modify the stats depending on the current level
  public Stats getStatsByLevel(int type, Stats oldStas, int count)
  {
    int modHp;
    int modAtk;
    int modDes;
    int modDef;

    modHp = 2 * (level - 1) - (2 * (count - 1));
    modAtk = 1 * (level - 1) - (1 * (count - 1));
    modDes = 1 * (level - 1) - (1 * (count - 1));
    modDef = 1 * (level - 1) - (1 * (count - 1));

    Stats newStats = new Stats {
      maxHp = Mathf.Clamp(oldStas.maxHp + modHp, 10, 30),
      hp = Mathf.Clamp(oldStas.maxHp + modHp, 10, 30),
      des = Mathf.Clamp(oldStas.des + modDes, 0, 10),
      atk = Mathf.Clamp(oldStas.atk + modAtk, 0, 10),
      def = Mathf.Clamp(oldStas.def + modDef, 0, 10)
    };

    return newStats;
  }

  // Remove the acive buff 
  public Stats removeBuff(Stats stats, Buff buff)
  {
    stats.maxHp -= buff.hp;
    stats.def -= buff.def;
    stats.atk -= buff.atk;
    return stats;
  }

  // Light all the rooms
  public void lightAllRooms()
  {
    foreach (Room r in board.rooms)
    {
      r.lightTiles();
    }
  }

  // Instantiate chest object
  public GameObject instantiateChest()
  {
    GameObject chest = Instantiate(chestPrefab);
    return chest;
  }

  // Find the best path from startTile to endTile with Djkstra; 
  // avoid the collectible tile
  public List<Tile> findPath(Tile startTile, Tile endTile)
  {
    Room room = startTile.parent;

    Hashtable prev = new Hashtable();

    List<TileDist> allTileDist = new List<TileDist>();
    List<TileDist> allTiles = new List<TileDist>();
    foreach (Tile t in room.getTiles())
    {
      if (t == null)
        continue;
      TileDist tileDist = new TileDist(t);
      allTileDist.Add(tileDist);
      allTiles.Add(tileDist);
      prev.Add(t, null);
    }

    allTileDist.First(item => item.tile == startTile).dist = 0;
    while (allTileDist.Count > 0)
    {
      allTileDist = allTileDist.OrderBy(o => o.dist).ToList();
      TileDist u = allTileDist[0];
      if (u.tile == endTile)
        break;
      allTileDist.Remove(allTileDist[0]);
      if (float.IsPositiveInfinity(u.dist))
      {
        break;
      }

      foreach (Tile neigh in u.tile.getNeighbours())
      {
        if ((!neigh.canMove() && neigh != endTile) || (neigh.hasItem() && neigh != endTile))
          continue;
        if (allTiles.FirstOrDefault(item => item.tile == neigh) == null)
        {
          TileDist tileDist = new TileDist(neigh);
          allTiles.Add(tileDist);
          allTileDist.Add(tileDist);
        }
        TileDist v = allTiles.First(item => item.tile == neigh);
        float alt = u.dist + computeDistance(v.tile, u.tile);
        if (alt < v.dist)
        {
          v.dist = alt;
          prev[v.tile] = u.tile;
        }
      }
    }

    List<Tile> path = new List<Tile>();
    path.Add(endTile);
    Tile next = (Tile)prev[endTile];
    while (next != startTile)
    {
      path.Add(next);
      next = (Tile)prev[next];
    }

    return path;
  }

  // Compute distance value from 2 tile; the tiles in oblique position have
  // less distance then the other
  public float computeDistance(Tile tile1, Tile tile2)
  {
    Vector3 startPosition = tile1.getPosition();
    Vector3 endPosition = tile2.getPosition();

    float distance = Vector3.Distance(startPosition, endPosition);

    if (distance > 1)
    {
      return 1.0f;
    }

    return 0.75f;
  }

  // Utility class for Djkstra
  public class TileDist
  {
    public Tile tile;
    public float dist;

    public TileDist(Tile tile)
    {
      this.tile = tile;
      this.dist = float.PositiveInfinity;
    }
  }
}




