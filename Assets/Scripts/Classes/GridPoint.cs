using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint {
    public int x { set; get; }
    public int z { set; get; }
    public bool inBounds { set; get; } = false;
    public float angleCenter { set; get; } = 0.0f;
    public Vector3 posCenter { set; get; } = Vector3.zero;
    public Vector2 uvCenter { set; get; } = Vector2.zero;
    public Vector3 normalCenter { set; get; } = Vector3.zero;
    public Vector3[] pos { set; get; } = new Vector3[4];
    public Vector2[] uv { set; get; } = new Vector2[4];
    public Vector3[] normal { set; get; } = new Vector3[4];
}