using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint
{
    public bool inBounds { set; get; } = false;
    public float angle { set; get; } = 0.0f;
    public Vector3 position { set; get; } = Vector3.zero;
    public Vector3 normal { set; get; } = Vector3.zero;
    public Vector2 uv { set; get; } = Vector2.zero;
}