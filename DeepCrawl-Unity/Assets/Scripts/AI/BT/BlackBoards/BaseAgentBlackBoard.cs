using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using System.IO;

using Panda;

public class BaseAgentBlackBoard : MonoBehaviour
{
    protected EntityManager entityManager;
    protected Character agent;
    protected Entity agentEntity;
    protected Stats agentStats;
    protected Character target;
    protected Entity targetEntity;

    protected Tile nextMovement;

    protected Room room;

    // Start is called before the first frame update
    void Start()
    {
        // Get EntityManager
        entityManager = World.Active.GetExistingManager<EntityManager>();
        // Get agent
        agent = gameObject.GetComponent<Character>();
        // Get entity agent
        agentEntity = agent.Entity;
        // Get agent statistics
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);
        // Get the target player
        target = agent.target.GetComponent<Character>();
        // Get entity target
        targetEntity = target.Entity;
        // Get the start position of the agent
        Vector3 pos = gameObject.transform.position;
        // Get the start tile of the agent
        Tile startTile = BoardManagerSystem.instance.getTile((int)pos.x, (int)pos.z);
        // Get the room of the agent
        room = startTile.getParent();
    }

    [Task]
    public void GotPotion()
    {
        // Get the potion of the agent
        Potion potion = agent.GetComponent<Inventory>().potion;
        // If the potion object is not null, return Succed
        if (potion != null)
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    public void ActiveBuff()
    {
        // If the agent has an Active Buff, return Succed
        if (entityManager.HasComponent(agent.Entity, typeof(Buff)))
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    public void Move()
    {
        // Get the current position and tile of the agent
        Position currentPosition = entityManager.GetComponentData<Position>(agentEntity);
        Tile currentTile = BoardManagerSystem.instance.getTile(currentPosition.x, currentPosition.y);

        // Get the optimal path from the current position of the agent to the
        // current position of the target
        List<Tile> path = BoardManagerSystem.instance.findPath(currentTile, nextMovement, true, null);

        // Get the first tile of the path
        Tile nextTile = path[path.Count - 1];
        // Compute the offset from the current tile of the agent to the
        // next tile of the path
        Vector3 offset = nextTile.getPosition() - currentTile.getPosition();

        // Convert the offset to a movement action
        UserInput userInput = BoardManagerSystem.instance.offsetToMovementUserInput(offset);

        // Add the action to entity
        entityManager.AddComponentData(agent.Entity, userInput);

        Task.current.Succeed();
    }

    [Task]
    public void FindTarget()
    {
        // Get the current position and tile of the target
        Position targetPosition = entityManager.GetComponentData<Position>(targetEntity);
        Tile targetTile = BoardManagerSystem.instance.getTile(targetPosition.x, targetPosition.y);

        if (targetTile != null)
        {
            nextMovement = targetTile;
            Task.current.Succeed();
            return;
        }

        Task.current.Fail();
    }

    [Task]
    public void GotHealthPotion()
    {
        // Get the potion of the agent
        Potion potion = agent.GetComponent<Inventory>().potion;
        // If the potion object is not null and it is an health potion, return Succed
        if (potion != null && potion.GetType() == typeof(HealthPotion))
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    public void GotBuffPotion()
    {
        // Get the potion of the agent
        Potion potion = agent.GetComponent<Inventory>().potion;
        // If the potion object is not null and it is an health potion, return Succed
        if (potion != null && potion.GetType() == typeof(BuffPotion))
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    public void HpLessThan(int hpThreshold)
    {
        // Update the current agent statistics
        agentStats = entityManager.GetComponentData<Stats>(agent.Entity);

        // If the agent has low hp than hpThreshold, return Succed
        if (agentStats.hp < agentStats.maxHp - hpThreshold)
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    [Task]
    public void UsePotion()
    {
        UserInput userInput = new UserInput { action = 8 };

        // Add the action to entity
        entityManager.AddComponentData(agent.Entity, userInput);

        Task.current.Succeed();
    }

    [Task]
    public void FindPotion(string potionType)
    {
        float dist = 9999f;
        // Get the current agent position
        Position agentPosition = entityManager.GetComponentData<Position>(agentEntity);
        Item bestPotion = null;

        // Get the desired potion type as a parameter
        System.Type type = null;

        if (potionType == "Health")
        {
            type = typeof(HealthPotion);
        }
        else if (potionType == "Buff")
        {
            type = typeof(BuffPotion);
        }

        // For each potion still pickable
        foreach (Item i in room.getItems())
        {
            if (i.GetType() == type &&
                entityManager.HasComponent<Pickable>(i.Entity))
            {
                // Get the position of the potion
                Position healthPosition = entityManager.GetComponentData<Position>(i.Entity);
                // Get the distance between the potion and the agent
                float newDist = Vector2.Distance(new Vector2(agentPosition.x, agentPosition.y),
                    new Vector2(healthPosition.x, healthPosition.y));

                // Find the nearest potion
                if (newDist < dist)
                {
                    dist = newDist;
                    bestPotion = i;
                }
            }
        }

        // If exist a near potion, return Succed
        if (bestPotion != null)
        {
            nextMovement = BoardManagerSystem.instance.getTileFromObject(bestPotion.gameObject);
            Task.current.Succeed();
            return;
        }

        Task.current.Fail();
    }

    [Task]
    public void EnemyIsNear()
    {
        // Get the current position and tile of the agent
        Position currentPosition = entityManager.GetComponentData<Position>(agentEntity);
        Tile currentTile = BoardManagerSystem.instance.getTile(currentPosition.x, currentPosition.y);

        List<Tile> neigh = currentTile.getNeighbours();
        foreach (Tile t in neigh)
        {
            if (t.hasCharacter())
            {
                Task.current.Succeed();
                return;
            }
        }
        Task.current.Fail();
        return;
    }

    [Task]
    public void FindSecureSpot()
    {
        // Get the current position and tile of the agent
        Position currentPosition = entityManager.GetComponentData<Position>(agentEntity);
        Tile currentTile = BoardManagerSystem.instance.getTile(currentPosition.x, currentPosition.y);

        // Get the current position and tile of the target
        Position targetPosition = entityManager.GetComponentData<Position>(targetEntity);
        Tile targetTile = BoardManagerSystem.instance.getTile(targetPosition.x, targetPosition.y);
        Stats targetStats = entityManager.GetComponentData<Stats>(targetEntity);

        List<Tile> avoidTiles = BoardManagerSystem.instance.getRangeTiles(targetStats.actualRange, targetTile, target.gameObject);
        List<Tile> targetNeigh = targetTile.getNeighbours();

        List<Tile> removingTiles = new List<Tile>();
        foreach(Tile avoid in avoidTiles)
        {
            foreach(Tile neigh in targetNeigh)
            {
                if(avoid == neigh)
                {
                    removingTiles.Add(avoid);
                    break;
                }
            }
        }

        foreach(Tile t in removingTiles)
        {
            avoidTiles.Remove(t);
        }

        List<Tile> path = BoardManagerSystem.instance.findPath(currentTile, targetTile, true, avoidTiles);

        if(path == null)
        {
            Task.current.Fail();
        }

        nextMovement = path[path.Count - 1];

        Task.current.Succeed();

        return;
    }
}
