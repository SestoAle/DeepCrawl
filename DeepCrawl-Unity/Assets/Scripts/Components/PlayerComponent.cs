using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct Player : IComponentData
{
}

public class PlayerComponent : ComponentDataWrapper<Player> { }
