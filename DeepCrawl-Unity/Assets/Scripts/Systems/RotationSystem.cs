using UnityEngine;
using Unity.Entities;

public class RotationSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public ComponentArray<Transform> Transforms;
    public ComponentDataArray<Rotation> Rotations;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    var puc = PostUpdateCommands;
    for (int i = 0; i < data.Length; i++)
    {
      // Change rotation of character
      Quaternion newRotation = Quaternion.Euler(new Vector3(data.Rotations[i].rotationX, data.Rotations[i].rotationY, data.Rotations[i].rotationZ));
      data.Transforms[i].rotation = newRotation;
      puc.RemoveComponent<Rotation>(data.Entity[i]);
    }
  }

}
