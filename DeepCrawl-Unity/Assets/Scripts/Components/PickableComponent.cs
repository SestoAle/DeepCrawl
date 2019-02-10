using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Pickable : IComponentData
{
}

public class PickableComponent : ComponentDataWrapper<Pickable> { }
