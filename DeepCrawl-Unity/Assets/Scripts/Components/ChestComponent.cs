using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;

[Serializable]
public struct Chest : IComponentData
{

}

public class ChestComponent : ComponentDataWrapper<Chest> { }