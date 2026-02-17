using UnityEngine;
using System.Collections.Generic;

public class CemeteryGeneratorPooled : MonoBehaviour
{
    const float NicheChance = 0.4f, 
        NicheBlockChance = 0.1f,
        TombChance = 0.2f,
        MausoleumChance = 0.1f;


    [Header("Generation Settings")]
    [SerializeField] Transform player;
    [SerializeField] float chunkSize = 50f;
    [SerializeField] int renderDistance = 3;
    
    [Header("Spacing")]
    [SerializeField] float pathWidth = 10f;
    [SerializeField] float structureSpacing = 15f;
    
    Dictionary<Vector2Int, ChunkData> activeChunks = new Dictionary<Vector2Int, ChunkData>();
    Vector2Int currentChunkCoord;
    int seed;
    
    class ChunkData
    {
        public List<GameObject> grounds = new List<GameObject>(),
            niches = new List<GameObject>(),
            nicheBlocks = new List<GameObject>(),
            tombs = new List<GameObject>(),
            mausoleums = new List<GameObject>();

        public void Add(PoolName poolName, GameObject newObj)
        {
            switch(poolName)
            {
                case PoolName.Niches: niches.Add(newObj); break;
                case PoolName.NicheBlocks: nicheBlocks.Add(newObj); break;
                case PoolName.Tombs: tombs.Add(newObj); break;
                case PoolName.Mausoleums: mausoleums.Add(newObj); break;
                case PoolName.Grounds: grounds.Add(newObj); break;
            }
        }

        public void Despawn()
        {
            DespawnWithPoolName(grounds, PoolName.Grounds);
            DespawnWithPoolName(niches, PoolName.Niches);
            DespawnWithPoolName(nicheBlocks, PoolName.NicheBlocks);
            DespawnWithPoolName(tombs, PoolName.Tombs);
            DespawnWithPoolName(mausoleums, PoolName.Mausoleums);
        }

        void DespawnWithPoolName(List<GameObject> objs, PoolName poolName)
        {
            foreach (GameObject obj in objs)
                PoolManager.Instance.Despawn(poolName, obj);
        }
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
                GenerateChunk(coord);
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
                if (!activeChunks.ContainsKey(coord))
                    GenerateChunk(coord);
            }
        }
        
        // Despawn far chunks (using pooling!)
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (Mathf.Abs(chunk.Key.x - currentChunkCoord.x) > renderDistance ||
                Mathf.Abs(chunk.Key.y - currentChunkCoord.y) > renderDistance)
                chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var coord in chunksToRemove)
            DespawnChunk(coord);
    }
    
    void GenerateChunk(Vector2Int coord)
    {
        Random.InitState(seed + coord.x * 1000 + coord.y);
        
        ChunkData chunkData = new ChunkData();
        activeChunks[coord] = chunkData;
        
        Vector3 chunkOrigin = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        
        GameObject ground = PoolManager.Instance.Spawn(
            PoolName.Grounds,
            chunkOrigin + new Vector3(chunkSize / 2, -0.1f, chunkSize / 2),
            Quaternion.identity
        );

        ground.transform.localScale = new Vector3(chunkSize / 10, 1, chunkSize / 10);
        chunkData.grounds.Add(ground);
        
        int gridSize = Mathf.FloorToInt(chunkSize / structureSpacing);
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 localPosition = new Vector3(
                    x * structureSpacing + Random.Range(-2f, 2f),
                    0f,
                    z * structureSpacing + Random.Range(-2f, 2f)
                );
                
                Vector3 worldPosition = chunkOrigin + localPosition;
                
                // Skip path center
                if (Mathf.Abs(localPosition.x - chunkSize / 2f) < pathWidth / 2f && 
                    Mathf.Abs(localPosition.z - chunkSize / 2f) < pathWidth / 2f)
                    continue;
                
                PoolName poolName = PoolName.None;
                float roll = Random.value;
                if (roll < NicheChance)
                    poolName = PoolName.Niches;
                else if (roll < NicheChance + NicheBlockChance)
                    poolName = PoolName.NicheBlocks;
                else if (roll < NicheChance + NicheBlockChance + TombChance)
                    poolName = PoolName.Tombs;
                else if (roll < NicheChance + NicheBlockChance + TombChance + MausoleumChance)
                    poolName = PoolName.Mausoleums;

                if (poolName != PoolName.None)
                {
                    GameObject structure = PoolManager.Instance.Spawn(poolName, worldPosition, Quaternion.identity);
                    structure.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
                    chunkData.Add(poolName, structure);
                }
            }
        }
    }
    
    void DespawnChunk(Vector2Int coord)
    {
        activeChunks[coord].Despawn();
        activeChunks.Remove(coord);
    }

    void OnDestroy()
    {
        foreach (var coord in new List<Vector2Int>(activeChunks.Keys))
            DespawnChunk(coord);
    }
}
