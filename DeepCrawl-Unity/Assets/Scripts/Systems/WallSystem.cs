using Unity.Jobs;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;

public class WallBarrier : BarrierSystem { }

public class WallSystem : JobComponentSystem
{
  public struct WallGroup
  {
    public readonly int Length;
    public EntityArray Entity;
    public ComponentDataArray<WallPosition> Position;
    public ComponentArray<MeshRenderer> Renderers;
    public ComponentDataArray<Fadeble> Fadeble;

    public ComponentDataArray<Wall> Wall;

    public SubtractiveComponent<Door> Door;
  }

  public struct PlayerGroup
  {
    public readonly int Length;
    [ReadOnly] public ComponentDataArray<Position> Position;
    [ReadOnly] public ComponentDataArray<Player> Player;
  }

  [Inject] private WallGroup m_wallGroup;
  [Inject] private PlayerGroup m_playerGroup;
  [Inject] private WallBarrier barrier;

  private struct Job : IJobParallelFor
  {
    public WallGroup wallGroup;
    public PlayerGroup playerGroup;
    [ReadOnly] public EntityCommandBuffer commandBuffer;

    public void Execute(int i)
    {
      // Get the position of the player and the wall
      Position playerPosition = playerGroup.Position[0];
      WallPosition wallPosition = wallGroup.Position[i];

      // Get the distance between them
      Vector2 distanceVector = new Vector2(playerPosition.x, playerPosition.y) - new Vector2(wallPosition.x, wallPosition.y);
      float distance = Vector3.Magnitude(distanceVector);
      distanceVector = distanceVector / distance;

      // If the player is near the wall
      if (distance < 2f &&
          ((wallGroup.Wall[i].type == 1 && distanceVector.y > 0) ||
           (wallGroup.Wall[i].type == 2 && distanceVector.x > 0))
         )
      {
        // Add a Fade component
        Entity entity = wallGroup.Entity[i];
        Fadeble fadeble = wallGroup.Fadeble[i];
        if (fadeble.isFade == 0)
        {
          fadeble.isFade = 1;
          wallGroup.Fadeble[i] = fadeble;
          commandBuffer.AddComponent(entity, new Fade { });
        }
      }
      else
      {
        // If is faded, add a RemoveFade component
        if (wallGroup.Fadeble[i].isFade != 0)
        {
          Entity entity = wallGroup.Entity[i];
          Fadeble fadeble = wallGroup.Fadeble[i];
          fadeble.isFade = 0;
          wallGroup.Fadeble[i] = fadeble;
          commandBuffer.AddComponent(entity, new RemoveFade { });
        }
      }
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps)
  {
    if (BoardManagerSystem.instance.isTraning)
    {
      this.Enabled = false;
      barrier.Enabled = false;
    }

    Job job = new Job() {
      wallGroup = m_wallGroup,
      playerGroup = m_playerGroup,
      commandBuffer = barrier.CreateCommandBuffer()
    };

    return job.Schedule(m_wallGroup.Length, 64, inputDeps);
  }
}