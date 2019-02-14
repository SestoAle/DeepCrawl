using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ObjectPool : MonoBehaviour 
{
  public GameObject prefab;

  public List<GameObject> pool = new List<GameObject>();

  public Quaternion defaultRotation;

  public int count = 64;

  private void Awake()
  {
    if(prefab != null)
    {
      // When awake, instantiate the objects and set them to False
      for (int i = 0; i < count; i++)
      {
        GameObject poolObject = Instantiate(prefab);
        defaultRotation = poolObject.transform.rotation;
        poolObject.transform.parent = transform;
        poolObject.SetActive(false);
        pool.Add(poolObject);
      }
    }
  }

  public bool initialize()
  {
    if (prefab == null || pool.Count > 0)
      return false;
    Awake();
    return true;
  }

  public GameObject getPooledObject()
  {
    // When requested, return the first disabled object and make it active
    for (int i = 0; i < pool.Count; i++)
    {
      if (!pool[i].activeInHierarchy)
      {
        pool[i].SetActive(true);
        pool[i].transform.rotation = defaultRotation;
        if (!BoardManagerSystem.instance.isTraning)
        {
          StandardShaderUtils.ChangeRenderMode(pool[i].GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
          foreach (Renderer renderer in pool[i].GetComponentsInChildren<Renderer>())
          {
            // Reset its shader to opaque mode
            StandardShaderUtils.ChangeRenderMode(renderer.material, StandardShaderUtils.BlendMode.Opaque);
          }
        }
        return pool[i];
      }
    }

    // If all the objects are active, instantiate a new one
    GameObject newPoolObject = Instantiate(prefab);
    newPoolObject.transform.parent = gameObject.transform;
    pool.Add(newPoolObject);
    return newPoolObject;
  }

  public void destroyAllObjects()
  {
    // Disabled all the objects in the pool
    for (int i = 0; i < pool.Count; i++)
    {
      pool[i].SetActive(false);
    }
  }
}
