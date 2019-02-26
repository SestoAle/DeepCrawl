using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;

public class PinchSystem : ComponentSystem
{
  public struct Data
  {
    public readonly int Length;
    public EntityArray Entity;
    public GameObjectArray GameObjects;
  }

  [Inject] private Data data;

  public float orthoZoomSpeed = 0.01f;
  public float orthoZoomSpeedWheel = 2f;

  protected override void OnUpdate()
  {
    if(BoardManagerSystem.instance.isTraning)
    {
      return;
    }

    // Get the camera
    Camera camera = Camera.main;

    // If touch count is 2
    if (Input.touchCount == 2)
    {
      // Get the first and second touch
      Touch touchZero = Input.GetTouch(0);
      Touch touchOne = Input.GetTouch(1);

      Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
      Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

      float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
      float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

      float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

      // Change the otrhographic size depending on the pinch
      camera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

      // Clamp the size between min and max values
      camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, 3.0f, 8.0f);
    }

    // Utility for Mouse Scroll Wheel
    var orthographicSize = camera.orthographicSize;
    orthographicSize += Input.GetAxis("Mouse ScrollWheel") * orthoZoomSpeedWheel;
    orthographicSize = Mathf.Clamp(orthographicSize, 3.0f, 8.0f);
    camera.orthographicSize = orthographicSize;
  }
}