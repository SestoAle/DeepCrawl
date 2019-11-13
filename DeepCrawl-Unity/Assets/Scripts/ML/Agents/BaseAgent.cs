using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Unity.Entities;
using System.Linq;

public class BaseAgent : Agent
{

    Character agent;
    EntityManager entityManager;
    Character target;
    public Stats agentStats;
    public Stats targetStats;
    public Inventory agentInventory;

    Room room;

    public bool deterministic = true;

    // For agent vs agent
    public bool lastMove;

    [HideInInspector]
    public Stats prevAgentStats;
    [HideInInspector]
    public Stats prevTargetStats;

    private void Start()
    {
        AgentRestart();
    }

    public override void AgentReset()
    {

        AgentRestart();
    }

    public void AgentRestart()
    {
        // Get EntityManager
        entityManager = World.Active.GetExistingManager<EntityManager>();
        // Get the agent character component
        agent = GetComponent<Character>();
        // Get the agent stats
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        // Get agent inventory
        agentInventory = GetComponent<Inventory>();
        // Get the target player
        target = agent.target.GetComponent<Character>();
        // Get target stats
        targetStats = entityManager.GetComponentData<Stats>(target.Entity);

        prevAgentStats = agentStats;
        prevTargetStats = targetStats;

        // Initialize lstm internal state
        if (brain.brainType == BrainType.Internal)
        {
            lastAction = 90;
            lstmMemory = new float[1, 2, 256];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    lstmMemory[0, i, j] = 0.0f;
                }
            }
        }

        lastMove = false;
    }

    List<float> prevState = new List<float>();
    List<float> currState = new List<float>();

    public override void CollectObservations()
    {
        // Initialize the observation array
        List<float> obs = new List<float>();
        // Get the curren position of the agent
        Vector3 pos = gameObject.transform.position;
        // Get the current tile of the agent
        Tile startTile = BoardManagerSystem.instance.getTile((int)pos.x, (int)pos.z);
        // Get the room of the agent
        room = startTile.getParent();

        // Update the agent and target stats
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        targetStats = entityManager.GetComponentData<Stats>(target.Entity);

        // Get the global observation
        for (int x = 0; x < room.getSize(); x++)
            for (int y = 0; y < room.getSize(); y++)
            {
                {
                    Tile t = room.getTile(x, y);
                    if (t == null)
                    {
                        obs.Add(0);
                        continue;
                    }
                    if (t.canMove())
                    {
                        if (t.hasItem())
                        {
                            obs.Add((3.0f + (float)t.getItem().id));
                        }
                        else
                        {
                            obs.Add(1.0f);
                        }
                    }
                    else
                    {
                        if (t.hasCharacter())
                        {
                            if (ReferenceEquals(t.getCharacter(), gameObject))
                            {
                                obs.Add(2.0f);
                            }
                            else if (ReferenceEquals(t.getCharacter(), target.gameObject))
                            {
                                obs.Add(3.0f);
                            }
                            else
                            {
                                obs.Add(0.0f);
                            }
                        }
                        else
                        {
                            obs.Add(0.0f);
                        }
                    }
                }
            }

        // Get the local 5x5 observation
        int doubleSize = BoardManagerSystem.instance.doubleSize;
        int size = (int)doubleSize / 2;

        Vector2 roomPos = new Vector2(pos.x - room.getCoords()[0], pos.z - room.getCoords()[1]);

        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                if (i == 0 && j == 0)
                {
                    obs.Add(2.0f);
                    continue;
                }
                Tile t = room.getTile((int)roomPos.x + i, (int)roomPos.y + j);

                if (t == null)
                {
                    obs.Add(0.0f);
                }
                else if (t.canMove())
                {
                    if (t.hasItem())
                    {
                        obs.Add((3.0f + (float)t.getItem().id));
                    }
                    else
                    {
                        obs.Add(1.0f);
                    }
                }
                else
                {
                    if (t.hasCharacter() && ReferenceEquals(t.getCharacter(), target.gameObject))
                    {
                        obs.Add(3.0f);
                    }
                    else
                    {
                        obs.Add(0.0f);
                    }
                }
            }
        }

        // Get the local 3x3 observation
        int tripleSize = BoardManagerSystem.instance.tripleSize;
        size = (int)tripleSize / 2;

        roomPos = new Vector2(pos.x - room.getCoords()[0], pos.z - room.getCoords()[1]);

        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                if (i == 0 && j == 0)
                {
                    obs.Add(2.0f);
                    continue;
                }
                Tile t = room.getTile((int)roomPos.x + i, (int)roomPos.y + j);

                if (t == null)
                {
                    obs.Add(0.0f);
                }
                else if (t.canMove())
                {
                    if (t.hasItem())
                    {
                        obs.Add((3.0f + (float)t.getItem().id));
                    }
                    else
                    {
                        obs.Add(1.0f);
                    }
                }
                else
                {
                    if (t.hasCharacter() && ReferenceEquals(t.getCharacter(), target.gameObject))
                    {
                        obs.Add(3.0f);
                    }
                    else
                    {
                        obs.Add(0.0f);
                    }
                }
            }
        }

        // Add agent HP observation
        obs.Add(sampleHp(agentStats.hp, agentStats.maxHp));

        // Add agent potion observation
        if (agentInventory.potion != null)
        {
            obs.Add(20 + agentInventory.potion.id + 1);
        }
        else
        {
            obs.Add(20 + 1);
        }

        // Add agent weapons observation
        obs.Add(21 + agentInventory.meeleWeapon.id);
        obs.Add(21 + agentInventory.rangeWeapon.id);

        // Add agent active buff observation
        if (entityManager.HasComponent<Buff>(agent.Entity))
        {
            obs.Add(31);
        }
        else
        {
            obs.Add(30);
        }

        // Add agent possible shoot direction observation
        obs.Add(41 +
                    getShootDirection(BoardManagerSystem.instance.getTileFromObject(gameObject)));

        // Get current target inventory
        Inventory targetInventory = target.gameObject.GetComponent<Inventory>();
        // Add target HP observation
        obs.Add(50 + sampleHp(targetStats.hp, targetStats.maxHp));

        // Add target inventory observations
        if (targetInventory.potion != null)
        {
            obs.Add(50 + 20 + targetInventory.potion.id + 1);
        }
        else
        {
            obs.Add(50 + 20 + 1);
        }
        obs.Add(50 + 21 + targetInventory.meeleWeapon.id);
        obs.Add(50 + 21 + targetInventory.rangeWeapon.id);

        // Add target active buffer observation
        if (entityManager.HasComponent<Buff>(target.Entity))
        {
            obs.Add(81);
        }
        else
        {
            obs.Add(80);
        }

        currState = obs;

        prevAgentStats = agentStats;
        prevTargetStats = targetStats;

        AddVectorObs(obs);
    }

    public int sameCount = 0;

    public override void AgentAction(float[] vectorAction, string textAction)
    {

        // Prevent the internal agent from gettin stuck
        if (brain.brainType == BrainType.Internal && BoardManagerSystem.instance.doubleAgent)
        {
            if (prevState.SequenceEqual(currState))
            {
                sameCount++;
                if (sameCount >= 50)
                {
                    Debug.Log("Random");
                    vectorAction[0] = Random.Range(0, 18);
                    sameCount = 0;
                }
            }
            else
            {
                sameCount = 0;
            }

            prevState = currState;
        }

        // Give a small negative reward for each move to speedup the agent
        AddReward(-0.01f);

        // Get agent current stats
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        int currentHp = agentStats.hp;

        if (currentHp <= 0)
        {
            return;
        }

        // If it's done, don't do anything
        if (entityManager.HasComponent<DoneComponent>(agent.Entity))
        {
            return;
        }

        // Get weapons
        RangeWeapon rangeWeapon = agent.GetComponent<Inventory>().rangeWeapon;
        Potion potion = agent.GetComponent<Inventory>().potion;

        // Get the position and orientation and the tile of this character and highlight his neighbours
        Vector3 oldPos = gameObject.transform.position;
        Tile tile = BoardManagerSystem.instance.getTileFromObject(agent.gameObject);

        Vector3 newPos = oldPos;

        Tile rangeTile = null;
        int totalRange = rangeWeapon.range + agentStats.des - 2;
        IComponentData potionComponent = null;

        switch ((int)vectorAction[0])
        {
            // Movement actions (0-7)
            case 0:
                newPos = oldPos + new Vector3(0, 0, 1);
                break;
            case 1:
                newPos = oldPos + new Vector3(1, 0, 0);
                break;
            case 2:
                newPos = oldPos + new Vector3(0, 0, -1);
                break;
            case 3:
                newPos = oldPos + new Vector3(-1, 0, 0);
                break;
            case 4:
                newPos = oldPos + new Vector3(1, 0, 1);
                break;
            case 5:
                newPos = oldPos + new Vector3(1, 0, -1);
                break;
            case 6:
                newPos = oldPos + new Vector3(-1, 0, -1);
                break;
            case 7:
                newPos = oldPos + new Vector3(-1, 0, 1);
                break;
            // Buff Action (8)
            case 8:
                if (entityManager.HasComponent(agent.Entity, typeof(Buff)) || potion == null)
                {
                    // If the agent can't use a potion but he make the action, add a negative reward
                    makeImpossibleMove();
                }
                else
                {
                    // If the agent can use a potion, it doesnt't loose the turn so
                    // remove the negative reward
                    potionComponent = potion.getComponent();
                }
                break;
            // Range actions (9,16)
            case 9:
                rangeTile = getRangeTile((int)DIRECTION.North, totalRange, tile);
                break;
            case 10:
                rangeTile = getRangeTile((int)DIRECTION.East, totalRange, tile);
                break;
            case 11:
                rangeTile = getRangeTile((int)DIRECTION.South, totalRange, tile);
                break;
            case 12:
                rangeTile = getRangeTile((int)DIRECTION.West, totalRange, tile);
                break;
            case 13:
                rangeTile = getRangeTile((int)DIRECTION.NorthEast, totalRange, tile);
                break;
            case 14:
                rangeTile = getRangeTile((int)DIRECTION.SouthEast, totalRange, tile);
                break;
            case 15:
                rangeTile = getRangeTile((int)DIRECTION.SouthWest, totalRange, tile);
                break;
            case 16:
                rangeTile = getRangeTile((int)DIRECTION.NorthWest, totalRange, tile);
                break;
        }

        if (newPos != oldPos)
        {
            // Get the new tile and check if exists or if can move on it
            Tile newTile = BoardManagerSystem.instance.getTile((int)newPos.x, (int)newPos.z);

            if (newTile == null || (!newTile.canMove() && !newTile.hasCharacter()) || !tile.isNeighbour(newTile))
            {
                // If the agent can't move in that direction, give a negative reward
                makeImpossibleMove();
            }
        }

        if (potionComponent != null)
        {
            // If the agent cure himself, add a positive reward
            if(!BoardManagerSystem.instance.isGuided)
                AddReward(0.01f);
        }

        if (!IsDone() || brain.brainType == BrainType.Internal)
        {
            if (BoardManagerSystem.instance.doubleAgent && brain.brainType == BrainType.Internal)
            {
                if (Random.Range(0f, 1f) <= BoardManagerSystem.instance.academy.resetParameters["randomDoubleAgent"])
                {
                    //entityManager.AddComponentData(agent.Entity, new UserInput { action = Random.Range(0, 17) });
                }
                else
                {
                    entityManager.AddComponentData(agent.Entity, new UserInput { action = (int)vectorAction[0] });
                }
            }
            else
            {
                entityManager.AddComponentData(agent.Entity, new UserInput { action = (int)vectorAction[0] });
            }
        }
        else
        {
            entityManager.AddComponentData(agent.Entity, new DoneComponent { });
        }

        lastAction = (int)vectorAction[0];
        deterministic = true;
    }

    void makeImpossibleMove()
    {
        SetReward(-0.1f);
    }

    void makeNotyetMove()
    {
        SetReward(-0.1f);
    }

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

    // Give a negative reward for each Hp lost. This has to be called before 
    // RequestDecision()
    public void giveHpReward()
    {
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        int currentHp = agentStats.hp;

        if (currentHp <= 0)
        {
            //Done();
        }
    }

    // Give a positive reward for each Hp that the target has lost. This has to be called before 
    // RequestDecision()
    public void giveDamageReward()
    {
        targetStats = entityManager.GetComponentData<Stats>(target.Entity);
        int currentTargetHp = targetStats.hp;

        if (currentTargetHp <= 0)
        {
            if(!BoardManagerSystem.instance.isGuided)
                SetReward(10.0f * computeHpFactor());
            else
                SetReward(5.0f);

            // IRL Settings
            BoardManagerSystem.instance.randomSpawn(target.gameObject);
            Stats stats = new Stats { hp = 1, atk = 3, def = 3, des = 3, maxHp = 20 };
            entityManager.SetComponentData(target.Entity, stats);
            //Done();
        }
    }

    // Compute the HP factor
    public float computeHpFactor()
    {
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        int currentHp = agentStats.hp;
        int maxHp = agentStats.maxHp;

        float hp_discounted_factor = (float)currentHp / (float)maxHp;

        return hp_discounted_factor;
    }

    // Return the possible direction of shooting for the agent
    public int getShootDirection(Tile startTile)
    {
        foreach (DIRECTION dir in System.Enum.GetValues(typeof(DIRECTION)))
        {
            Vector2 offset = GameManager.instance.directionToTile((int)dir);
            Tile newTile = startTile;
            Tile prevTile = startTile;
            int range = agentStats.actualRange;
            for (int i = 0; i < range; i++)
            {
                newTile = BoardManagerSystem.instance.getTile((int)(newTile.x + offset.x), (int)(newTile.y + offset.y));
                if (newTile == null)
                    break;

                if (!prevTile.isNeighbour(newTile))
                    break;

                if (newTile.hasCharacter() && !newTile.isAround(gameObject))
                {
                    GameObject c = (GameObject)newTile.getCharacter();
                    if (c == target.gameObject)
                    {
                        return (int)dir / 45 + 1;
                    }
                }

                if (!newTile.canMove() || newTile.hasCharacter())
                    break;

                prevTile = newTile;
            }
        }
        return 0;
    }

    public int sampleHp(int hp, int maxHp)
    {
        return (int)20 * hp / maxHp;
    }
}
