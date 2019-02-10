using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CameraSystem : ComponentSystem
{

  public struct Data
  {
    public readonly int Length;
    public ComponentDataArray<Turn> Turns;
    public ComponentArray<Transform> Transforms;
  }

  [Inject] private Data data;

  protected override void OnUpdate()
  {
    if (BoardManagerSystem.instance.noAnim)
      return;

    var dt = Time.deltaTime;

    // Get camera information
    var camera = Camera.main;
    Vector3 cameraOffset = GameManager.instance.cameraOffset;
    Vector3 cameraRotation = GameManager.instance.cameraRotation;
    var cameraSmoothing = GameManager.instance.cameraSmoothing;

    if(!GameManager.instance.cameraManual)
    {
      for (int i = 0; i < data.Length; i++)
      {
        if (data.Turns[i].hasTurn == 1)
        {
          // Change camera position following the character that has the turn
          Vector3 characterPos = data.Transforms[i].position;
          if(Vector3.Distance(characterPos, camera.transform.position) < float.Epsilon)
          {
            continue;
          }
          Vector3 newCameraPos = new Vector3();
          newCameraPos.x = characterPos.x - cameraOffset.x;
          newCameraPos.z = characterPos.z - cameraOffset.z;
          newCameraPos.y = cameraOffset.y;
          camera.transform.position = Vector3.Lerp(camera.transform.position, newCameraPos, cameraSmoothing * dt);
          camera.transform.rotation = Quaternion.Euler(cameraRotation);
        }
      }
    }
  }
}
