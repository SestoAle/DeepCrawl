using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Linq;
using System.IO;

using Panda;

public class RandomAgentBlackBoard : BaseAgentBlackBoard
{
    [Task]
    public void RandomMove()
    {
        // Make a Random Move
        entityManager.AddComponentData(agent.Entity, new UserInput { action = Random.Range(0, 17) });

        // Complete whether is the random moves
        Task.current.Succeed();
    }
}
