
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrintConsole : MonoBehaviour
{
    public static void Print(string s)
    {
        Debug.LogError(s);
    }
}

public static class CaveGeneratorUtilities
{
    

    public static class DataConstants
    {
        public const int Ground = 0;
        public const int Wall = 1;
        public const int Passage = 2;
        public const int Collectable = 3;
        public const int Player = 10;
    }

    public static class CaveSave
    {
        public const string RootFolderPath = "Assets/CaveGeneratorTool";
        public const string MeshFolderPath = "/Meshes";
        public const string PrefabFolderPath = "/Prefabs";
        public const string AssetExtention = ".asset";
        public const string PrefabExtention = ".prefab";

        public static void CreateAssetsDirectories()
        {
            if(!AssetDatabase.IsValidFolder(RootFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CaveGeneratorTool");
            } // end if
            CreateMeshesDirectory();
            CreatePrefabsDirectory();
        }

        public static void CreateMeshesDirectory()
        {
            string path = RootFolderPath + MeshFolderPath;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(RootFolderPath, "Meshes");
            }
        }

        public static void CreatePrefabsDirectory()
        {
            string path = RootFolderPath + PrefabFolderPath;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(RootFolderPath, "Prefabs");
            }
        }
    }

    public static class Matrix
    {
        public static List<CaveTile> MatrixToTileList(int[,] data)
        {
            List<CaveTile> tiles = new List<CaveTile>();

            for (int row = 0; row < data.GetLength(0); row++)
            {
                for (int col = 0; col < data.GetLength(0); col++)
                {
                    CaveTile tile = new CaveTile(row, col, data[row, col]);
                    tiles.Add(tile);
                } // end for
            } // end for

            return tiles;
        }
    }

    public static class TextureUtilities
    {
        public static Texture2D CreateMapTexture(int[,] pixels, Color wallColor, Color groundColor, Color colletableColor, Color playerColor, int width, int height)
        {
            Texture2D texture = new Texture2D(pixels.GetLength(0), pixels.GetLength(1));

            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    Color color = groundColor;
                    switch (pixels[i, j])
                    {
                        case DataConstants.Wall:
                            color = wallColor;
                            break;
                        case DataConstants.Collectable:
                            color = colletableColor;
                            break;
                        case DataConstants.Player:
                            color = playerColor;
                            break;
                    }
                    texture.SetPixel(i, j, color);
                } // end for
            } // end for

            ScaleTexture(texture, width, height, FilterMode.Trilinear);

            Rect scaledRect = new Rect(0, 0, width, height);
            texture.Resize(width, height);
            texture.ReadPixels(scaledRect, 0, 0, true);

            texture.Apply();

            return texture;
        }
        private static void ScaleTexture(Texture2D texture, int width, int height, FilterMode fmode)
        {
            texture.filterMode = fmode;
            texture.Apply(true);

            RenderTexture rtt = new RenderTexture(width, height, 32);

            Graphics.SetRenderTarget(rtt);

            GL.LoadPixelMatrix(0, 1, 1, 0);

            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), texture);
        }
    }
    
}
