using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class heightmapTest : MonoBehaviour
{
    private void Start() {
        //heightmpapTest2();
    }

    /*
    void heightmpapTest2() {
    int TerrainId = 13;
    Chunk chunk = new Chunk();
    chunk.end = new Vector2Int(400,400);
    Texture2D hMap = Resources.Load<Texture2D>("Terrain/" + TerrainId + "/" + TerrainId + "-heightmap");
    Mesh mesh = MeshGen.Create(MeshGen.MakeTerrain(hMap,chunk));
    //Mesh meshLod0 = MeshGen.Create(MeshGen.MakeTerrainChunk2(Grid, chunk));

    GameObject test = new GameObject("mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
    //terrainLod0GoCollection[chunk.id].transform.SetParent(terrainChunkGoCollection[chunk.id].transform);
    //terrainLod0GoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
    test.GetComponent<MeshFilter>().mesh = mesh;
    //terrainLod0GoCollection[chunk.id].GetComponent<MeshCollider>().sharedMesh = meshLod0;
    test.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
    //terrainLod0GoCollection[chunk.id].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
}
void heightmpapTest() {
    int TerrainId = 13;
    //Texture2D hMap = Resources.Load("Terrain/13/13-heightmap.png") as Texture2D;
    Texture2D hMap = Resources.Load<Texture2D>("Terrain/" + TerrainId + "/" + TerrainId + "-heightmap");

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();

    //Bottom left section of the map, other sections are similar
    for (int i = 0; i < 350; i++) {
        for (int j = 0; j < 350; j++) {
            //Add each new vertex in the plane
            verts.Add(new Vector3(i, hMap.GetPixel(i, j).grayscale * 1000, j));
            //Skip if a new square on the plane hasn't been formed
            if (i == 0 || j == 0) continue;
            //Adds the index of the three vertices in order to make up each of the two tris
            tris.Add(250 * i + j); //Top right
            tris.Add(250 * i + j - 1); //Bottom right
            tris.Add(250 * (i - 1) + j - 1); //Bottom left - First triangle
            tris.Add(250 * (i - 1) + j - 1); //Bottom left 
            tris.Add(250 * (i - 1) + j); //Top left
            tris.Add(250 * i + j); //Top right - Second triangle
        }
    }

    Vector2[] uvs = new Vector2[verts.Count];
    for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
        uvs[i] = new Vector2(verts[i].x, verts[i].z);

    GameObject plane = new GameObject("ProcPlane"); //Create GO and add necessary components
    plane.AddComponent<MeshFilter>();
    plane.AddComponent<MeshRenderer>();
    Mesh procMesh = new Mesh();
    procMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    procMesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
    procMesh.uv = uvs;
    procMesh.triangles = tris.ToArray();
    procMesh.RecalculateNormals(); //Determines which way the triangles are facing
    plane.GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter
}
    */
}
