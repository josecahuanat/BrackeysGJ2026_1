using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }

    [Header("Generation Settings")]
    [SerializeField] Transform player;
    [SerializeField] Transform groundsParent;
    [SerializeField] float chunkSize;
    [SerializeField] int renderDistance;

    [Header("Puzzle Zones")]
    [SerializeField] PuzzleData[] puzzles;
    [SerializeField] PuzzleZoneConfig[] availablePuzzleConfigs;
    [SerializeField] [Range(0f,1f)] float puzzleChunkProbability = 0.3f;

    Dictionary<Vector2Int, Ground> activeGroundChunks = new Dictionary<Vector2Int, Ground>();
    Vector2Int currentChunkCoord;
    int seed;
    int puzzleIndex, puzzleDifficulty;
    
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

    }
    
    void Update()
    {
        Vector2Int newChunkCoord = GetChunkCoord(player.position);
        
        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            UpdateChunks();
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
    }

    void DespawnChunk(Vector2Int coord)
    {
        activeGroundChunks[coord].Despawn();
        activeGroundChunks.Remove(coord);
    }

    void OnDestroy()
    {
        foreach (var coord in new List<Vector2Int>(activeGroundChunks.Keys))
            DespawnChunk(coord);
    }

    public PuzzleDifficulty GetPuzzle()
    {
        return puzzles[puzzleIndex].difficulties[puzzleDifficulty];
    }
}
