﻿using Unity.Entities;
using Unity.Mathematics;

public struct AABB : IComponentData
{
    public float2 min;
    public float2 max;
};
