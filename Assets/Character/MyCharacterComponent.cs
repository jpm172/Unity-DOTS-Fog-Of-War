using System;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
public struct MyCharacterComponent : IComponentData
{
    public float MovementSpeed;
}

[Serializable]
public struct PlayerInputs : IComponentData
{
    public float2 MoveInput;
    public float3 AimPosition;
}

