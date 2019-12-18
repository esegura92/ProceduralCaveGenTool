using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;

public class CaveGeneratorWindow : EditorWindow
{
    private const string Title = "Cave Generator";

    private string prefabName = "Cave";
    private string seed = "LlamaZooSeed";
    private int mapwidth = 100;
    private int mapHeight = 100;
    
    private int iterations = 5;
    private int wallDensityPercentaje = 41;
    private int borderDensity = 4;
    private int minWallRegionDensity = 5;
    private int minRoomRegionDensity = 100;
    private int connetionsDensity = 2;
    private int scale = 1;
    private int wallsAltitude = 3;
    private Texture2D texture;
    private Material wallsMaterial;
    private Material groundMaterial;

    private CaveGenerator caveGenerator;
    private AnimBool areAdvancedOptionsActive;

    [MenuItem("Tools/Cave Generator")]
    public static void OpenWindow()
    {
        CaveGeneratorWindow window = GetWindow<CaveGeneratorWindow>(Title);
        CaveGeneratorUtilities.CaveSave.CreateAssetsDirectories();
    }


    private void OnEnable()
    {
        caveGenerator = new CaveGenerator();
        areAdvancedOptionsActive = new AnimBool(false, Repaint);
        texture = new Texture2D(mapwidth, mapHeight);
        wallsMaterial = AssetDatabase.LoadAssetAtPath(CaveGeneratorUtilities.CaveSave.RootFolderPath + "/Materials/DefaultCaveWalls.mat", typeof(Material)) as Material;
        groundMaterial = AssetDatabase.LoadAssetAtPath(CaveGeneratorUtilities.CaveSave.RootFolderPath + "/Materials/DefaultCaveGround.mat", typeof(Material)) as Material;
    }

    private void OnGUI()
    {
        EditorGUILayout.PrefixLabel("Preview", EditorStyles.boldLabel);
        GUILayout.Button(texture);
        if(GUILayout.Button("Update Preview"))
        {
            GeneratePreviewTexture();
        } // end if

        seed = EditorGUILayout.TextField("Seed", seed);
        //begin horizontal
        mapwidth = EditorGUILayout.IntField("Width", mapwidth);
        mapHeight = EditorGUILayout.IntField("Height", mapHeight);
        groundMaterial = EditorGUILayout.ObjectField("Cave ground Material", groundMaterial, typeof(Material)) as Material;
        wallsMaterial = EditorGUILayout.ObjectField("Cave walls Material", wallsMaterial, typeof(Material)) as Material;

        areAdvancedOptionsActive.target = EditorGUILayout.Foldout(areAdvancedOptionsActive.target, "Advanced Options");
        if (EditorGUILayout.BeginFadeGroup(areAdvancedOptionsActive.faded))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel("Walls Density: " + wallDensityPercentaje.ToString(), EditorStyles.boldLabel);
            wallDensityPercentaje = (int)GUILayout.HorizontalSlider((float)wallDensityPercentaje, 0f, 100f);
            EditorGUILayout.PrefixLabel("Algorithm Iterations: ", EditorStyles.boldLabel);
            iterations = EditorGUILayout.IntField(iterations);
            EditorGUILayout.PrefixLabel("Border Density: ", EditorStyles.boldLabel);
            borderDensity = EditorGUILayout.IntField(borderDensity);
            EditorGUILayout.PrefixLabel("Minimal Wall Density: ", EditorStyles.boldLabel);
            minWallRegionDensity = EditorGUILayout.IntField(minWallRegionDensity);
            EditorGUILayout.PrefixLabel("Minimal Room Density: ", EditorStyles.boldLabel);
            minRoomRegionDensity = EditorGUILayout.IntField(minRoomRegionDensity);
            EditorGUILayout.PrefixLabel("Connection radius: ", EditorStyles.boldLabel);
            connetionsDensity = EditorGUILayout.IntField(connetionsDensity);
            EditorGUILayout.PrefixLabel("Individual Size: ", EditorStyles.boldLabel);
            scale = EditorGUILayout.IntField(scale);
            EditorGUILayout.PrefixLabel("Mesh height: ", EditorStyles.boldLabel);
            wallsAltitude = EditorGUILayout.IntField(wallsAltitude);
            EditorGUI.indentLevel--;
        } // end if
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.PrefixLabel("Prefab Name: ", EditorStyles.boldLabel);
        prefabName = EditorGUILayout.TextField(prefabName);

        if (GUILayout.Button("SavePrefab"))
        {
            GenerateCave();
        } // end if

        //end horizontal

    }

    private void GenerateCave()
    {
        CaveGeneratorUtilities.CaveSave.CreateAssetsDirectories();
        caveGenerator.GenerateCave(seed, mapwidth, mapHeight, wallDensityPercentaje, iterations, borderDensity, minWallRegionDensity, minRoomRegionDensity, connetionsDensity, scale,
                                        wallsAltitude, groundMaterial, wallsMaterial, prefabName);
    }

    public void GeneratePreviewTexture()
    {
        int[,] caveMap = caveGenerator.GenerateMapData(seed, mapwidth, mapHeight, wallDensityPercentaje, iterations, borderDensity, minWallRegionDensity, minRoomRegionDensity, connetionsDensity);
        texture = CaveGeneratorUtilities.TextureUtilities.CreateMapTexture(caveMap, Color.black, Color.grey, Color.cyan, Color.green, 300, 300);
    }
}
