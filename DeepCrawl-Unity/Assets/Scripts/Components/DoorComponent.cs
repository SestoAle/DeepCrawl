using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Door : IComponentData
{

}

public class DoorComponent : ComponentDataWrapper<Door> { }
