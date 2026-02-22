using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }

    [Header("Generation Settings")]
    [SerializeField] AdaptiveAudioManager audioManager;
    [SerializeField] Transform player;
    [SerializeField] Transform groundsParent;
    [SerializeField] float chunkSize;
    [SerializeField] int renderDistance;

    [Header("Puzzle Zones")]
    [SerializeField] PuzzleData[] puzzles;
    [SerializeField] PuzzleZoneConfig[] availablePuzzleConfigs;

    Dictionary<Vector2Int, Ground> activeGroundChunks;
    List<Ground> activeGroundsList;
    Vector2Int currentChunkCoord;
    int seed;
    int puzzleIndex, closestGroundIndex;

    int CurrentPuzzleDiff => activeGroundsList.Count == 0? 0 : activeGroundsList[closestGroundIndex].PuzzleDifficulty;
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        activeGroundChunks = new Dictionary<Vector2Int, Ground>();
        activeGroundsList = new List<Ground>();
        seed = Random.Range(0, 100000);
        Random.InitState(seed);
        
        currentChunkCoord = GetChunkCoord(player.position);
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int coord = currentChunkCoord + new Vector2Int(x, z);
                GenerateGroundChunk(coord);
            }
        }

        UpdateLampPosts();
    }
    
    void Update()
    {
        if (player == null)
            return;

        Vector2Int newChunkCoord = GetChunkCoord(player.position);
        
        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            UpdateChunks();

            closestGroundIndex = activeGroundsList.IndexOf(activeGroundChunks[currentChunkCoord]);
            UpdateLampPosts();
        }
    }
    
    Vector2Int GetChunkCoord(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }
    
    void UpdateChunks()
    {
        // Generate new chunks
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int coord = currentChunkCoord + new Vector2Int(x, z);
                if (!activeGroundChunks.ContainsKey(coord))
                    GenerateGroundChunk(coord);
            }
        }
        
        // Despawn far chunks (using pooling!)
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeGroundChunks)
        {
            if (Mathf.Abs(chunk.Key.x - currentChunkCoord.x) > renderDistance ||
                Mathf.Abs(chunk.Key.y - currentChunkCoord.y) > renderDistance)
                chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var coord in chunksToRemove)
            DespawnChunk(coord);
    }
   
    void GenerateGroundChunk(Vector2Int coord)
    {
        // Debug.Log(coord);
        Random.InitState(seed + coord.x * 1000 + coord.y);
        
        Vector3 chunkOrigin = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        
        Vector3 groundPosition = chunkOrigin + new Vector3(chunkSize / 2f, -0.05f, chunkSize / 2f);
        GameObject ground = PoolManager.Instance.Spawn(
            PoolName.Grounds, groundsParent, groundPosition, Quaternion.identity);
        
        //ground.transform.localScale = new Vector3(chunkSize / 10, 1, chunkSize / 10);
        activeGroundChunks[coord] = ground.GetComponent<Ground>();
        // Debug.Log($"{ground}, {activeGroundChunks[coord]}");
        activeGroundChunks[coord].Initialize();
        activeGroundsList.Add(activeGroundChunks[coord]);
    }

    void DespawnChunk(Vector2Int coord)
    {
        activeGroundChunks[coord].Despawn();
        activeGroundsList.Remove(activeGroundChunks[coord]);
        activeGroundChunks.Remove(coord);
    }

    void OnDestroy()
    {
        foreach (var coord in new List<Vector2Int>(activeGroundChunks.Keys))
            DespawnChunk(coord);
    }

    void UpdateLampPosts()
    {
        audioManager.SwitchToLayer(CurrentPuzzleDiff + 1);
        // Debug.Log("UpdateLampPosts");
        foreach(var ground in activeGroundsList)
        {
            // Debug.Log($"{CurrentPuzzleDiff}, {ground.PuzzleDifficulty}");
            if (ground.PuzzleDifficulty > CurrentPuzzleDiff)
                ground.PZL.SetLampPostColor(LampPostColor.Red);
            else if (ground.PuzzleDifficulty < CurrentPuzzleDiff)
                ground.PZL.SetLampPostColor(LampPostColor.Blue);
            else
            {
                ground.PZL.SetLampPostColor(LampPostColor.Yellow);
            }
        }
    }

    public PuzzleDifficulty GetPuzzle()
    {
        int minPuzzleDifficulty = CurrentPuzzleDiff == 0? 0 : CurrentPuzzleDiff - 1;
        int maxPuzzleDifficulty = puzzles[puzzleIndex].difficulties.Length > CurrentPuzzleDiff + 1? CurrentPuzzleDiff + 1 : CurrentPuzzleDiff;
        // Debug.Log($"{minPuzzleDifficulty}, {maxPuzzleDifficulty}");

        bool foundMinDiff = false, foundMaxDiff = false;
        foreach(var ground in activeGroundsList)
        {
            if (ground.PuzzleDifficulty == minPuzzleDifficulty)
                foundMinDiff = true;

            if (ground.PuzzleDifficulty == maxPuzzleDifficulty)
                foundMaxDiff = true;

            if (foundMinDiff && foundMaxDiff)
                break;
        }

        if (!foundMinDiff)
            return puzzles[puzzleIndex].difficulties[minPuzzleDifficulty];
        else if (!foundMaxDiff)
            return puzzles[puzzleIndex].difficulties[maxPuzzleDifficulty];
        else
        {
            int randomPuzzleDiff = Random.Range(minPuzzleDifficulty, maxPuzzleDifficulty + 1);
            return puzzles[puzzleIndex].difficulties[randomPuzzleDiff];
        }
    }
}
