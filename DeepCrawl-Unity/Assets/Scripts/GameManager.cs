using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DIRECTION
{
  North = 0,
  NorthEast = 45,
  East = 90,
  SouthEast = 135,
  South = 180,
  SouthWest = 225,
  West = 270,
  NorthWest = 315
}

public class GameManager : MonoBehaviour
{

  // Camera informations
  public Vector3 cameraOffset = new Vector3(0, 3, 2);
  public Vector3 cameraRotation = new Vector3(30, 0, 0);
  public float cameraSmoothing = 5f;
  public float cameraDrag = 0.2f;
  public bool cameraManual = false;

  // Character informations
  public float characterSpeed = 5f;

  // UI Information
  public GameUI gameUI;

  public static GameManager instance;

  void Awake()
  {
    //Check if instance already exists
    if (instance == null)

      //if not, set instance to this
      instance = this;

    //If instance already exists and it's not this:
    else if (instance != this)

      //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
      Destroy(gameObject);

    //Sets this to not be destroyed when reloading scene
    DontDestroyOnLoad(gameObject);
  }

  public void DestroyGameObject(GameObject gameObject)
  {
    Destroy(gameObject);
  }

  public void DestroyPooledGameObject(GameObject gameObject)
  {
    gameObject.SetActive(false);
  }

  // TODO: return Vector3 so to use it also in Input?
  // Convert int to Vector2 offset
  public Vector2 directionToTile(int direction)
  {
    Vector2 tilePos = new Vector2();
    switch (direction)
    {
      case (int)DIRECTION.North:
        tilePos.x = 0;
        tilePos.y = 1;
        break;
      case (int)DIRECTION.East:
        tilePos.x = 1;
        tilePos.y = 0;
        break;
      case (int)DIRECTION.South:
        tilePos.x = 0;
        tilePos.y = -1;
        break;
      case (int)DIRECTION.West:
        tilePos.x = -1;
        tilePos.y = 0;
        break;
      case (int)DIRECTION.NorthEast:
        tilePos.x = 1;
        tilePos.y = 1;
        break;
      case (int)DIRECTION.SouthEast:
        tilePos.x = 1;
        tilePos.y = -1;
        break;
      case (int)DIRECTION.SouthWest:
        tilePos.x = -1;
        tilePos.y = -1;
        break;
      case (int)DIRECTION.NorthWest:
        tilePos.x = -1;
        tilePos.y = 1;
        break;
    }
    return tilePos;
  }

  Vector3 prevMousePos;

  // Move camera manually
  public void moveManualCamera(Vector3 currentMousePos)
  {
    Camera camera = Camera.main;
    cameraManual = true;
    // Define the movement vector
    Vector3 offset = Vector3.zero;

    // Check the previous and current mouse positions to compute the movement vector
    if (prevMousePos == Vector3.zero)
    {
      prevMousePos = currentMousePos;
    }
    else
    {
      offset = prevMousePos - Input.mousePosition;
      prevMousePos = Input.mousePosition;
    }

    // Compute the camera movement on the xz-plane depending on the movement
    // vector above
    Vector3 Forward = camera.transform.forward * offset.y;
    Vector3 Right = camera.transform.right * offset.x;
    Forward.y = 0;
    Vector3 newCameraPos = camera.transform.position + Forward + Right;
    var dt = Time.deltaTime;
    // Lerp the new camera position
    camera.transform.position = Vector3.Lerp(camera.transform.position, newCameraPos, cameraDrag * dt);
  }

  public void resetMousePos()
  {
    prevMousePos = new Vector3();
  }
}
