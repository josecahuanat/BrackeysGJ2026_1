using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] bool showDebugs;

    [Header("Pool Settings")]
    [SerializeField] PoolName poolName;
    [SerializeField] GameObject prefab;
    [SerializeField] int initialSize;
    
    Queue<GameObject> availableObjects = new Queue<GameObject>();
    List<GameObject> allObjects = new List<GameObject>();
    
    void Awake()
    {        
        for (int i = 0; i < initialSize; i++)
            CreateNewObject();
    }
    
    GameObject CreateNewObject()
    {   
        if (showDebugs)
            Debug.Log("CreateNewObject");

        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        allObjects.Add(obj);
        availableObjects.Enqueue(obj);
        return obj;
    }
    
    public GameObject Get(Transform parent, Vector3 position, Quaternion rotation)
    {
        if (showDebugs)
            Debug.Log(availableObjects.Count);

        if (availableObjects.Count == 0)
            CreateNewObject();

        GameObject obj = availableObjects.Dequeue();
        obj.SetActive(true);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = position;
        obj.transform.localRotation = rotation;
        return obj;
    }
    
    public void Return(GameObject obj)
    {
        if (obj == null)
            return;
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        availableObjects.Enqueue(obj);
    }

    public PoolName GetPoolName() => poolName;
}
