using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct MagicEffect : ISharedComponentData
{
    public GameObject effect;
    public int x;
    public int y;
}
