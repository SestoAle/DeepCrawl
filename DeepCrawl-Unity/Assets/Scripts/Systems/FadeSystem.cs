using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FadeSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentArray<MeshRenderer> Renderer;
    public ComponentDataArray<Fade> Fade;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    if (BoardManagerSystem.instance.isTraning)
      return;

    for (int i = 0; i < data.Length; i++)
    {
      // Change the mode of this entity to Fade
      StandardShaderUtils.ChangeRenderMode(data.Renderer[i].material, StandardShaderUtils.BlendMode.Fade);
      // Remove the Fade component
      PostUpdateCommands.RemoveComponent<Fade>(data.Entity[i]);
    }
  }
}
