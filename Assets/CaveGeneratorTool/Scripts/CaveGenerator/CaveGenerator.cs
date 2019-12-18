using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CaveGenerator
{
    private const string WallsAssetSulfix = "WallMesh";
    private const string ShapeAssetSulfix = "ShapeMesh";
    private const string GroundAssetSulfix = "GroundMesh";

    private MeshModifier meshGenerator;
    private CaveDataGenerator caveGenerator;
    private int borderDensity;
    private string prefabName;
    public CaveGenerator()
    {
        meshGenerator = new MeshModifier();
        caveGenerator = new CaveDataGenerator();
        borderDensity = 0;
        prefabName = string.Empty;
    }

    public CaveGenerator(string seed, int width, int height, int wallDensityPercentaje, int algorithmIterations, int borderDensity, int wallRegionMinDensity,
                            int roomRegionMinDensity, int passagesDensity, float scale, float wallsAltitude, string name)
    {
        prefabName = name;
        this.borderDensity = borderDensity;
        meshGenerator = new MeshModifier(scale, wallsAltitude);
        int numericSeed = GetNumericSeed(seed);
        caveGenerator = new CaveDataGenerator(numericSeed, width, height, wallDensityPercentaje, algorithmIterations, borderDensity, wallRegionMinDensity, roomRegionMinDensity,
                                                passagesDensity);
    }


    public void GenerateCave(string seed, int width, int height, int wallDensityPercentaje, int algorithmIterations, int borderDensity, int wallRegionMinDensity,
                            int roomRegionMinDensity, int passagesDensity, float scale, float wallsAltitude, Material groundMaterial, Material wallsMaterial, string name)
    {
        prefabName = name;
        this.borderDensity = borderDensity;
        int[,] data = GenerateMapData(seed, width, height, wallDensityPercentaje, algorithmIterations, borderDensity, wallRegionMinDensity, roomRegionMinDensity, passagesDensity);
        meshGenerator.SetProperties(scale, wallsAltitude);
        GenerateCavePrefab(data, groundMaterial, wallsMaterial);

    }

    public void GenerateCave(Material groundMaterial, Material wallsMaterial)
    {
        int[,] data = GenerateMapData();
        GenerateCavePrefab(data, groundMaterial, wallsMaterial);
    }

    public int[,] GenerateMapData(string seed, int width, int height, int wallOverallDensity, int algorithmIterations, int borderDensity, int wallRegionMinDensity,
                            int roomRegionMinDensity, int passagesDensity)
    {
        int numericSeed = GetNumericSeed(seed);
        caveGenerator.SetSeed(numericSeed);
        caveGenerator.SetConfiguration(width, height, wallOverallDensity, algorithmIterations, borderDensity,
                                                algorithmIterations, roomRegionMinDensity, passagesDensity);

        return GenerateMapData();
    }

    public int[,] GenerateMapData()
    {
        return caveGenerator.GenerateCave();
    }

    private void GenerateCavePrefab(int[,] data, Material groundMaterial, Material wallsMaterial)
    {
        float scale = meshGenerator.Scale;
        float wallsHeight = meshGenerator.WallHeight;

        List<Material> groundMats = new List<Material>();
        groundMats.Add(groundMaterial);

        List<Material> wallsMats = new List<Material>();
        wallsMats.Add(wallsMaterial);

        GameObject cave = new GameObject(prefabName);
        cave.transform.position = Vector3.zero;


        GameObject caveGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        caveGround.name = prefabName + GroundAssetSulfix;
        caveGround.transform.parent = cave.transform;
        caveGround.transform.localPosition = new Vector3(-25.0f  - ((float)borderDensity / 2.0f)* scale, 0, 0);
        caveGround.transform.localScale = new Vector3(5.0f * scale, 1.0f, 10.0f * scale);
        caveGround.GetComponent<MeshRenderer>().materials = groundMats.ToArray();

        string caveShapeName = prefabName + ShapeAssetSulfix;
        GameObject caveShape = new GameObject(caveShapeName);
        caveShape.transform.parent = cave.transform;
        
        caveShape.transform.localPosition = new Vector3(0, wallsHeight, 0);
        MeshFilter shapeMeshFilter = caveShape.AddComponent<MeshFilter>();
        MeshRenderer shapeRenderer = caveShape.AddComponent<MeshRenderer>();
        shapeRenderer.materials = wallsMats.ToArray();

        string caveWallsName = prefabName + WallsAssetSulfix;
        GameObject caveWalls = new GameObject(caveWallsName);
        caveWalls.transform.parent = cave.transform;
        caveWalls.transform.localPosition = new Vector3(0, wallsHeight, 0);
        MeshFilter wallsMeshFilter = caveWalls.AddComponent<MeshFilter>();
        MeshRenderer wallsRenderer = caveWalls.AddComponent<MeshRenderer>();
        wallsRenderer.materials = wallsMats.ToArray();

        caveWalls.transform.SetAsFirstSibling();
        caveShape.transform.SetAsFirstSibling();


        Mesh shapeMesh = new Mesh();
        Mesh wallsMesh = new Mesh();
        int[,] meshData = caveGenerator.GetBorderedCave(data);
        meshGenerator.ModifyMesh(shapeMesh, wallsMesh, meshData, scale, wallsHeight);

        string shapeAssetPath = CaveGeneratorUtilities.CaveSave.RootFolderPath + CaveGeneratorUtilities.CaveSave.MeshFolderPath + "/" + caveShape.name + CaveGeneratorUtilities.CaveSave.AssetExtention;
        string wallsAssetPath = CaveGeneratorUtilities.CaveSave.RootFolderPath + CaveGeneratorUtilities.CaveSave.MeshFolderPath + "/" + caveWalls.name + CaveGeneratorUtilities.CaveSave.AssetExtention;

        AssetDatabase.CreateAsset(shapeMesh, shapeAssetPath);
        AssetDatabase.CreateAsset(wallsMesh, wallsAssetPath);

        AssetDatabase.SaveAssets();

        Mesh shapeMeshAsset = AssetDatabase.LoadAssetAtPath(shapeAssetPath, typeof(Mesh)) as Mesh;
        Mesh wallsMeshAsset = AssetDatabase.LoadAssetAtPath(wallsAssetPath, typeof(Mesh)) as Mesh;

        shapeMeshFilter.mesh = shapeMeshAsset;
        wallsMeshFilter.mesh = wallsMeshAsset;
        meshGenerator.SetMeshCollider(caveWalls, wallsMeshAsset);

        Cave caveComponent = cave.AddComponent<Cave>();
        caveComponent.Scale = scale;
        caveComponent.Data = CaveGeneratorUtilities.Matrix.MatrixToTileList(data);
        caveComponent.Width = data.GetLength(0);
        caveComponent.Height = data.GetLength(1);

        string prefabPath = CaveGeneratorUtilities.CaveSave.RootFolderPath + CaveGeneratorUtilities.CaveSave.PrefabFolderPath + "/" + cave.name + CaveGeneratorUtilities.CaveSave.PrefabExtention;
        PrefabUtility.SaveAsPrefabAsset(cave, prefabPath);
        GameObject.DestroyImmediate(cave);
    }

    private int GetNumericSeed(string seed)
    {
        int numericSeed;

        if (!Int32.TryParse(seed, out numericSeed))
            numericSeed = seed.GetHashCode();

        return numericSeed;
    }
}
