using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] PoolName poolName;
    [SerializeField] GameObject prefab;
    [SerializeField] int initialSize = 10;
    
    Queue<GameObject> availableObjects = new Queue<GameObject>();
    List<GameObject> allObjects = new List<GameObject>();
    
    void Awake()
    {        
        for (int i = 0; i < initialSize; i++)
            CreateNewObject();
    }
    
    GameObject CreateNewObject()
    {        
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        allObjects.Add(obj);
        availableObjects.Enqueue(obj);
        return obj;
    }
    
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = availableObjects.Count > 0? availableObjects.Dequeue() : CreateNewObject();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }
    
    public void Return(GameObject obj)
    {        
        obj.SetActive(false);
        obj.transform.parent = transform;
        availableObjects.Enqueue(obj);
    }

    public PoolName GetPoolName() => poolName;
}
