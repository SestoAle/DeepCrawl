using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RemoveFadeSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObject;
    public ComponentArray<MeshRenderer> Renderer;
    public ComponentDataArray<RemoveFade> RemoveFade;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    if (BoardManagerSystem.instance.isTraning)
      return;

    for (int i = 0; i < data.Length; i++)
    {
      // Change the render mode of the object to opaque
      StandardShaderUtils.ChangeRenderMode(data.Renderer[i].material, StandardShaderUtils.BlendMode.Opaque);
      // Remove Fade component
      PostUpdateCommands.RemoveComponent<RemoveFade>(data.Entity[i]);
    }
  }
}
