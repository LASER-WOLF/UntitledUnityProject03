using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGen
{

    public static Mesh Create(MeshGenItem meshGen, bool modTwoSided = false, bool modFlipYZ = false, Vector3 size = new Vector3(), Vector3 offset = new Vector3()) {
        if (modTwoSided) { meshGen = ModTwoSided(meshGen); }
        if (modFlipYZ) { meshGen = ModFlipYZ(meshGen); }
        if (size != Vector3.zero) { meshGen = ModSize(meshGen, size); }
        if (offset != Vector3.zero) { meshGen = ModOffset(meshGen, offset); }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = "MeshGen";
        mesh.vertices = meshGen.verts;
        mesh.triangles = meshGen.tris;
        mesh.uv = meshGen.uv;
        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }


    public static MeshGenItem MakeTerrain(Texture2D hMap, Chunk chunk, int size = 1) {

        int width = hMap.width;
        int height = hMap.height;
        float hMapMultiplier = 1000;

        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        //int count = 0; 
        int numVerts = 0;
        for (int x = chunk.start.x; x < chunk.end.x; x += size) {
            for (int z = chunk.start.y; z < chunk.end.y; z += size) {
                if (x + size < width && z + size < height) {

                    Vector2Int[] gpPos = new Vector2Int[4];
                    gpPos[0] = new Vector2Int(x, z + size);
                    gpPos[1] = new Vector2Int(x + size, z + size);
                    gpPos[2] = new Vector2Int(x, z);
                    gpPos[3] = new Vector2Int(x + size, z);

                    float[] gpHeight = new float[4];
                    gpHeight[0] = hMap.GetPixel(x, z + size).grayscale * hMapMultiplier;
                    gpHeight[1] = hMap.GetPixel(x + size, z + size).grayscale * hMapMultiplier;
                    gpHeight[2] = hMap.GetPixel(x, z).grayscale * hMapMultiplier;
                    gpHeight[3] = hMap.GetPixel(x + size, z).grayscale * hMapMultiplier;

                    Vector2[] gpUv = new Vector2[4];
                    gpUv[0] = new Vector2Int(0,0);
                    gpUv[1] = new Vector2Int(1,0);
                    gpUv[2] = new Vector2Int(0,1);
                    gpUv[3] = new Vector2Int(1,1);

                    meshGroup.Add(MakeTerrainGp(gpPos, gpHeight, gpUv, numVerts));
                    //numVerts += meshGroup[count].verts.Length;
                    numVerts += 4;
                    //count++;
                }
            }
        }
        return Combine(meshGroup);

    }

    public static MeshGenItem MakeTerrainGp(Vector2Int[] gpPos, float[] gpHeight, Vector2[] gpUv, int numVerts = 0, float offset = 0.0f) {
        MeshGenItem plane = Plane(new Vector3[4] { new Vector3(gpPos[0].x, gpHeight[0] + offset, gpPos[0].y), new Vector3(gpPos[1].x, gpHeight[1] + offset, gpPos[1].y), new Vector3(gpPos[2].x, gpHeight[2] + offset, gpPos[2].y), new Vector3(gpPos[3].x, gpHeight[3] + offset, gpPos[3].y) }, numVerts);
        plane.uv = gpUv;
        return plane;
    }


    public static MeshGenItem MakeTerrainDictUnit(GridPoint[,] grid, Dictionary<Vector2Int, int> dict, Quaternion rotation = new Quaternion(), float offset = 0.03f, float size = 1.0f) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        foreach (Vector2Int gp in dict.Keys) {
                    Vector3 pos = grid[gp.x, gp.y].position;
                    meshGroup.Add(ModRotate(MakeDiamond(pos, size, size, size, 0, offset, 0, numVerts),rotation,pos+new Vector3(0.5f,0,0.5f)));
                    numVerts += meshGroup[count].verts.Length;
                    count++;
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainListGrass(GridPoint[,] grid, List<GrassPlaced> list, float offset = 0.001f) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        foreach (GrassPlaced grassPlaced in list) {
            Vector3 pos = grid[grassPlaced.gp.x, grassPlaced.gp.y].position;
            meshGroup.Add(MakeTerrainGpSquare(pos, offset, 1, numVerts));
            numVerts += meshGroup[count].verts.Length;
            count++;
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainChunk(GridPoint[,] grid, Chunk chunk, int size = 1) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        for (int x = chunk.start.x; x < chunk.end.x; x+=size) {
            for (int z = chunk.start.y; z < chunk.end.y; z+=size) {
                if (x < grid.GetLength(0) && z < grid.GetLength(1)) {
                    if (grid[x, z].inBounds) {
                        meshGroup.Add(MakeTerrainGpSquare(grid[x, z].position, 0, size, numVerts));
                        numVerts += meshGroup[count].verts.Length;
                        count++;
                    }
                }
            }
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainDictText(GridPoint[,] grid, Dictionary<Vector2Int, int> dict) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        foreach (Vector2Int gp in dict.Keys) {
            Vector3 pos = grid[gp.x, gp.y].position;
            Quaternion rotation = Camera.main.transform.rotation;
            MeshGenItem mesh = MakeGroupAutoAlphaNum(dict[gp].ToString("D4"), Vector3.zero, 0.25f, 0.25f, 0.25f, 1, 0, 0, 0, numVerts);
            mesh = ModFlipYZ(mesh);
            mesh = ModRotate(mesh, rotation, new Vector3(0.5f, 0, 0));
            mesh = ModOffset(mesh, pos + new Vector3(0, 2f, 0.5f));
            meshGroup.Add(mesh);
            numVerts += meshGroup[count].verts.Length;
            count++;
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainGrid(GridPoint[,] grid, float angle = 0.0f, bool steepMode = false, float size = 1.0f, float offset = 0.01f) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int z = 0; z < grid.GetLength(1); z++) {
                if (grid[x, z].inBounds && ((!steepMode && grid[x, z].angle <= angle) || (steepMode && grid[x, z].angle > angle))) {
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid[x, z].normal);
                    meshGroup.Add(MakeTerrainGpDotRotated(rotation, grid[x, z].position, size, size, size, offset, numVerts));
                    numVerts += meshGroup[count].verts.Length;
                    count++;
                }
            }
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainGridChunk(GridPoint[,] grid, Chunk chunk, float angle = 0.0f, bool steepMode = false, float size = 1.0f, float offset = 0.01f) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0; int numVerts = 0;
        for (int x = chunk.start.x; x < chunk.end.x; x++) {
            for (int z = chunk.start.y; z < chunk.end.y; z++) {
                if (x < grid.GetLength(0) && z < grid.GetLength(1)) {
                    if (grid[x, z].inBounds && ((!steepMode && grid[x, z].angle <= angle) || (steepMode && grid[x, z].angle > angle))) {
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid[x, z].normal);
                        meshGroup.Add(MakeTerrainGpDotRotated(rotation, grid[x, z].position, size, size, size, offset, numVerts));
                        numVerts += meshGroup[count].verts.Length;
                        count++;
                    }
                }
            }
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeTerrainGpDotRotated(Quaternion rotation, Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offset = 0.01f, int numVerts = 0) {
        return ModOffset(ModRotate(MakeTerrainGpDot(pos, sizeX, sizeY, sizeZ, (1 - sizeX) * 0.5f, 0, (1 - sizeZ) * 0.5f, numVerts), rotation, pos + new Vector3(0.5f, 0, 0.5f)), new Vector3(0, offset, 0));
    }

    public static MeshGenItem MakeTerrainGpXRotated(Quaternion rotation, Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offset = 0.01f, int numVerts = 0) {
        return ModOffset(ModRotate(MakeTerrainGpX(pos, sizeX, sizeY, sizeZ, (1 - sizeX) * 0.5f, 0, (1 - sizeZ) * 0.5f, numVerts), rotation, pos + new Vector3(0.5f, 0, 0.5f)), new Vector3(0, offset, 0));
    }

    public static MeshGenItem MakeTerrainGpX(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.560f), new Vector3(0.560f, 0.000f, 0.560f), new Vector3(0.440f, 0.000f, 0.440f), new Vector3(0.560f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.680f), new Vector3(0.440f, 0.000f, 0.680f), new Vector3(0.320f, 0.000f, 0.560f), new Vector3(0.440f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.440f), new Vector3(0.440f, 0.000f, 0.440f), new Vector3(0.320f, 0.000f, 0.320f), new Vector3(0.440f, 0.000f, 0.320f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.800f), new Vector3(0.320f, 0.000f, 0.800f), new Vector3(0.200f, 0.000f, 0.680f), new Vector3(0.320f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.320f), new Vector3(0.320f, 0.000f, 0.320f), new Vector3(0.200f, 0.000f, 0.200f), new Vector3(0.320f, 0.000f, 0.200f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.680f), new Vector3(0.680f, 0.000f, 0.680f), new Vector3(0.560f, 0.000f, 0.560f), new Vector3(0.680f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.440f), new Vector3(0.680f, 0.000f, 0.440f), new Vector3(0.560f, 0.000f, 0.320f), new Vector3(0.680f, 0.000f, 0.320f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.800f), new Vector3(0.800f, 0.000f, 0.800f), new Vector3(0.680f, 0.000f, 0.680f), new Vector3(0.800f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.320f), new Vector3(0.800f, 0.000f, 0.320f), new Vector3(0.680f, 0.000f, 0.200f), new Vector3(0.800f, 0.000f, 0.200f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeTerrainGpDot(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.400f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.600f), new Vector3(0.500f, 0.000f, 0.400f), new Vector3(0.600f, 0.000f, 0.500f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeTerrainGpSquare(Vector3 pos = new Vector3(), float offset = 0.1f, float size = 1, int numVerts = 0, bool deform = true) {
        float h1 = 0; float h2 = 0; float h3 = 0; float h4 = 0;
        Vector2[] uv = new Vector2[4] {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
        };
        if (deform) {
            Vector3 rayStart1 = pos + new Vector3(0.0f, 100000, size);
            Vector3 rayStart2 = pos + new Vector3(size, 100000, size);
            Vector3 rayStart3 = pos + new Vector3(0.0f, 100000, 0.0f);
            Vector3 rayStart4 = pos + new Vector3(size, 100000, 0.0f);
            Ray ray1 = new Ray(rayStart1, new Vector3(0, -1, 0));
            Ray ray2 = new Ray(rayStart2, new Vector3(0, -1, 0));
            Ray ray3 = new Ray(rayStart3, new Vector3(0, -1, 0));
            Ray ray4 = new Ray(rayStart4, new Vector3(0, -1, 0));
            RaycastHit hit1; if (Physics.Raycast(ray1, out hit1)) { h1 = hit1.point.y - pos.y; uv[0] = hit1.textureCoord; }
            RaycastHit hit2; if (Physics.Raycast(ray2, out hit2)) { h2 = hit2.point.y - pos.y; uv[1] = hit2.textureCoord; }
            RaycastHit hit3; if (Physics.Raycast(ray3, out hit3)) { h3 = hit3.point.y - pos.y; uv[2] = hit3.textureCoord; }
            RaycastHit hit4; if (Physics.Raycast(ray4, out hit4)) { h4 = hit4.point.y - pos.y; uv[3] = hit4.textureCoord; }
        }
        MeshGenItem plane = Plane(new Vector3[4] { new Vector3(0.0f, h1 + 0.0f + offset, size) + pos, new Vector3(size, h2 + 0.0f + offset, size) + pos, new Vector3(0.0f, h3 + 0.0f + offset, 0.0f) + pos, new Vector3(size, h4 + 0.0f + offset, 0.0f) + pos }, numVerts);
        plane.uv = uv;
        return plane;
    }

    public static MeshGenItem MakeGroupAutoAlphaNum(string content, Vector3 pos = new Vector3(), float sizeX = 0.25f, float sizeY = 0.25f, float sizeZ = 0.25f, float offsetX = 1.0f, float offsetY = 0.0f, float offsetZ = 0.0f, float marginLine = 1f, int numVerts = 0) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        string[] lines = content.Split('\n');
        int count = 0;
        foreach (string line in lines) {
            meshGroup.Add(MakeGroupAutoAlphaNumLine(line, pos-new Vector3(0,0,((sizeZ+(sizeZ*marginLine))*count)), sizeX, sizeY, sizeZ, offsetX, offsetY, offsetZ , numVerts));
            numVerts += meshGroup[count].verts.Length;
            count++;
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem MakeGroupAutoAlphaNumLine(string content, Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 1.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        content = content.ToLower();
        List<MeshGenGroup> meshGroups = new List<MeshGenGroup>();
        for (int i = 0; i < content.Length; i++) {
            if (content[i] == '0') { meshGroups.Add(new MeshGenGroup() { type = "num0" }); } 
            else if (content[i] == '1') { meshGroups.Add(new MeshGenGroup() { type = "num1" }); } 
            else if (content[i] == '2') { meshGroups.Add(new MeshGenGroup() { type = "num2" }); } 
            else if (content[i] == '3') { meshGroups.Add(new MeshGenGroup() { type = "num3" }); } 
            else if (content[i] == '4') { meshGroups.Add(new MeshGenGroup() { type = "num4" }); } 
            else if (content[i] == '5') { meshGroups.Add(new MeshGenGroup() { type = "num5" }); } 
            else if (content[i] == '6') { meshGroups.Add(new MeshGenGroup() { type = "num6" }); } 
            else if (content[i] == '7') { meshGroups.Add(new MeshGenGroup() { type = "num7" }); } 
            else if (content[i] == '8') { meshGroups.Add(new MeshGenGroup() { type = "num8" }); } 
            else if (content[i] == '9') { meshGroups.Add(new MeshGenGroup() { type = "num9" }); } 
            else if (content[i] == 'a') { meshGroups.Add(new MeshGenGroup() { type = "letterA" }); } 
            else if (content[i] == 'b') { meshGroups.Add(new MeshGenGroup() { type = "letterB" }); } 
            else if (content[i] == 'c') { meshGroups.Add(new MeshGenGroup() { type = "letterC" }); } 
            else if (content[i] == 'd') { meshGroups.Add(new MeshGenGroup() { type = "letterD" }); } 
            else if (content[i] == 'e') { meshGroups.Add(new MeshGenGroup() { type = "letterE" }); } 
            else if (content[i] == 'f') { meshGroups.Add(new MeshGenGroup() { type = "letterF" }); } 
            else if (content[i] == 'g') { meshGroups.Add(new MeshGenGroup() { type = "letterG" }); } 
            else if (content[i] == 'h') { meshGroups.Add(new MeshGenGroup() { type = "letterH" }); } 
            else if (content[i] == 'i') { meshGroups.Add(new MeshGenGroup() { type = "letterI" }); } 
            else if (content[i] == 'j') { meshGroups.Add(new MeshGenGroup() { type = "letterJ" }); } 
            else if (content[i] == 'k') { meshGroups.Add(new MeshGenGroup() { type = "letterK" }); } 
            else if (content[i] == 'l') { meshGroups.Add(new MeshGenGroup() { type = "letterL" }); } 
            else if (content[i] == 'm') { meshGroups.Add(new MeshGenGroup() { type = "letterM" }); } 
            else if (content[i] == 'n') { meshGroups.Add(new MeshGenGroup() { type = "letterN" }); } 
            else if (content[i] == 'o') { meshGroups.Add(new MeshGenGroup() { type = "letterO" }); } 
            else if (content[i] == 'p') { meshGroups.Add(new MeshGenGroup() { type = "letterP" }); } 
            else if (content[i] == 'q') { meshGroups.Add(new MeshGenGroup() { type = "letterQ" }); } 
            else if (content[i] == 'r') { meshGroups.Add(new MeshGenGroup() { type = "letterR" }); } 
            else if (content[i] == 's') { meshGroups.Add(new MeshGenGroup() { type = "letterS" }); } 
            else if (content[i] == 't') { meshGroups.Add(new MeshGenGroup() { type = "letterT" }); } 
            else if (content[i] == 'u') { meshGroups.Add(new MeshGenGroup() { type = "letterU" }); } 
            else if (content[i] == 'v') { meshGroups.Add(new MeshGenGroup() { type = "letterV" }); } 
            else if (content[i] == 'w') { meshGroups.Add(new MeshGenGroup() { type = "letterW" }); } 
            else if (content[i] == 'x') { meshGroups.Add(new MeshGenGroup() { type = "letterX" }); } 
            else if (content[i] == 'y') { meshGroups.Add(new MeshGenGroup() { type = "letterY" }); } 
            else if (content[i] == 'z') { meshGroups.Add(new MeshGenGroup() { type = "letterZ" }); } 
            else if (content[i] == '.') { meshGroups.Add(new MeshGenGroup() { type = "symDot" }); } 
            else if (content[i] == ',') { meshGroups.Add(new MeshGenGroup() { type = "symComma" }); } 
            else if (content[i] == ':') { meshGroups.Add(new MeshGenGroup() { type = "symColon" }); } 
            else if (content[i] == ';') { meshGroups.Add(new MeshGenGroup() { type = "symSemicolon" }); } 
            else if (content[i] == '\'') { meshGroups.Add(new MeshGenGroup() { type = "symApostrophe" }); } 
            else if (content[i] == '"') { meshGroups.Add(new MeshGenGroup() { type = "symQuotationMark" }); } 
            else if (content[i] == '-') { meshGroups.Add(new MeshGenGroup() { type = "symHyphen" }); } 
            else if (content[i] == '_') { meshGroups.Add(new MeshGenGroup() { type = "symUnderscore" }); } 
            else if (content[i] == '/') { meshGroups.Add(new MeshGenGroup() { type = "symSlash" }); } 
            else if (content[i] == '\\') { meshGroups.Add(new MeshGenGroup() { type = "symBackslash" }); } 
            else if (content[i] == '|') { meshGroups.Add(new MeshGenGroup() { type = "symVertBar" }); } 
            else if (content[i] == '(') { meshGroups.Add(new MeshGenGroup() { type = "symParenthesesLeft" }); } 
            else if (content[i] == ')') { meshGroups.Add(new MeshGenGroup() { type = "symParenthesesRight" }); } 
            else if (content[i] == '[') { meshGroups.Add(new MeshGenGroup() { type = "symBracketsLeft" }); } 
            else if (content[i] == ']') { meshGroups.Add(new MeshGenGroup() { type = "symBracketsRight" }); } 
            else if (content[i] == '<') { meshGroups.Add(new MeshGenGroup() { type = "symLessThan" }); } 
            else if (content[i] == '>') { meshGroups.Add(new MeshGenGroup() { type = "symGreaterThan" }); } 
            else if (content[i] == '=') { meshGroups.Add(new MeshGenGroup() { type = "symEquals" }); } 
            else if (content[i] == '+') { meshGroups.Add(new MeshGenGroup() { type = "symPlus" }); } 
            else if (content[i] == '&') { meshGroups.Add(new MeshGenGroup() { type = "symAmpersand" }); } 
            else if (content[i] == '%') { meshGroups.Add(new MeshGenGroup() { type = "symPercentSign" }); } 
            else if (content[i] == '!') { meshGroups.Add(new MeshGenGroup() { type = "symExclamationMark" }); } 
            else if (content[i] == '?') { meshGroups.Add(new MeshGenGroup() { type = "symQuestionMark" }); } 
            else if (content[i] == '#') { meshGroups.Add(new MeshGenGroup() { type = "symHashtag" }); } 
            else if (content[i] == '*') { meshGroups.Add(new MeshGenGroup() { type = "symAsterisk" }); } 
            else if (content[i] == '$') { meshGroups.Add(new MeshGenGroup() { type = "symDollarSign" }); } 
            else if (content[i] == ' ') { meshGroups.Add(new MeshGenGroup() { type = "empty" }); } 
            else { meshGroups.Add(new MeshGenGroup() { type = "square" }); }
        }
        foreach (MeshGenGroup m in meshGroups) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return GroupMerge(meshGroups, numVerts);
    }

    public static MeshGenItem MakeNum0(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" }); //top
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" }); //left
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f) }, type = "plane" }); //right
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.45f, 0.0f, 0.625f), new Vector3(0.55f, 0.0f, 0.625f), new Vector3(0.45f, 0.0f, 0.375f), new Vector3(0.55f, 0.0f, 0.375f) }, type = "plane" }); //center
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" }); //bottom
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum1(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.45f, 0.0f, 1.0f), new Vector3(0.55f, 0.0f, 1.0f), new Vector3(0.45f, 0.0f, 0.1f), new Vector3(0.55f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.25f, 0.0f, 0.1f), new Vector3(0.75f, 0.0f, 0.1f), new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.75f, 0.0f, 0.0f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.35f, 0.0f, 0.9f), new Vector3(0.45f, 0.0f, 0.9f), new Vector3(0.35f, 0.0f, 0.8f), new Vector3(0.45f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.25f, 0.0f, 0.8f), new Vector3(0.35f, 0.0f, 0.8f), new Vector3(0.25f, 0.0f, 0.7f), new Vector3(0.35f, 0.0f, 0.7f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum2(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.8f), new Vector3(0.3f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.8f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.6f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.6f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 0.0f, 0.5f), new Vector3(0.6f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.4f), new Vector3(0.6f, 0.0f, 0.4f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.4f, 0.0f, 0.4f), new Vector3(0.5f, 0.0f, 0.4f), new Vector3(0.4f, 0.0f, 0.3f), new Vector3(0.5f, 0.0f, 0.3f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.3f), new Vector3(0.4f, 0.0f, 0.3f), new Vector3(0.3f, 0.0f, 0.2f), new Vector3(0.4f, 0.0f, 0.2f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.2f), new Vector3(0.3f, 0.0f, 0.2f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f), new Vector3(0.2f, 0.0f, 0.0f), new Vector3(0.8f, 0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum3(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.8f), new Vector3(0.3f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.8f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.45f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.45f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.5f), new Vector3(0.8f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.2f), new Vector3(0.3f, 0.0f, 0.2f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum4(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.6f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.6f, 0.0f, 0.4f), new Vector3(0.7f, 0.0f, 0.4f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.1f, 0.0f, 0.4f), new Vector3(0.9f, 0.0f, 0.4f), new Vector3(0.1f, 0.0f, 0.3f), new Vector3(0.9f, 0.0f, 0.3f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.6f, 0.0f, 0.3f), new Vector3(0.7f, 0.0f, 0.3f), new Vector3(0.6f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 0.0f, 0.9f), new Vector3(0.6f, 0.0f, 0.9f), new Vector3(0.5f, 0.0f, 0.8f), new Vector3(0.6f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.4f, 0.0f, 0.8f), new Vector3(0.5f, 0.0f, 0.8f), new Vector3(0.4f, 0.0f, 0.7f), new Vector3(0.5f, 0.0f, 0.7f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.7f), new Vector3(0.4f, 0.0f, 0.7f), new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.4f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.2f, 0.0f, 0.5f), new Vector3(0.3f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.1f, 0.0f, 0.5f), new Vector3(0.2f, 0.0f, 0.5f), new Vector3(0.1f, 0.0f, 0.4f), new Vector3(0.2f, 0.0f, 0.4f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum5(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 1.0f), new Vector3(0.8f, 0.0f, 1.0f), new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.5f), new Vector3(0.3f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.5f), new Vector3(0.8f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.2f), new Vector3(0.3f, 0.0f, 0.2f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum6(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.4f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.4f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.4f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.8f), new Vector3(0.4f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.8f), new Vector3(0.3f, 0.0f, 0.8f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.5f), new Vector3(0.8f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum7(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 1.0f), new Vector3(0.8f, 0.0f, 1.0f), new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.8f), new Vector3(0.8f, 0.0f, 0.8f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.6f, 0.0f, 0.8f), new Vector3(0.7f, 0.0f, 0.8f), new Vector3(0.6f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 0.0f, 0.6f), new Vector3(0.6f, 0.0f, 0.6f), new Vector3(0.5f, 0.0f, 0.4f), new Vector3(0.6f, 0.0f, 0.4f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.4f, 0.0f, 0.4f), new Vector3(0.5f, 0.0f, 0.4f), new Vector3(0.4f, 0.0f, 0.2f), new Vector3(0.5f, 0.0f, 0.2f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.2f), new Vector3(0.4f, 0.0f, 0.2f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.4f, 0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum8(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.8f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.5f), new Vector3(0.3f, 0.0f, 0.5f), new Vector3(0.2f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.5f), new Vector3(0.8f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.8f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.7f, 0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeNum9(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 1.0f), new Vector3(0.7f, 0.0f, 1.0f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.9f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.2f, 0.0f, 0.9f), new Vector3(0.3f, 0.0f, 0.9f), new Vector3(0.2f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.6f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.7f, 0.0f, 0.9f), new Vector3(0.8f, 0.0f, 0.9f), new Vector3(0.7f, 0.0f, 0.2f), new Vector3(0.8f, 0.0f, 0.2f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.6f), new Vector3(0.7f, 0.0f, 0.6f), new Vector3(0.3f, 0.0f, 0.5f), new Vector3(0.7f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.6f, 0.0f, 0.2f), new Vector3(0.7f, 0.0f, 0.2f), new Vector3(0.6f, 0.0f, 0.1f), new Vector3(0.7f, 0.0f, 0.1f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.3f, 0.0f, 0.1f), new Vector3(0.6f, 0.0f, 0.1f), new Vector3(0.3f, 0.0f, 0.0f), new Vector3(0.6f, 0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }


    public static MeshGenItem MakeLetterA(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.375f, 0.000f, 1.000f), new Vector3(0.625f, 0.000f, 1.000f), new Vector3(0.375f, 0.000f, 0.750f), new Vector3(0.625f, 0.000f, 0.750f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.250f, 0.000f, 0.750f), new Vector3(0.375f, 0.000f, 0.750f), new Vector3(0.250f, 0.000f, 0.400f), new Vector3(0.375f, 0.000f, 0.400f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.625f, 0.000f, 0.750f), new Vector3(0.750f, 0.000f, 0.750f), new Vector3(0.625f, 0.000f, 0.400f), new Vector3(0.750f, 0.000f, 0.400f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.375f, 0.000f, 0.500f), new Vector3(0.625f, 0.000f, 0.500f), new Vector3(0.375f, 0.000f, 0.400f), new Vector3(0.625f, 0.000f, 0.400f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.125f, 0.000f, 0.400f), new Vector3(0.250f, 0.000f, 0.400f), new Vector3(0.125f, 0.000f, 0.000f), new Vector3(0.250f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.750f, 0.000f, 0.400f), new Vector3(0.875f, 0.000f, 0.400f), new Vector3(0.750f, 0.000f, 0.000f), new Vector3(0.875f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterB(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.150f, 0.000f, 1.000f), new Vector3(0.275f, 0.000f, 1.000f), new Vector3(0.150f, 0.000f, 0.000f), new Vector3(0.275f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.275f, 0.000f, 1.000f), new Vector3(0.650f, 0.000f, 1.000f), new Vector3(0.275f, 0.000f, 0.900f), new Vector3(0.650f, 0.000f, 0.900f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.650f, 0.000f, 0.900f), new Vector3(0.775f, 0.000f, 0.900f), new Vector3(0.650f, 0.000f, 0.600f), new Vector3(0.775f, 0.000f, 0.600f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.275f, 0.000f, 0.600f), new Vector3(0.775f, 0.000f, 0.600f), new Vector3(0.275f, 0.000f, 0.500f), new Vector3(0.775f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.775f, 0.000f, 0.500f), new Vector3(0.900f, 0.000f, 0.500f), new Vector3(0.775f, 0.000f, 0.100f), new Vector3(0.900f, 0.000f, 0.100f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.275f, 0.000f, 0.100f), new Vector3(0.775f, 0.000f, 0.100f), new Vector3(0.275f, 0.000f, 0.000f), new Vector3(0.775f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterC(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.350f, 0.000f, 1.000f), new Vector3(0.725f, 0.000f, 1.000f), new Vector3(0.350f, 0.000f, 0.875f), new Vector3(0.725f, 0.000f, 0.875f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.725f, 0.000f, 0.875f), new Vector3(0.850f, 0.000f, 0.875f), new Vector3(0.725f, 0.000f, 0.750f), new Vector3(0.850f, 0.000f, 0.750f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.225f, 0.000f, 0.875f), new Vector3(0.350f, 0.000f, 0.875f), new Vector3(0.225f, 0.000f, 0.750f), new Vector3(0.350f, 0.000f, 0.750f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.100f, 0.000f, 0.750f), new Vector3(0.225f, 0.000f, 0.750f), new Vector3(0.100f, 0.000f, 0.250f), new Vector3(0.225f, 0.000f, 0.250f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.225f, 0.000f, 0.250f), new Vector3(0.350f, 0.000f, 0.250f), new Vector3(0.225f, 0.000f, 0.125f), new Vector3(0.350f, 0.000f, 0.125f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.350f, 0.000f, 0.125f), new Vector3(0.725f, 0.000f, 0.125f), new Vector3(0.350f, 0.000f, 0.000f), new Vector3(0.725f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.725f, 0.000f, 0.250f), new Vector3(0.850f, 0.000f, 0.250f), new Vector3(0.725f, 0.000f, 0.125f), new Vector3(0.850f, 0.000f, 0.125f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterD(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.100f, 0.000f, 1.000f), new Vector3(0.220f, 0.000f, 1.000f), new Vector3(0.100f, 0.000f, 0.000f), new Vector3(0.220f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.220f, 0.000f, 1.000f), new Vector3(0.600f, 0.000f, 1.000f), new Vector3(0.220f, 0.000f, 0.880f), new Vector3(0.600f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.220f, 0.000f, 0.120f), new Vector3(0.600f, 0.000f, 0.120f), new Vector3(0.220f, 0.000f, 0.000f), new Vector3(0.600f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.600f, 0.000f, 0.880f), new Vector3(0.720f, 0.000f, 0.880f), new Vector3(0.600f, 0.000f, 0.760f), new Vector3(0.720f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.720f, 0.000f, 0.760f), new Vector3(0.840f, 0.000f, 0.760f), new Vector3(0.720f, 0.000f, 0.240f), new Vector3(0.840f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.600f, 0.000f, 0.240f), new Vector3(0.720f, 0.000f, 0.240f), new Vector3(0.600f, 0.000f, 0.120f), new Vector3(0.720f, 0.000f, 0.120f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterE(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.820f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.820f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.620f), new Vector3(0.700f, 0.000f, 0.620f), new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.700f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.120f), new Vector3(0.820f, 0.000f, 0.120f), new Vector3(0.320f, 0.000f, 0.000f), new Vector3(0.820f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterF(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.820f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.820f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.620f), new Vector3(0.700f, 0.000f, 0.620f), new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.700f, 0.000f, 0.500f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterG(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 1.000f), new Vector3(0.760f, 0.000f, 1.000f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.000f), new Vector3(0.760f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.880f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.360f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.240f), new Vector3(0.360f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.760f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.120f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.880f), new Vector3(0.880f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.880f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.500f), new Vector3(0.880f, 0.000f, 0.500f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.880f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.760f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.380f), new Vector3(0.760f, 0.000f, 0.380f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterH(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 1.000f), new Vector3(0.240f, 0.000f, 1.000f), new Vector3(0.120f, 0.000f, 0.000f), new Vector3(0.240f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.620f), new Vector3(0.760f, 0.000f, 0.620f), new Vector3(0.240f, 0.000f, 0.500f), new Vector3(0.760f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 1.000f), new Vector3(0.880f, 0.000f, 1.000f), new Vector3(0.760f, 0.000f, 0.000f), new Vector3(0.880f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterI(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.300f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.300f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.300f, 0.000f, 0.120f), new Vector3(0.700f, 0.000f, 0.120f), new Vector3(0.300f, 0.000f, 0.000f), new Vector3(0.700f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterJ(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 1.000f), new Vector3(0.720f, 0.000f, 1.000f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.720f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.600f, 0.000f, 0.880f), new Vector3(0.720f, 0.000f, 0.880f), new Vector3(0.600f, 0.000f, 0.120f), new Vector3(0.720f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.220f, 0.000f, 0.120f), new Vector3(0.600f, 0.000f, 0.120f), new Vector3(0.220f, 0.000f, 0.000f), new Vector3(0.600f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterK(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 1.000f), new Vector3(0.240f, 0.000f, 1.000f), new Vector3(0.120f, 0.000f, 0.000f), new Vector3(0.240f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.240f, 0.000f, 0.360f), new Vector3(0.500f, 0.000f, 0.360f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.380f, 0.000f, 0.640f), new Vector3(0.500f, 0.000f, 0.640f), new Vector3(0.380f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.360f), new Vector3(0.620f, 0.000f, 0.360f), new Vector3(0.500f, 0.000f, 0.240f), new Vector3(0.620f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.760f), new Vector3(0.620f, 0.000f, 0.760f), new Vector3(0.500f, 0.000f, 0.640f), new Vector3(0.620f, 0.000f, 0.640f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.240f), new Vector3(0.740f, 0.000f, 0.240f), new Vector3(0.620f, 0.000f, 0.120f), new Vector3(0.740f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.880f), new Vector3(0.740f, 0.000f, 0.880f), new Vector3(0.620f, 0.000f, 0.760f), new Vector3(0.740f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 0.120f), new Vector3(0.860f, 0.000f, 0.120f), new Vector3(0.740f, 0.000f, 0.000f), new Vector3(0.860f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 1.000f), new Vector3(0.860f, 0.000f, 1.000f), new Vector3(0.740f, 0.000f, 0.880f), new Vector3(0.860f, 0.000f, 0.880f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterL(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.120f), new Vector3(0.820f, 0.000f, 0.120f), new Vector3(0.320f, 0.000f, 0.000f), new Vector3(0.820f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterM(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.080f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.080f, 0.000f, 0.000f), new Vector3(0.200f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.760f), new Vector3(0.320f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.760f), new Vector3(0.440f, 0.000f, 0.760f), new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.440f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.500f), new Vector3(0.560f, 0.000f, 0.500f), new Vector3(0.440f, 0.000f, 0.260f), new Vector3(0.560f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.760f), new Vector3(0.680f, 0.000f, 0.760f), new Vector3(0.560f, 0.000f, 0.500f), new Vector3(0.680f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 1.000f), new Vector3(0.800f, 0.000f, 1.000f), new Vector3(0.680f, 0.000f, 0.760f), new Vector3(0.800f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.800f, 0.000f, 1.000f), new Vector3(0.920f, 0.000f, 1.000f), new Vector3(0.800f, 0.000f, 0.000f), new Vector3(0.920f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterN(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.140f, 0.000f, 1.000f), new Vector3(0.260f, 0.000f, 1.000f), new Vector3(0.140f, 0.000f, 0.000f), new Vector3(0.260f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.260f, 0.000f, 1.000f), new Vector3(0.380f, 0.000f, 1.000f), new Vector3(0.260f, 0.000f, 0.760f), new Vector3(0.380f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.380f, 0.000f, 0.760f), new Vector3(0.500f, 0.000f, 0.760f), new Vector3(0.380f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.620f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.240f), new Vector3(0.620f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.240f), new Vector3(0.740f, 0.000f, 0.240f), new Vector3(0.620f, 0.000f, 0.000f), new Vector3(0.740f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 1.000f), new Vector3(0.860f, 0.000f, 1.000f), new Vector3(0.740f, 0.000f, 0.000f), new Vector3(0.860f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterO(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.760f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.120f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.880f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.360f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.240f), new Vector3(0.360f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 1.000f), new Vector3(0.640f, 0.000f, 1.000f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.640f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 0.120f), new Vector3(0.640f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.000f), new Vector3(0.640f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.880f), new Vector3(0.640f, 0.000f, 0.760f), new Vector3(0.760f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.240f), new Vector3(0.760f, 0.000f, 0.240f), new Vector3(0.640f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.880f, 0.000f, 0.760f), new Vector3(0.760f, 0.000f, 0.240f), new Vector3(0.880f, 0.000f, 0.240f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterP(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.700f, 0.000f, 0.500f), new Vector3(0.320f, 0.000f, 0.380f), new Vector3(0.700f, 0.000f, 0.380f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.700f, 0.000f, 0.880f), new Vector3(0.820f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.500f), new Vector3(0.820f, 0.000f, 0.500f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterQ(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.760f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.120f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.880f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.360f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.240f), new Vector3(0.360f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 1.000f), new Vector3(0.640f, 0.000f, 1.000f), new Vector3(0.360f, 0.000f, 0.880f), new Vector3(0.640f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 0.120f), new Vector3(0.640f, 0.000f, 0.120f), new Vector3(0.360f, 0.000f, 0.000f), new Vector3(0.640f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.880f), new Vector3(0.640f, 0.000f, 0.760f), new Vector3(0.760f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.240f), new Vector3(0.760f, 0.000f, 0.240f), new Vector3(0.640f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.880f, 0.000f, 0.760f), new Vector3(0.760f, 0.000f, 0.240f), new Vector3(0.880f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.520f, 0.000f, 0.360f), new Vector3(0.640f, 0.000f, 0.360f), new Vector3(0.520f, 0.000f, 0.240f), new Vector3(0.640f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.960f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.000f), new Vector3(0.960f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterR(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.700f, 0.000f, 0.500f), new Vector3(0.320f, 0.000f, 0.380f), new Vector3(0.700f, 0.000f, 0.380f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.700f, 0.000f, 0.880f), new Vector3(0.820f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.500f), new Vector3(0.820f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.580f, 0.000f, 0.380f), new Vector3(0.700f, 0.000f, 0.380f), new Vector3(0.580f, 0.000f, 0.260f), new Vector3(0.700f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.700f, 0.000f, 0.260f), new Vector3(0.820f, 0.000f, 0.260f), new Vector3(0.700f, 0.000f, 0.00f), new Vector3(0.820f, 0.000f, 0.0f) }, type = "plane" });
        //meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.820f, 0.000f, 0.140f), new Vector3(0.940f, 0.000f, 0.140f), new Vector3(0.820f, 0.000f, 0.000f), new Vector3(0.940f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterS(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 1.000f), new Vector3(0.760f, 0.000f, 1.000f), new Vector3(0.240f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.240f, 0.000f, 0.000f), new Vector3(0.760f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.880f), new Vector3(0.880f, 0.000f, 0.880f), new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.880f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.380f), new Vector3(0.880f, 0.000f, 0.380f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.880f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.880f), new Vector3(0.240f, 0.000f, 0.880f), new Vector3(0.120f, 0.000f, 0.620f), new Vector3(0.240f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.240f), new Vector3(0.120f, 0.000f, 0.120f), new Vector3(0.240f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.620f), new Vector3(0.500f, 0.000f, 0.620f), new Vector3(0.240f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.760f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.380f), new Vector3(0.760f, 0.000f, 0.380f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterT(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.080f, 0.000f, 1.000f), new Vector3(0.920f, 0.000f, 1.000f), new Vector3(0.080f, 0.000f, 0.880f), new Vector3(0.920f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterU(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 1.000f), new Vector3(0.240f, 0.000f, 1.000f), new Vector3(0.120f, 0.000f, 0.120f), new Vector3(0.240f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.120f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.240f, 0.000f, 0.000f), new Vector3(0.760f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 1.000f), new Vector3(0.880f, 0.000f, 1.000f), new Vector3(0.760f, 0.000f, 0.120f), new Vector3(0.880f, 0.000f, 0.120f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterV(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.250f), new Vector3(0.560f, 0.000f, 0.250f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.440f, 0.000f, 0.500f), new Vector3(0.320f, 0.000f, 0.250f), new Vector3(0.440f, 0.000f, 0.250f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.500f), new Vector3(0.680f, 0.000f, 0.500f), new Vector3(0.560f, 0.000f, 0.250f), new Vector3(0.680f, 0.000f, 0.250f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.750f), new Vector3(0.320f, 0.000f, 0.750f), new Vector3(0.200f, 0.000f, 0.500f), new Vector3(0.320f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.750f), new Vector3(0.800f, 0.000f, 0.750f), new Vector3(0.680f, 0.000f, 0.500f), new Vector3(0.800f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.080f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.080f, 0.000f, 0.750f), new Vector3(0.200f, 0.000f, 0.750f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.800f, 0.000f, 1.000f), new Vector3(0.920f, 0.000f, 1.000f), new Vector3(0.800f, 0.000f, 0.750f), new Vector3(0.920f, 0.000f, 0.750f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterW(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.620f), new Vector3(0.560f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.620f), new Vector3(0.440f, 0.000f, 0.620f), new Vector3(0.320f, 0.000f, 0.260f), new Vector3(0.440f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.620f), new Vector3(0.680f, 0.000f, 0.620f), new Vector3(0.560f, 0.000f, 0.260f), new Vector3(0.680f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.380f), new Vector3(0.320f, 0.000f, 0.380f), new Vector3(0.200f, 0.000f, 0.000f), new Vector3(0.320f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.380f), new Vector3(0.800f, 0.000f, 0.380f), new Vector3(0.680f, 0.000f, 0.000f), new Vector3(0.800f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.080f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.080f, 0.000f, 0.380f), new Vector3(0.200f, 0.000f, 0.380f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.800f, 0.000f, 1.000f), new Vector3(0.920f, 0.000f, 1.000f), new Vector3(0.800f, 0.000f, 0.380f), new Vector3(0.920f, 0.000f, 0.380f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterX(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 1.000f), new Vector3(0.240f, 0.000f, 1.000f), new Vector3(0.120f, 0.000f, 0.760f), new Vector3(0.240f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.240f), new Vector3(0.240f, 0.000f, 0.240f), new Vector3(0.120f, 0.000f, 0.000f), new Vector3(0.240f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.760f), new Vector3(0.360f, 0.000f, 0.760f), new Vector3(0.240f, 0.000f, 0.640f), new Vector3(0.360f, 0.000f, 0.640f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.240f, 0.000f, 0.380f), new Vector3(0.360f, 0.000f, 0.380f), new Vector3(0.240f, 0.000f, 0.260f), new Vector3(0.360f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.360f, 0.000f, 0.640f), new Vector3(0.640f, 0.000f, 0.640f), new Vector3(0.360f, 0.000f, 0.360f), new Vector3(0.640f, 0.000f, 0.360f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.760f), new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.640f, 0.000f, 0.640f), new Vector3(0.760f, 0.000f, 0.640f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.640f, 0.000f, 0.380f), new Vector3(0.760f, 0.000f, 0.380f), new Vector3(0.640f, 0.000f, 0.260f), new Vector3(0.760f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 1.000f), new Vector3(0.880f, 0.000f, 1.000f), new Vector3(0.760f, 0.000f, 0.760f), new Vector3(0.880f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.760f, 0.000f, 0.240f), new Vector3(0.880f, 0.000f, 0.240f), new Vector3(0.760f, 0.000f, 0.000f), new Vector3(0.880f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }
    public static MeshGenItem MakeLetterY(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.500f), new Vector3(0.560f, 0.000f, 0.500f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.620f), new Vector3(0.440f, 0.000f, 0.620f), new Vector3(0.320f, 0.000f, 0.500f), new Vector3(0.440f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.620f), new Vector3(0.680f, 0.000f, 0.620f), new Vector3(0.560f, 0.000f, 0.500f), new Vector3(0.680f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.740f), new Vector3(0.320f, 0.000f, 0.740f), new Vector3(0.200f, 0.000f, 0.620f), new Vector3(0.320f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.740f), new Vector3(0.800f, 0.000f, 0.740f), new Vector3(0.680f, 0.000f, 0.620f), new Vector3(0.800f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.080f, 0.000f, 1.000f), new Vector3(0.200f, 0.000f, 1.000f), new Vector3(0.080f, 0.000f, 0.740f), new Vector3(0.200f, 0.000f, 0.740f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.800f, 0.000f, 1.000f), new Vector3(0.920f, 0.000f, 1.000f), new Vector3(0.800f, 0.000f, 0.740f), new Vector3(0.920f, 0.000f, 0.740f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeLetterZ(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 1.000f), new Vector3(0.880f, 0.000f, 1.000f), new Vector3(0.120f, 0.000f, 0.880f), new Vector3(0.880f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.120f), new Vector3(0.880f, 0.000f, 0.120f), new Vector3(0.120f, 0.000f, 0.000f), new Vector3(0.880f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 0.880f), new Vector3(0.880f, 0.000f, 0.880f), new Vector3(0.740f, 0.000f, 0.740f), new Vector3(0.880f, 0.000f, 0.740f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.740f), new Vector3(0.740f, 0.000f, 0.740f), new Vector3(0.620f, 0.000f, 0.620f), new Vector3(0.740f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.620f), new Vector3(0.620f, 0.000f, 0.620f), new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.620f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.380f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.380f, 0.000f, 0.380f), new Vector3(0.500f, 0.000f, 0.380f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.260f, 0.000f, 0.380f), new Vector3(0.380f, 0.000f, 0.380f), new Vector3(0.260f, 0.000f, 0.260f), new Vector3(0.380f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.120f, 0.000f, 0.260f), new Vector3(0.260f, 0.000f, 0.260f), new Vector3(0.120f, 0.000f, 0.120f), new Vector3(0.260f, 0.000f, 0.120f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymDot(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.260f), new Vector3(0.560f, 0.000f, 0.260f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymComma(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.380f), new Vector3(0.560f, 0.000f, 0.380f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.120f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.320f, 0.000f, 0.000f), new Vector3(0.440f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymColon(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.620f), new Vector3(0.560f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.380f), new Vector3(0.560f, 0.000f, 0.380f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymSemicolon(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.620f), new Vector3(0.560f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.380f), new Vector3(0.560f, 0.000f, 0.380f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.120f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.320f, 0.000f, 0.000f), new Vector3(0.440f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymApostrophe(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 1.000f), new Vector3(0.560f, 0.000f, 1.000f), new Vector3(0.440f, 0.000f, 0.740f), new Vector3(0.560f, 0.000f, 0.740f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymQuotationMark(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.300f, 0.000f, 1.000f), new Vector3(0.420f, 0.000f, 1.000f), new Vector3(0.300f, 0.000f, 0.740f), new Vector3(0.420f, 0.000f, 0.740f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.580f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.580f, 0.000f, 0.740f), new Vector3(0.700f, 0.000f, 0.740f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymHyphen(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.550f), new Vector3(0.800f, 0.000f, 0.550f), new Vector3(0.200f, 0.000f, 0.450f), new Vector3(0.800f, 0.000f, 0.450f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymUnderscore(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.100f, 0.000f, 0.100f), new Vector3(0.900f, 0.000f, 0.100f), new Vector3(0.100f, 0.000f, 0.000f), new Vector3(0.900f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymSlash(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 1.000f), new Vector3(0.740f, 0.000f, 1.000f), new Vector3(0.620f, 0.000f, 0.740f), new Vector3(0.740f, 0.000f, 0.740f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.740f), new Vector3(0.620f, 0.000f, 0.740f), new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.620f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.380f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.380f, 0.000f, 0.260f), new Vector3(0.500f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.260f, 0.000f, 0.260f), new Vector3(0.380f, 0.000f, 0.260f), new Vector3(0.260f, 0.000f, 0.000f), new Vector3(0.380f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymBackslash(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.260f, 0.000f, 1.000f), new Vector3(0.380f, 0.000f, 1.000f), new Vector3(0.260f, 0.000f, 0.740f), new Vector3(0.380f, 0.000f, 0.740f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.380f, 0.000f, 0.740f), new Vector3(0.500f, 0.000f, 0.740f), new Vector3(0.380f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.500f), new Vector3(0.620f, 0.000f, 0.500f), new Vector3(0.500f, 0.000f, 0.260f), new Vector3(0.620f, 0.000f, 0.260f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.260f), new Vector3(0.740f, 0.000f, 0.260f), new Vector3(0.620f, 0.000f, 0.000f), new Vector3(0.740f, 0.000f, 0.000f) }, type = "plane" });     
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymVertBar(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 1.000f), new Vector3(0.560f, 0.000f, 1.000f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymParenthesesLeft(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 1.000f), new Vector3(0.680f, 0.000f, 1.000f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.680f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.640f), new Vector3(0.560f, 0.000f, 0.640f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.640f), new Vector3(0.440f, 0.000f, 0.640f), new Vector3(0.320f, 0.000f, 0.360f), new Vector3(0.440f, 0.000f, 0.360f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.360f), new Vector3(0.560f, 0.000f, 0.360f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.120f), new Vector3(0.680f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.000f), new Vector3(0.680f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymParenthesesRight(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.440f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.880f), new Vector3(0.560f, 0.000f, 0.880f), new Vector3(0.440f, 0.000f, 0.640f), new Vector3(0.560f, 0.000f, 0.640f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.640f), new Vector3(0.680f, 0.000f, 0.640f), new Vector3(0.560f, 0.000f, 0.360f), new Vector3(0.680f, 0.000f, 0.360f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.360f), new Vector3(0.560f, 0.000f, 0.360f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.120f), new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.320f, 0.000f, 0.000f), new Vector3(0.440f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymBracketsLeft(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.425f, 0.000f, 1.000f), new Vector3(0.675f, 0.000f, 1.000f), new Vector3(0.425f, 0.000f, 0.875f), new Vector3(0.675f, 0.000f, 0.875f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.300f, 0.000f, 1.000f), new Vector3(0.425f, 0.000f, 1.000f), new Vector3(0.300f, 0.000f, 0.000f), new Vector3(0.425f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.425f, 0.000f, 0.125f), new Vector3(0.675f, 0.000f, 0.125f), new Vector3(0.425f, 0.000f, 0.000f), new Vector3(0.675f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymBracketsRight(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.325f, 0.000f, 1.000f), new Vector3(0.575f, 0.000f, 1.000f), new Vector3(0.325f, 0.000f, 0.875f), new Vector3(0.575f, 0.000f, 0.875f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.575f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.575f, 0.000f, 0.000f), new Vector3(0.700f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.325f, 0.000f, 0.125f), new Vector3(0.575f, 0.000f, 0.125f), new Vector3(0.325f, 0.000f, 0.000f), new Vector3(0.575f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymLessThan(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.600f, 0.000f, 0.800f), new Vector3(0.850f, 0.000f, 0.800f), new Vector3(0.600f, 0.000f, 0.680f), new Vector3(0.850f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.350f, 0.000f, 0.680f), new Vector3(0.600f, 0.000f, 0.680f), new Vector3(0.350f, 0.000f, 0.560f), new Vector3(0.600f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.100f, 0.000f, 0.560f), new Vector3(0.350f, 0.000f, 0.560f), new Vector3(0.100f, 0.000f, 0.440f), new Vector3(0.350f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.350f, 0.000f, 0.440f), new Vector3(0.600f, 0.000f, 0.440f), new Vector3(0.350f, 0.000f, 0.320f), new Vector3(0.600f, 0.000f, 0.320f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.600f, 0.000f, 0.320f), new Vector3(0.850f, 0.000f, 0.320f), new Vector3(0.600f, 0.000f, 0.200f), new Vector3(0.850f, 0.000f, 0.200f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymGreaterThan(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.150f, 0.000f, 0.800f), new Vector3(0.400f, 0.000f, 0.800f), new Vector3(0.150f, 0.000f, 0.680f), new Vector3(0.400f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.400f, 0.000f, 0.680f), new Vector3(0.650f, 0.000f, 0.680f), new Vector3(0.400f, 0.000f, 0.560f), new Vector3(0.650f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.650f, 0.000f, 0.560f), new Vector3(0.900f, 0.000f, 0.560f), new Vector3(0.650f, 0.000f, 0.440f), new Vector3(0.900f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.400f, 0.000f, 0.440f), new Vector3(0.650f, 0.000f, 0.440f), new Vector3(0.400f, 0.000f, 0.320f), new Vector3(0.650f, 0.000f, 0.320f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.150f, 0.000f, 0.320f), new Vector3(0.400f, 0.000f, 0.320f), new Vector3(0.150f, 0.000f, 0.200f), new Vector3(0.400f, 0.000f, 0.200f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymEquals(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.720f), new Vector3(0.800f, 0.000f, 0.720f), new Vector3(0.200f, 0.000f, 0.600f), new Vector3(0.800f, 0.000f, 0.600f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.400f), new Vector3(0.800f, 0.000f, 0.400f), new Vector3(0.200f, 0.000f, 0.280f), new Vector3(0.800f, 0.000f, 0.280f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymPlus(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.800f), new Vector3(0.560f, 0.000f, 0.800f), new Vector3(0.440f, 0.000f, 0.200f), new Vector3(0.560f, 0.000f, 0.200f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.560f), new Vector3(0.440f, 0.000f, 0.560f), new Vector3(0.200f, 0.000f, 0.440f), new Vector3(0.440f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.560f), new Vector3(0.800f, 0.000f, 0.560f), new Vector3(0.560f, 0.000f, 0.440f), new Vector3(0.800f, 0.000f, 0.440f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymAmpersand(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.280f, 0.000f, 0.920f), new Vector3(0.500f, 0.000f, 0.920f), new Vector3(0.280f, 0.000f, 0.800f), new Vector3(0.500f, 0.000f, 0.800f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.160f, 0.000f, 0.800f), new Vector3(0.280f, 0.000f, 0.800f), new Vector3(0.160f, 0.000f, 0.680f), new Vector3(0.280f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.800f), new Vector3(0.620f, 0.000f, 0.800f), new Vector3(0.500f, 0.000f, 0.680f), new Vector3(0.620f, 0.000f, 0.680f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.280f, 0.000f, 0.680f), new Vector3(0.500f, 0.000f, 0.680f), new Vector3(0.280f, 0.000f, 0.560f), new Vector3(0.500f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.160f, 0.000f, 0.560f), new Vector3(0.280f, 0.000f, 0.560f), new Vector3(0.160f, 0.000f, 0.120f), new Vector3(0.280f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.280f, 0.000f, 0.120f), new Vector3(0.620f, 0.000f, 0.120f), new Vector3(0.280f, 0.000f, 0.000f), new Vector3(0.620f, 0.000f, 0.000f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.500f, 0.000f, 0.560f), new Vector3(0.620f, 0.000f, 0.560f), new Vector3(0.500f, 0.000f, 0.440f), new Vector3(0.620f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.620f, 0.000f, 0.440f), new Vector3(0.740f, 0.000f, 0.440f), new Vector3(0.620f, 0.000f, 0.120f), new Vector3(0.740f, 0.000f, 0.120f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 0.680f), new Vector3(0.860f, 0.000f, 0.680f), new Vector3(0.740f, 0.000f, 0.440f), new Vector3(0.860f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.740f, 0.000f, 0.120f), new Vector3(0.860f, 0.000f, 0.120f), new Vector3(0.740f, 0.000f, 0.000f), new Vector3(0.860f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymPercentSign(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymExclamationMark(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 1.000f), new Vector3(0.560f, 0.000f, 1.000f), new Vector3(0.440f, 0.000f, 0.240f), new Vector3(0.560f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.120f), new Vector3(0.560f, 0.000f, 0.120f), new Vector3(0.440f, 0.000f, 0.000f), new Vector3(0.560f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymQuestionMark(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.880f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.200f, 0.000f, 0.760f), new Vector3(0.320f, 0.000f, 0.760f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 1.000f), new Vector3(0.700f, 0.000f, 1.000f), new Vector3(0.320f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.880f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.700f, 0.000f, 0.880f), new Vector3(0.820f, 0.000f, 0.880f), new Vector3(0.700f, 0.000f, 0.620f), new Vector3(0.820f, 0.000f, 0.620f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.580f, 0.000f, 0.620f), new Vector3(0.700f, 0.000f, 0.620f), new Vector3(0.580f, 0.000f, 0.500f), new Vector3(0.700f, 0.000f, 0.500f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.460f, 0.000f, 0.500f), new Vector3(0.580f, 0.000f, 0.500f), new Vector3(0.460f, 0.000f, 0.240f), new Vector3(0.580f, 0.000f, 0.240f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.460f, 0.000f, 0.120f), new Vector3(0.580f, 0.000f, 0.120f), new Vector3(0.460f, 0.000f, 0.000f), new Vector3(0.580f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymHashtag(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymAsterisk(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.440f, 0.000f, 0.800f), new Vector3(0.560f, 0.000f, 0.800f), new Vector3(0.440f, 0.000f, 0.200f), new Vector3(0.560f, 0.000f, 0.200f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.680f), new Vector3(0.320f, 0.000f, 0.680f), new Vector3(0.200f, 0.000f, 0.560f), new Vector3(0.320f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.320f, 0.000f, 0.560f), new Vector3(0.440f, 0.000f, 0.560f), new Vector3(0.320f, 0.000f, 0.440f), new Vector3(0.440f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.200f, 0.000f, 0.440f), new Vector3(0.320f, 0.000f, 0.440f), new Vector3(0.200f, 0.000f, 0.320f), new Vector3(0.320f, 0.000f, 0.320f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.680f), new Vector3(0.800f, 0.000f, 0.680f), new Vector3(0.680f, 0.000f, 0.560f), new Vector3(0.800f, 0.000f, 0.560f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.560f, 0.000f, 0.560f), new Vector3(0.680f, 0.000f, 0.560f), new Vector3(0.560f, 0.000f, 0.440f), new Vector3(0.680f, 0.000f, 0.440f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.680f, 0.000f, 0.440f), new Vector3(0.800f, 0.000f, 0.440f), new Vector3(0.680f, 0.000f, 0.320f), new Vector3(0.800f, 0.000f, 0.320f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSymDollarSign(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f), new Vector3(0.000f, 0.000f, 0.000f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeDiamond(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 1.8f, 0.5f), new Vector3(0.5f, 0.9f, 0.9f), new Vector3(0.9f, 0.9f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 1.8f, 0.5f), new Vector3(0.9f, 0.9f, 0.5f), new Vector3(0.5f, 0.9f, 0.1f), new Vector3(0.5f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 1.8f, 0.5f), new Vector3(0.5f, 0.9f, 0.1f), new Vector3(0.1f, 0.9f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f) }, type = "plane" });
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.5f, 1.8f, 0.5f), new Vector3(0.1f, 0.9f, 0.5f), new Vector3(0.5f, 0.9f, 0.9f), new Vector3(0.5f, 0.0f, 0.5f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    public static MeshGenItem MakeSquare(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0, float h1 = 0.0f, float h2 = 0.0f, float h3 = 0.0f, float h4 = 0.0f) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[4] { new Vector3(0.0f, h1+0.0f, 1.0f), new Vector3(1.0f, h2+0.0f, 1.0f), new Vector3(0.0f, h3+0.0f, 0.0f), new Vector3(1.0f, h4+0.0f, 0.0f) }, type = "plane" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }

    /*
    public static MeshGenItem MakeTriangle(Vector3 pos = new Vector3(), float sizeX = 1.0f, float sizeY = 1.0f, float sizeZ = 1.0f, float offsetX = 0.0f, float offsetY = 0.0f, float offsetZ = 0.0f, int numVerts = 0) {
        List<MeshGenData> meshData = new List<MeshGenData>();
        meshData.Add(new MeshGenData() { verts = new Vector3[3] { new Vector3(0.5f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f) }, type = "triangle" });
        foreach (MeshGenData m in meshData) { m.pos = pos; m.size = new Vector3(sizeX, sizeY, sizeZ); m.offset = new Vector3(offsetX, offsetY, offsetZ); }
        return Group(meshData, numVerts);
    }
    */

    public static MeshGenItem ModSize(MeshGenItem mesh, Vector3 size) {
        for (int i = 0; i < mesh.verts.Length; i++) {
            mesh.verts[i].x *= size.x;
            mesh.verts[i].y *= size.y;
            mesh.verts[i].z *= size.z;
        }
        return mesh;
    }

    public static MeshGenItem ModOffset(MeshGenItem mesh, Vector3 offset) {
        for (int i = 0; i < mesh.verts.Length; i++) {
            mesh.verts[i] += offset;
        }
        return mesh;
    }


    public static MeshGenItem ModRotate(MeshGenItem mesh, Quaternion rotation, Vector3 center = new Vector3()) {
        for (int i = 0; i < mesh.verts.Length; i++) {
            mesh.verts[i] = rotation * (mesh.verts[i] - center) + center;
        }
        return mesh;
    }

    
    public static MeshGenItem ModFlipYZ(MeshGenItem mesh) {
        for (int i = 0; i < mesh.verts.Length; i++) {
            float y = mesh.verts[i].y;
            mesh.verts[i].y = mesh.verts[i].z;
            mesh.verts[i].z = y;
        }
        return mesh;
    }
    public static MeshGenItem ModTwoSided(MeshGenItem mesh) {
        List<int> trisList = new List<int>();
        trisList.AddRange(mesh.tris);
        trisList.Reverse();
        trisList.AddRange(mesh.tris);
        mesh.tris = trisList.ToArray();
        return mesh;
    }

    public static MeshGenItem GroupMerge(List<MeshGenGroup> meshGroups, int numVerts = 0) {
        List<MeshGenItem> meshGroupMerge = new List<MeshGenItem>();
        int count = 0; int emptyCount = 0;
        foreach (MeshGenGroup m in meshGroups) {
            if (m.type == "square") { meshGroupMerge.Add(MakeSquare(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            //elseif (m.type == "triangle") { meshGroupMerge.Add(MakeTriangle(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } else if (m.type == "diamond") { meshGroupMerge.Add(MakeDiamond(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num0") { meshGroupMerge.Add(MakeNum0(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num1") { meshGroupMerge.Add(MakeNum1(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num2") { meshGroupMerge.Add(MakeNum2(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num3") { meshGroupMerge.Add(MakeNum3(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num4") { meshGroupMerge.Add(MakeNum4(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num5") { meshGroupMerge.Add(MakeNum5(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num6") { meshGroupMerge.Add(MakeNum6(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num7") { meshGroupMerge.Add(MakeNum7(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num8") { meshGroupMerge.Add(MakeNum8(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); } 
            else if (m.type == "num9") { meshGroupMerge.Add(MakeNum9(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterA") { meshGroupMerge.Add(MakeLetterA(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterB") { meshGroupMerge.Add(MakeLetterB(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterC") { meshGroupMerge.Add(MakeLetterC(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterD") { meshGroupMerge.Add(MakeLetterD(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterE") { meshGroupMerge.Add(MakeLetterE(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterF") { meshGroupMerge.Add(MakeLetterF(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterG") { meshGroupMerge.Add(MakeLetterG(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterH") { meshGroupMerge.Add(MakeLetterH(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterI") { meshGroupMerge.Add(MakeLetterI(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterJ") { meshGroupMerge.Add(MakeLetterJ(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterK") { meshGroupMerge.Add(MakeLetterK(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterL") { meshGroupMerge.Add(MakeLetterL(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterM") { meshGroupMerge.Add(MakeLetterM(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterN") { meshGroupMerge.Add(MakeLetterN(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterO") { meshGroupMerge.Add(MakeLetterO(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterP") { meshGroupMerge.Add(MakeLetterP(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterQ") { meshGroupMerge.Add(MakeLetterQ(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterR") { meshGroupMerge.Add(MakeLetterR(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterS") { meshGroupMerge.Add(MakeLetterS(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterT") { meshGroupMerge.Add(MakeLetterT(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterU") { meshGroupMerge.Add(MakeLetterU(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterV") { meshGroupMerge.Add(MakeLetterV(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterW") { meshGroupMerge.Add(MakeLetterW(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterX") { meshGroupMerge.Add(MakeLetterX(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterY") { meshGroupMerge.Add(MakeLetterY(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "letterZ") { meshGroupMerge.Add(MakeLetterZ(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symDot") { meshGroupMerge.Add(MakeSymDot(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symComma") { meshGroupMerge.Add(MakeSymComma(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symColon") { meshGroupMerge.Add(MakeSymColon(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symSemicolon") { meshGroupMerge.Add(MakeSymSemicolon(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symApostrophe") { meshGroupMerge.Add(MakeSymApostrophe(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symQuotationMark") { meshGroupMerge.Add(MakeSymQuotationMark(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symHyphen") { meshGroupMerge.Add(MakeSymHyphen(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symUnderscore") { meshGroupMerge.Add(MakeSymUnderscore(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symSlash") { meshGroupMerge.Add(MakeSymSlash(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symBackslash") { meshGroupMerge.Add(MakeSymBackslash(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symVertBar") { meshGroupMerge.Add(MakeSymVertBar(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symParenthesesLeft") { meshGroupMerge.Add(MakeSymParenthesesLeft(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symParenthesesRight") { meshGroupMerge.Add(MakeSymParenthesesRight(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symBracketsLeft") { meshGroupMerge.Add(MakeSymBracketsLeft(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symBracketsRight") { meshGroupMerge.Add(MakeSymBracketsRight(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symLessThan") { meshGroupMerge.Add(MakeSymLessThan(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symGreaterThan") { meshGroupMerge.Add(MakeSymGreaterThan(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symEquals") { meshGroupMerge.Add(MakeSymEquals(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symPlus") { meshGroupMerge.Add(MakeSymPlus(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symAmpersand") { meshGroupMerge.Add(MakeSymAmpersand(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symPercentSign") { meshGroupMerge.Add(MakeSymPercentSign(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symExclamationMark") { meshGroupMerge.Add(MakeSymExclamationMark(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symQuestionMark") { meshGroupMerge.Add(MakeSymQuestionMark(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symHashtag") { meshGroupMerge.Add(MakeSymHashtag(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symAsterisk") { meshGroupMerge.Add(MakeSymAsterisk(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            else if (m.type == "symDollarSign") { meshGroupMerge.Add(MakeSymDollarSign(m.pos, m.size.x, m.size.y, m.size.z, ((m.size.x * count) * m.offset.x), ((m.size.y * count) * m.offset.y), ((m.size.z * count) * m.offset.z), numVerts)); }
            if (m.type == "empty") { emptyCount += 1; }
            else { numVerts += meshGroupMerge[count-emptyCount].verts.Length; }
            count += 1;
        }
        return Combine(meshGroupMerge);
    }

    public static MeshGenItem Group(List<MeshGenData> meshData, int numVerts = 0) {
        List<MeshGenItem> meshGroup = new List<MeshGenItem>();
        int count = 0;
        foreach (MeshGenData m in meshData) {
            int count2 = 0;
            foreach (Vector3 verts in m.verts) {
                m.verts[count2].x = (m.verts[count2].x * m.size.x) + m.offset.x;
                m.verts[count2].y = (m.verts[count2].y * m.size.y) + m.offset.y;
                m.verts[count2].z = (m.verts[count2].z * m.size.z) + m.offset.z;
                m.verts[count2] += m.pos;
                count2++;
            }
            if (m.type == "plane") { meshGroup.Add(Plane(m.verts, numVerts)); }
            //else if (m.type == "triangle") { meshGroup.Add(MeshGenTriangle(m.verts, numVerts)); } 
            numVerts += meshGroup[count].verts.Length;
            count += 1;
        }
        return Combine(meshGroup);
    }

    public static MeshGenItem Combine(List<MeshGenItem> meshGroup) {
        List<Vector3> vertsList = new List<Vector3>();
        List<int> trisList = new List<int>();
        List<Vector2> uvList = new List<Vector2>();
        foreach (MeshGenItem m in meshGroup) {
            vertsList.AddRange(m.verts);
            trisList.AddRange(m.tris);
            uvList.AddRange(m.uv);
        }
        return new MeshGenItem() { verts = vertsList.ToArray(), tris = trisList.ToArray(), uv = uvList.ToArray() };
    }

    public static MeshGenItem Plane(Vector3[] pos, int numVerts = 0) {
        Vector3[] verts = new Vector3[4];
        int[] tris = new int[6];
        verts[0] = new Vector3(pos[0].x, pos[0].y, pos[0].z);
        verts[1] = new Vector3(pos[1].x, pos[1].y, pos[1].z);
        verts[2] = new Vector3(pos[2].x, pos[2].y, pos[2].z);
        verts[3] = new Vector3(pos[3].x, pos[3].y, pos[3].z);
        tris[0] = numVerts + 0;
        tris[1] = numVerts + 1;
        tris[2] = numVerts + 2;
        tris[3] = numVerts + 2;
        tris[4] = numVerts + 1;
        tris[5] = numVerts + 3;
        Vector2[] uv = new Vector2[4] {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
        };

        return new MeshGenItem() { verts = verts, tris = tris, uv = uv};
    }

    /*
    public static MeshGenItem Triangle(Vector3[] pos, int numVerts = 0) {
        Vector3[] verts = new Vector3[3];
        int[] tris = new int[3];
        verts[0] = new Vector3(pos[0].x, pos[0].y, pos[0].z);
        verts[1] = new Vector3(pos[1].x, pos[1].y, pos[1].z);
        verts[2] = new Vector3(pos[2].x, pos[2].y, pos[2].z);
        tris[0] = numVerts + 0;
        tris[1] = numVerts + 1;
        tris[2] = numVerts + 2;
        return new MeshGen() { verts = verts, tris = tris };
    }
    */
}
