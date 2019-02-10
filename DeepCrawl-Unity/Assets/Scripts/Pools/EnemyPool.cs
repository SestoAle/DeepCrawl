using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyPool : ObjectPool 
{

  private void Awake()
  {
    for (int i = 0; i < count; i ++)
    {
      GameObject poolObject = Instantiate(prefab);
      poolObject.transform.parent = transform;
      poolObject.name = "Enemy " + (i + 1);
      pool.Add(poolObject);
      poolObject.SetActive(false);
    }
  }

  public new GameObject getPooledObject()
  {
    var em = World.Active.GetExistingManager<EntityManager>();
    Entity entity;
    for (int i = 0; i < pool.Count; i++)
    {
      if (!pool[i].activeInHierarchy)
      {
        pool[i].SetActive(true);
        entity = pool[i].GetComponent<Character>().Entity;
        em.AddComponentData(entity, new Turn { index = i + 1, hasEndedTurn = 0, hasTurn = 0 });
        return pool[i];
      }
    }

    GameObject newPoolObject = Instantiate(prefab);
    newPoolObject.transform.parent = gameObject.transform;
    em = World.Active.GetExistingManager<EntityManager>();
    entity = newPoolObject.GetComponent<Character>().Entity;
    Turn turn = new Turn { index = pool.Count + 1, hasEndedTurn = 0, hasTurn = 0 };
    em.AddComponentData(entity, turn);
    pool.Add(newPoolObject);
    return newPoolObject;
  }
}
