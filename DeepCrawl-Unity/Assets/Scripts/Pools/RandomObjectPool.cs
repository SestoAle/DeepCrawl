using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectPool : ObjectPool 
{
  public GameObject[] prefabs;

  private void Awake()
  {
    for (int i = 0; i < count; i++)
    {
      // Create a new random object from the array of prefabs
      GameObject poolObject = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
      poolObject.transform.parent = transform;
      poolObject.SetActive(false);
      pool.Add(poolObject);
    }
  }

  public new GameObject getPooledObject()
  {
    for (int i = 0; i < pool.Count; i++)
    {
      if (!pool[i].activeInHierarchy)
      {
        pool[i].SetActive(true);
        return pool[i];
      }
    }

    // Create a new random object from the array of prefabs
    GameObject newPoolObject = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
    newPoolObject.transform.parent = gameObject.transform;
    pool.Add(newPoolObject);
    return newPoolObject;
  }
}
