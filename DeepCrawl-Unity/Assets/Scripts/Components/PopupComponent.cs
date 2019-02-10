using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PopupComponent : ISharedComponentData
{
  public GameObject popupText;
  public float randomOffset;
}