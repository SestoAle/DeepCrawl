using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Magic : IComponentData
{
    public int type;
    public int damage;
    public int mp;
}
