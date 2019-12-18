using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Cave level;
    private PlayerTracker tracker;
    [SerializeField]
    private UIMinimapRenderer uiRenderer;
    [SerializeField]
    private float updateDistance = 1;
    [SerializeField]
    private int mapRange = 20;
    private Vector3 lastSavedPosition;
    [SerializeField]
    private GameObject CollectablePrefab;
    [SerializeField]
    private Transform CollectablesContainer;
    private void Awake()
    {
        tracker = new PlayerTracker(mapRange);
    }

    private void Start()
    {
        lastSavedPosition = player.position;
        SpawnCollectables();
        UpdateMiniMap();
    }

    public void UpdateMiniMap()
    {

        CaveTile playerTile = level.GetTileAtPosition(player.position);
        int[,] miniMapData = tracker.TrackPlayer(playerTile, level);
        uiRenderer.SetMiniMap(miniMapData);
    }

    private void SpawnCollectables()
    {
        List<CaveTile> collectableTiles = level.GetTilesWithValue(3);
        for(int i = 0; i < collectableTiles.Count; i++)
        {
            CaveTile tile = collectableTiles[i];
            Vector3 position = level.GetPositionAtTile(tile.X, tile.Y);
            position.y += 0.25f;
            InstantiateGameObject(CollectablePrefab, position, CollectablesContainer);
        } // end if
    }

    private void InstantiateGameObject(GameObject go, Vector3 position, Transform parent)
    {
        GameObject goInstance = Instantiate(go, parent);
        goInstance.transform.position = position;
    }

    private void Update()
    {
        if(Vector3.Distance(lastSavedPosition, player.position) >= updateDistance)
        {
            lastSavedPosition = player.position;
            UpdateMiniMap();
        }
    }

}
