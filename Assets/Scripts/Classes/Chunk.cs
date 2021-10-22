using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public int id { set; get; }
    public int size { set; get; }
    public Vector3 position { set; get; }
    public Vector2Int start { set; get; }
    public Vector2Int end { set; get; }
}
