using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainManager : MonoBehaviour
{
    public ListManager listManager;

    GameObject terrain;
    GameObject terrainGo;
    GameObject gridGo;
    GameObject grassGo;
    GameObject lightSun;
    Dictionary<int, GameObject> terrainChunkGoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> terrainLod0GoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> terrainLod1GoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> terrainLod2GoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> terrainLod3GoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> gridChunkGoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> gridGoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> gridSteepGoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> grassChunkGoCollection = new Dictionary<int, GameObject>();
    Dictionary<int, Dictionary<int, GameObject>> grassLod0GoCollection = new Dictionary<int, Dictionary<int, GameObject>>();
    Dictionary<int, Dictionary<int, GameObject>> grassLod1GoCollection = new Dictionary<int, Dictionary<int, GameObject>>();

    Vector3 _terrainSize;
    Vector3 _terrainPos;

    GridPoint[,] _grid;
    Chunk[,] _chunkGrid;

    int _terrainId;
    float angle = 30; //placeholder
    int chunkSize = 50;
    int lod1Size = 2;
    int lod2Size = 5;
    int lod3Size = 10;

    public int TerrainId {
        get => _terrainId;
        set => _terrainId = value;
    }

    public GridPoint[,] Grid {
        get => _grid;
        set => _grid = value;
    }

    public Chunk[,] ChunkGrid {
        get => _chunkGrid;
        set => _chunkGrid = value;
    }

    public Vector3 TerrainSize {
        get => _terrainSize;
        set => _terrainSize = value;
    }

    public Vector3 TerrainPos {
        get => _terrainPos;
        set => _terrainPos = value;
    }

    void Awake() {
        Debug.Log("starting TerrainManager");
        SetupTerrain();
        GridGrassFillTerrain();
    }

    private void Update() {
        //RotateSun();
    }

    void SetupTerrain() {
        Debug.Log("Setting up terrain");

        terrainGo = new GameObject("terrain");
        terrainGo.transform.SetParent(this.transform);
        terrainGo.transform.localPosition = new Vector3(0, 0, 0);

        grassGo = new GameObject("grass");
        grassGo.transform.SetParent(this.transform);
        grassGo.transform.localPosition = new Vector3(0, 0.001f, 0);

        gridGo = new GameObject("grid");
        gridGo.transform.SetParent(this.transform);
        gridGo.transform.localPosition = new Vector3(0, .2f, 0);

        terrain = new GameObject("terrain", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        terrain.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        terrain.transform.SetParent(this.transform);
        terrain.GetComponent<MeshFilter>().sharedMesh = Resources.Load<Mesh>("Terrain/" + TerrainId + "/"+ TerrainId + "-mesh");
        terrain.GetComponent<MeshCollider>().sharedMesh = Resources.Load<Mesh>("Terrain/" + TerrainId + "/" + TerrainId + "-mesh");
        terrain.GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-mat");
        terrain.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        RenderSettings.skybox = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-skybox");

        TerrainSize = terrain.GetComponent<MeshRenderer>().bounds.size;
        TerrainPos = terrain.transform.position;

        Grid = GridGen(new Vector2Int(Mathf.CeilToInt(TerrainSize.x), Mathf.CeilToInt(TerrainSize.z)));
        ChunkGrid = ChunkGridGen(new Vector2Int(Grid.GetLength(0), Grid.GetLength(1)));

        TerrainChunkGridMeshGen();
        terrain.SetActive(false);

        GridMeshGen(angle);

        lightSun = new GameObject("lightSun", typeof(Light));
        lightSun.transform.SetParent(this.transform);
        lightSun.transform.localPosition = new Vector3(0, 50, 0);
        lightSun.transform.rotation = Quaternion.Euler(30f, -213, 0);
        lightSun.GetComponent<Light>().type = LightType.Directional;
        //lightSun.GetComponent<Light>().color = new Color(0.82f, 0.55f, 0.46f, 1.00f);
        lightSun.GetComponent<Light>().color = new Color(0.8980f, 0.8549f, 0.6901f, 1.00f);
        lightSun.GetComponent<Light>().intensity = 1.75f;
        lightSun.GetComponent<Light>().shadows = LightShadows.None;
        lightSun.GetComponent<Light>().cullingMask = 1;
        RenderSettings.sun = lightSun.GetComponent<Light>();
    }
    void GridGrassFillTerrain() {
        //grass
        GridGrassAutoGen(Grid, 3, 0, 85, 00.0f, 01.8f);
        GridGrassAutoGen(Grid, 4, 0, 80, 01.8f, 02.4f);
        GridGrassAutoGen(Grid, 5, 0, 75, 02.4f, 03.6f);
        GridGrassAutoGen(Grid, 4, 0, 80, 03.6f, 05.8f);
        GridGrassAutoGen(Grid, 5, 0, 75, 05.8f, 07.4f);
        GridGrassAutoGen(Grid, 3, 0, 85, 07.4f, 09.2f);
        GridGrassAutoGen(Grid, 2, 0, 105, 09.2f, 11.6f);
        GridGrassAutoGen(Grid, 4, 0, 80, 11.6f, 13.8f);
        GridGrassAutoGen(Grid, 3, 0, 85, 13.8f, 15.8f);
        GridGrassAutoGen(Grid, 5, 0, 75, 15.8f, 17.2f);
        GridGrassAutoGen(Grid, 4, 0, 80, 17.2f, 19.6f);
        GridGrassAutoGen(Grid, 5, 0, 75, 19.6f, 20.2f);
        GridGrassAutoGen(Grid, 3, 0, 85, 20.2f, 22.2f);
        GridGrassAutoGen(Grid, 1, 0, 125, 22.2f, 24.8f);
        GridGrassAutoGen(Grid, 2, 0, 105, 24.8f, 27.4f);
        GridGrassAutoGen(Grid, 1, 0, 125, 27.4f, 30.2f);
        GridGrassAutoGen(Grid, 2, 0, 105, 30.2f, 34.6f);
        GridGrassAutoGen(Grid, 1, 0, 125, 34.6f, 38.8f);
        GridGrassAutoGen(Grid, 2, 0, 105, 38.8f, 39.2f);
        GridGrassAutoGen(Grid, 1, 0, 125, 39.2f, 44.4f);
        //snow
        GridGrassAutoGen(Grid, 17, 120, 130, 00.6f, 17.8f);
        GridGrassAutoGen(Grid, 17, 130, 135, 00.0f, 19.8f);
        GridGrassAutoGen(Grid, 18, 120, 130, 20.2f, 22.4f);
        GridGrassAutoGen(Grid, 18, 130, 135, 19.8f, 24.8f);
        GridGrassAutoGen(Grid, 16, 120, 130, 26.2f, 38.8f);
        GridGrassAutoGen(Grid, 16, 130, 135, 24.8f, 88.8f);
        GridGrassAutoGen(Grid, 19, 135, 200, 00.0f, 19.8f);
        GridGrassAutoGen(Grid, 20, 135, 200, 19.8f, 48.8f);
        GridGrassAutoGen(Grid, 18, 135, 200, 48.8f, 88.8f);
        GridGrassMeshGenAll();
    }

    void RotateSun() {
        lightSun.transform.Rotate(0.025f, 0, 0);
    }

    public Vector2Int WorldtoGrid(Vector3 pos) {
        int x = (int)((pos.x + (TerrainSize.x / 2)) - TerrainPos.x);
        int z = (int)((pos.z + (TerrainSize.z / 2)) - TerrainPos.z);
        return new Vector2Int(x, z); ;
    }

    public Vector3 GridToWorld(Vector2Int gp, bool enableHeight = true) {
        float y = 0; if (enableHeight) { y = Grid[gp.x, gp.y].position.y; }
        float x = (gp.x - (TerrainSize.x / 2)) + TerrainPos.x;
        float z = (gp.y - (TerrainSize.z / 2)) + TerrainPos.z;
        return new Vector3(x, y, z);
    }

    public Chunk GpToChunk(Vector2Int gp) {
        Chunk chunk = new Chunk();
        foreach (Chunk item in ChunkGrid) {
            if ((gp.x >= item.start.x && gp.x < item.end.x) && (gp.y >= item.start.y && gp.y < item.end.y)) {
                chunk = item;
            }
        }
        return chunk;
    }

    GridPoint[,] GridGen(Vector2Int size) {
        Debug.Log("Generating grid");
        GridPoint[,] gridGen = new GridPoint[size.x, size.y];
        for (int x = 0; x < size.x; x++) {
            for (int z = 0; z < size.y; z++) {
                gridGen[x, z] = new GridPoint();
                Vector3 pos = GridToWorld(new Vector2Int(x, z), false);
                Vector3 posCenter = pos + new Vector3(0.5f, 0, 0.5f);
                Vector3 rayStart = posCenter + new Vector3(0, 100000, 0);
                Ray ray = new Ray(rayStart, new Vector3(0, -1, 0));
                RaycastHit hit; if (Physics.Raycast(ray, out hit)) {
                    pos.y = hit.point.y;
                    gridGen[x, z].inBounds = true;
                    gridGen[x, z].angle = Vector3.Angle(hit.normal, rayStart);
                    gridGen[x, z].position = pos;
                    gridGen[x, z].normal = hit.normal;
                    gridGen[x, z].uv = hit.textureCoord;
                }
            }
        }
        Debug.Log(size.x + "x" + size.y + " grid generated");
        return gridGen;
    }

    Chunk[,] ChunkGridGen(Vector2Int size) {
        Debug.Log("Generating chunk grid");
        Vector2Int chunkNum = new Vector2Int();
        chunkNum.x = Mathf.CeilToInt((float)size.x / (float)chunkSize);
        chunkNum.y = Mathf.CeilToInt((float)size.y / (float)chunkSize);
        Chunk[,] chunkGridGen = new Chunk[chunkNum.x, chunkNum.y];
        int chunkCount = 0;
        for (int chunkX = 0; chunkX < chunkNum.x; chunkX++) {
            int xMin = chunkX * chunkSize;
            int xMax = (chunkX + 1) * chunkSize;
            for (int chunkZ = 0; chunkZ < chunkNum.y; chunkZ++) {
                int zMin = chunkZ * chunkSize;
                int zMax = (chunkZ + 1) * chunkSize;
                chunkGridGen[chunkX, chunkZ] = new Chunk();
                chunkGridGen[chunkX, chunkZ].id = chunkCount;
                chunkGridGen[chunkX, chunkZ].size = chunkSize;
                chunkGridGen[chunkX, chunkZ].position = Grid[xMin, zMin].position + new Vector3(chunkSize / 2, 0, chunkSize / 2);
                chunkGridGen[chunkX, chunkZ].start = new Vector2Int(xMin, zMin);
                chunkGridGen[chunkX, chunkZ].end = new Vector2Int(xMax, zMax);
                chunkCount++;
            }
        }
        Debug.Log(chunkNum.x + "x" + chunkNum.y + " chunk grid generated");
        return chunkGridGen;
    }

    void GridMeshGen(float angle) {
        Debug.Log("Generating grid mesh");
        foreach(Chunk chunk in ChunkGrid) {
            gridChunkGoCollection.Add(chunk.id, new GameObject("chunk: #" + chunk.id.ToString("D4"), typeof(LODGroup)));
            gridChunkGoCollection[chunk.id].transform.SetParent(gridGo.transform);
            gridChunkGoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
            gridGoCollection.Add(chunk.id, new GameObject("grid", typeof(MeshFilter), typeof(MeshRenderer)));
            gridGoCollection[chunk.id].transform.SetParent(gridChunkGoCollection[chunk.id].transform);
            gridGoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
            gridGoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlitTrans");
            gridGoCollection[chunk.id].GetComponent<Renderer>().material.color = new Color(.8f, .8f, .8f, .3f);
            gridGoCollection[chunk.id].GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainGridChunk(Grid, chunk, angle));
            gridSteepGoCollection.Add(chunk.id, new GameObject("grid steep", typeof(MeshFilter), typeof(MeshRenderer)));
            gridSteepGoCollection[chunk.id].transform.SetParent(gridChunkGoCollection[chunk.id].transform);
            gridSteepGoCollection[chunk.id].transform.localPosition = new Vector3(0, 0.05f, 0);
            gridSteepGoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlitTrans");
            gridSteepGoCollection[chunk.id].GetComponent<Renderer>().material.color = new Color(0.75f, 0.35f, 0.35f, 0.6f);
            gridSteepGoCollection[chunk.id].GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainGridChunk(Grid, chunk, angle, true, 1.5f));
            GridChunkUpdateLods(chunk);
        }
    }
    void GridChunkUpdateLods(Chunk chunk) {
        List<Renderer> r0List = new List<Renderer>();
        if (gridGoCollection.ContainsKey(chunk.id)) { r0List.Add(gridGoCollection[chunk.id].GetComponent<Renderer>()); }
        if (gridSteepGoCollection.ContainsKey(chunk.id)) { r0List.Add(gridSteepGoCollection[chunk.id].GetComponent<Renderer>()); }
        Renderer[] r0Array = r0List.ToArray();
        r0Array[0] = gridGoCollection[chunk.id].GetComponent<Renderer>();
        LOD[] lods = new LOD[1];
        lods[0] = new LOD(.5f, r0Array);
        lods[0].fadeTransitionWidth = 0.9f;
        gridChunkGoCollection[chunk.id].GetComponent<LODGroup>().SetLODs(lods);
        gridChunkGoCollection[chunk.id].GetComponent<LODGroup>().RecalculateBounds();
        gridChunkGoCollection[chunk.id].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }

    public void GridGrassAdd(Vector2Int gp, int grassId, bool doMeshGen = true) {
        Chunk chunk = GpToChunk(gp);
        Grass grass = listManager.Grass[grassId];
        GridGrassRemove(gp, doMeshGen);
        listManager.TryAddGrassPlacedList(chunk.id);
        listManager.GrassPlaced[chunk.id].Add(new GrassPlaced() { grass = grass, gp = gp });
        if (doMeshGen) { GridGrassMeshGen(chunk, grass); }
    }

    public void GridGrassRemove(Vector2Int gp, bool doMeshGen = true) {
        Chunk chunk = GpToChunk(gp);
        if (listManager.GrassPlaced.ContainsKey(chunk.id)) {
            for (int i = listManager.GrassPlaced[chunk.id].Count-1; i >= 0; i--) {
                GrassPlaced entry = listManager.GrassPlaced[chunk.id][i];
                if (entry.gp == gp) {
                    listManager.GrassPlaced[chunk.id].Remove(entry);
                    if (doMeshGen) { GridGrassMeshGen(chunk, entry.grass); }
                    listManager.TryRemoveGrassPlacedList(chunk.id);
                }
            }
        }
    }

    void GridGrassAutoGen(GridPoint[,] grid, int grassId = 1, float heightMin = 0, float heightMax = 999, float angleMin = 0, float angleMax = 10.0f) {
        Debug.Log("Generating grass #" + grassId);
        int count = 0;
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int z = 0; z < grid.GetLength(1); z++) {
                if (grid[x, z].inBounds && (grid[x, z].position.y >= heightMin && grid[x, z].position.y < heightMax) && (grid[x, z].angle >= angleMin && grid[x, z].angle < angleMax)) {
                    Vector2Int gp = new Vector2Int(x, z);
                    GridGrassAdd(gp, grassId, false);
                    count++;
                }
            }
        }
        Debug.Log("Added "+ count+" grass entries");
    }
    
    void GridGrassMeshGenAll() {
        foreach (Chunk chunk in ChunkGrid) {
            if (listManager.GrassPlaced.ContainsKey(chunk.id)) {
                foreach (GrassPlaced entry in listManager.GrassPlaced[chunk.id].GroupBy(r => r.grass.id).Select(r => new List<GrassPlaced>(r)[0]).ToList()) {
                    GridGrassMeshGen(chunk, entry.grass);
                }
            }
        }
    }

    void GridGrassMeshGen(Chunk chunk, Grass grass) {
        Debug.Log("Generating mesh for chunk #" + chunk.id.ToString("D4") + " - grass #" + grass.id.ToString("D4"));
        if (!listManager.GrassPlaced[chunk.id].Exists(r => r.grass == grass)) {
            TryRemoveGrassGo(chunk, grass);
        }  else {
            TryAddGrassGo(chunk, grass);
            grassLod1GoCollection[chunk.id][grass.id].GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainListGrass(Grid, listManager.GrassPlaced[chunk.id].FindAll(r => r.grass == grass)));
            grassLod0GoCollection[chunk.id][grass.id].GetComponent<MeshFilter>().mesh = GrassMeshGen.Create(grass, MeshGen.Create(MeshGen.MakeTerrainListGrass(Grid, listManager.GrassPlaced[chunk.id].FindAll(r => r.grass == grass))));
            GridGrassChunkUpdateLods(chunk);
            Debug.Log("Generated " + listManager.GrassPlaced[chunk.id].FindAll(r => r.grass == grass).Count + " grass meshes");
        }
    }

    void GridGrassChunkUpdateLods(Chunk chunk) {
        List<Renderer> r0List = new List<Renderer>(); foreach (GameObject entry in grassLod0GoCollection[chunk.id].Values) { r0List.Add(entry.GetComponent<Renderer>()); }
        List<Renderer> r1List = new List<Renderer>(); foreach (GameObject entry in grassLod1GoCollection[chunk.id].Values) { r1List.Add(entry.GetComponent<Renderer>()); }
        Renderer[] r0Array = r0List.ToArray();
        Renderer[] r1Array = r1List.ToArray();
        LOD[] lods = new LOD[2];
        lods[0] = new LOD(Mathf.Min(.6f, .02f + (listManager.GrassPlaced[chunk.id].Count * .0075f)), r0Array);
        lods[1] = new LOD(Mathf.Min(.08f, .01f + (listManager.GrassPlaced[chunk.id].Count * .0075f)), r1Array);
        lods[0].fadeTransitionWidth = 0.15f;
        lods[1].fadeTransitionWidth = 0.25f;
        grassChunkGoCollection[chunk.id].GetComponent<LODGroup>().SetLODs(lods);
        grassChunkGoCollection[chunk.id].GetComponent<LODGroup>().RecalculateBounds();
        grassChunkGoCollection[chunk.id].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }



    void TryAddGrassGo(Chunk chunk, Grass grass) {
        TryAddGrassChunkGo(chunk);
        if (!grassLod0GoCollection[chunk.id].ContainsKey(grass.id)) {
            grassLod0GoCollection[chunk.id].Add(grass.id, new GameObject("grass #" + grass.id.ToString("D4"), typeof(MeshFilter), typeof(MeshRenderer)));
            grassLod0GoCollection[chunk.id][grass.id].transform.SetParent(grassChunkGoCollection[chunk.id].transform);
            grassLod0GoCollection[chunk.id][grass.id].transform.localPosition = new Vector3(0, 0, 0);
            grassLod0GoCollection[chunk.id][grass.id].GetComponent<Renderer>().material = Resources.Load<Material>("Grass/" + grass.mat);
            grassLod1GoCollection[chunk.id].Add(grass.id, new GameObject("LOD", typeof(MeshFilter), typeof(MeshRenderer)));
            grassLod1GoCollection[chunk.id][grass.id].transform.SetParent(grassLod0GoCollection[chunk.id][grass.id].transform);
            grassLod1GoCollection[chunk.id][grass.id].transform.localPosition = new Vector3(0, 0, 0);
            grassLod1GoCollection[chunk.id][grass.id].GetComponent<Renderer>().material = Resources.Load<Material>("Grass/" + grass.mat);
            grassLod1GoCollection[chunk.id][grass.id].GetComponent<Renderer>().material.EnableKeyword("USE_LOD_MODE");
        }
    }

    void TryRemoveGrassGo(Chunk chunk, Grass grass) {
        if (!listManager.GrassPlaced[chunk.id].Exists(r => r.grass == grass)) {
            Destroy(grassLod0GoCollection[chunk.id][grass.id]);
            grassLod0GoCollection[chunk.id].Remove(grass.id);
            Destroy(grassLod1GoCollection[chunk.id][grass.id]);
            grassLod1GoCollection[chunk.id].Remove(grass.id);
        }
        TryRemoveGrassChunkGo(chunk);
    }

    void TryAddGrassChunkGo(Chunk chunk) {
        if (!grassLod0GoCollection.ContainsKey(chunk.id)) {
            grassLod0GoCollection.Add(chunk.id, new Dictionary<int, GameObject>());
            grassLod1GoCollection.Add(chunk.id, new Dictionary<int, GameObject>());
            grassChunkGoCollection.Add(chunk.id, new GameObject("chunk: #" + chunk.id.ToString("D4"), typeof(LODGroup)));
            grassChunkGoCollection[chunk.id].transform.SetParent(grassGo.transform);
            grassChunkGoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    void TryRemoveGrassChunkGo(Chunk chunk) {
        if (grassLod0GoCollection.ContainsKey(chunk.id)) {
            if (grassLod0GoCollection[chunk.id].Count == 0) {
                Destroy(grassChunkGoCollection[chunk.id]);
                grassChunkGoCollection.Remove(chunk.id);
                grassLod0GoCollection.Remove(chunk.id);
                grassLod1GoCollection.Remove(chunk.id);
            }
        }
    }

    void TerrainChunkGridMeshGen() {
        foreach (Chunk chunk in ChunkGrid) {
            TerrainChunkMeshGen(chunk);
        }
    }

    void TerrainChunkMeshGen(Chunk chunk) {
        Mesh meshLod0 = MeshGen.Create(MeshGen.MakeTerrainChunk(Grid, chunk));
        Mesh meshLod1 = MeshGen.Create(MeshGen.MakeTerrainChunk(Grid, chunk, lod1Size));
        Mesh meshLod2 = MeshGen.Create(MeshGen.MakeTerrainChunk(Grid, chunk, lod2Size));
        Mesh meshLod3 = MeshGen.Create(MeshGen.MakeTerrainChunk(Grid, chunk, lod3Size));

        terrainChunkGoCollection.Add(chunk.id, new GameObject("chunk: #" + chunk.id.ToString("D4"), typeof(LODGroup)));
        terrainChunkGoCollection[chunk.id].transform.SetParent(terrainGo.transform);
        terrainChunkGoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);

        terrainLod0GoCollection.Add(chunk.id, new GameObject("lod0", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
        terrainLod0GoCollection[chunk.id].transform.SetParent(terrainChunkGoCollection[chunk.id].transform);
        terrainLod0GoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod0GoCollection[chunk.id].GetComponent<MeshFilter>().sharedMesh = meshLod0;
        terrainLod0GoCollection[chunk.id].GetComponent<MeshCollider>().sharedMesh = meshLod0;
        terrainLod0GoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-mat");

        terrainLod1GoCollection.Add(chunk.id, new GameObject("lod1", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
        terrainLod1GoCollection[chunk.id].transform.SetParent(terrainChunkGoCollection[chunk.id].transform);
        terrainLod1GoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod1GoCollection[chunk.id].GetComponent<MeshFilter>().sharedMesh = meshLod1;
        terrainLod1GoCollection[chunk.id].GetComponent<MeshCollider>().sharedMesh = meshLod1;
        terrainLod1GoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-mat");

        terrainLod2GoCollection.Add(chunk.id, new GameObject("lod2", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
        terrainLod2GoCollection[chunk.id].transform.SetParent(terrainChunkGoCollection[chunk.id].transform);
        terrainLod2GoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod2GoCollection[chunk.id].GetComponent<MeshFilter>().sharedMesh = meshLod2;
        terrainLod2GoCollection[chunk.id].GetComponent<MeshCollider>().sharedMesh = meshLod2;
        terrainLod2GoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-mat");

        terrainLod3GoCollection.Add(chunk.id, new GameObject("lod3", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
        terrainLod3GoCollection[chunk.id].transform.SetParent(terrainChunkGoCollection[chunk.id].transform);
        terrainLod3GoCollection[chunk.id].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod3GoCollection[chunk.id].GetComponent<MeshFilter>().sharedMesh = meshLod3;
        terrainLod3GoCollection[chunk.id].GetComponent<MeshCollider>().sharedMesh = meshLod3;
        terrainLod3GoCollection[chunk.id].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId + "/" + TerrainId + "-mat");

        TerrainChunkUpdateLods(chunk);
    }

    void TerrainChunkUpdateLods(Chunk chunk) {
        Renderer[] r0Array = new Renderer[1];
        Renderer[] r1Array = new Renderer[1];
        Renderer[] r2Array = new Renderer[1];
        Renderer[] r3Array = new Renderer[1];
        r0Array[0] = terrainLod0GoCollection[chunk.id].GetComponent<Renderer>();
        r1Array[0] = terrainLod1GoCollection[chunk.id].GetComponent<Renderer>();
        r2Array[0] = terrainLod2GoCollection[chunk.id].GetComponent<Renderer>();
        r3Array[0] = terrainLod3GoCollection[chunk.id].GetComponent<Renderer>();
        LOD[] lods = new LOD[4];
        lods[0] = new LOD(.35f, r0Array);
        lods[1] = new LOD(.20f, r1Array);
        lods[2] = new LOD(.08f, r2Array);
        lods[3] = new LOD(.005f, r3Array);
        lods[0].fadeTransitionWidth = 0.10f;
        lods[1].fadeTransitionWidth = 0.10f;
        lods[2].fadeTransitionWidth = 0.10f;
        lods[3].fadeTransitionWidth = 0.10f;
        terrainChunkGoCollection[chunk.id].GetComponent<LODGroup>().SetLODs(lods);
        terrainChunkGoCollection[chunk.id].GetComponent<LODGroup>().RecalculateBounds();
        terrainChunkGoCollection[chunk.id].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }
}
