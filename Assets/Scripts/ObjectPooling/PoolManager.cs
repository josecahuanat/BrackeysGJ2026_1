using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    
    [SerializeField] List<ObjectPool> pools = new List<ObjectPool>();
    
    Dictionary<PoolName, ObjectPool> poolDictionary = new Dictionary<PoolName, ObjectPool>();
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        foreach (var pool in pools)
            poolDictionary.Add(pool.GetPoolName(), pool);
    }

    public GameObject Spawn(PoolName poolName, Vector3 position, Quaternion rotation) => poolDictionary[poolName].Get(position, rotation);

    public void Despawn(PoolName poolName, GameObject obj) => poolDictionary[poolName].Return(obj);
}
