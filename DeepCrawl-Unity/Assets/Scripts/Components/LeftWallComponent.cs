using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

[Serializable]
public struct LeftWall : IComponentData
{

}

public class LeftWallComponent : ComponentDataWrapper<LeftWall> { }