using Unity.Jobs;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;


public class ItemBarrier : BarrierSystem { }

public class ItemSystem : JobComponentSystem
{
  public struct ItemGroup
  {
    public readonly int Length;
    public EntityArray Entity;
    public ComponentArray<MeshRenderer> Renderers;
    public ComponentDataArray<Fadeble> Fadeble;
    public ComponentDataArray<WallPosition> Position;

    public SubtractiveComponent<Wall> Wall;
    public SubtractiveComponent<Door> Door;
  }

  public struct PlayerGroup
  {
    public readonly int Length;
    public ComponentDataArray<Position> Position;
    [ReadOnly] public ComponentDataArray<Player> Player;
  }

  [Inject] private ItemGroup m_itemGroup;
  [Inject] private PlayerGroup m_playerGroup;
  [Inject] private ItemBarrier barrier;

  private struct Job : IJobParallelFor
  {
    public ItemGroup itemGroup;
    public PlayerGroup playerGroup;
    [ReadOnly] public EntityCommandBuffer commandBuffer;

    public void Execute(int i)
    {
      if (BoardManagerSystem.instance.isTraning)
        return;

      // Get the positions of the player and of the wall
      Position playerPosition = playerGroup.Position[0];
      WallPosition wallPosition = itemGroup.Position[i];

      // Get the distance between player and wall
      Vector2 distanceVector = new Vector2(playerPosition.x, playerPosition.y) - new Vector2(wallPosition.x, wallPosition.y);
      float distance = Vector3.Magnitude(distanceVector);
      distanceVector = distanceVector / distance;

      // If the player is near to the wall
      if (distance < 2.0f && distanceVector.x > 0)
      {
        // Add a Fade component
        Entity entity = itemGroup.Entity[i];
        Fadeble fadeble = itemGroup.Fadeble[i];
        if (fadeble.isFade == 0)
        {
          fadeble.isFade = 1;
          itemGroup.Fadeble[i] = fadeble;
          commandBuffer.AddComponent(entity, new Fade { });
        }
      }
      else
      {
        // If the wall is faded
        if (itemGroup.Fadeble[i].isFade != 0)
        {
          // Change it to opaque mode
          Entity entity = itemGroup.Entity[i];
          Fadeble fadeble = itemGroup.Fadeble[i];
          fadeble.isFade = 0;
          itemGroup.Fadeble[i] = fadeble;
          commandBuffer.AddComponent(entity, new RemoveFade { });
        }
      }
    }
  }

  // Start the parallel job
  protected override JobHandle OnUpdate(JobHandle inputDeps)
  {
    if (BoardManagerSystem.instance.isTraning)
    {
      this.Enabled = false;
      barrier.Enabled = false;
    }

    Job job = new Job() {
      itemGroup = m_itemGroup,
      playerGroup = m_playerGroup,
      commandBuffer = barrier.CreateCommandBuffer()
    };

    return job.Schedule(m_itemGroup.Length, 64, inputDeps);
  }


}