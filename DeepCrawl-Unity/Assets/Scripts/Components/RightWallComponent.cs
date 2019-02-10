using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

[Serializable]
public struct RightWall : IComponentData
{

}

public class RightWallComponent : ComponentDataWrapper<RightWall> { }