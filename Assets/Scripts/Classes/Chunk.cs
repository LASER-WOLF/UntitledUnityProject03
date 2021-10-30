using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public int x { set; get; }
    public int z { set; get; }
    public int size { set; get; }
    public GridPoint start { set; get; }
    public GridPoint end { set; get; }
}
