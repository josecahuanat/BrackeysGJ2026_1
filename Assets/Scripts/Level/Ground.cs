using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ground : MonoBehaviour
{
    [System.Serializable]
    struct StructureGroup
    {
        public PoolName poolName;
        public int minStructures;
        public Transform[] parents;
    }

    List<GameObject> niches,
        nicheBlocks,
        tombs,
        mausoleums;

    [SerializeField] StructureGroup[] structureGroups;

    public void Initialize()
    {
        niches = new List<GameObject>();
        nicheBlocks = new List<GameObject>();
        tombs = new List<GameObject>();
        mausoleums = new List<GameObject>();

        foreach(var structureGroup in structureGroups)
        {
            int randomAmount = Random.Range(structureGroup.minStructures, structureGroup.parents.Length);
            List<Transform> structureParents = structureGroup.parents.ToList();

            int structuresToRemove = structureParents.Count - randomAmount;
            for (int i = 0 ; i < structuresToRemove ; i++)
            {
                int randomIndex = Random.Range(0, structureParents.Count);
                structureParents.RemoveAt(randomIndex);
            }

            foreach(var parent in structureParents)
            {
                GameObject structure = PoolManager.Instance.Spawn(structureGroup.poolName, parent, Vector3.zero, Quaternion.identity);
                switch(structureGroup.poolName)
                {
                    case PoolName.Niches: 
                    niches.Add(structure); 
                    break;
                    
                    case PoolName.NicheBlocks:
                    structure.GetComponent<NicheBlock>().Initialize(parent.GetComponent<NicheBlockGeneration>().GenData);
                    nicheBlocks.Add(structure); 
                    break;
                    
                    case PoolName.Tombs: 
                    tombs.Add(structure);
                    break;
                    
                    case PoolName.Mausoleums: 
                    mausoleums.Add(structure);
                    break;
                }
            }
        }
    }

    public void Despawn()
    {
        DespawnWithPoolName(niches, PoolName.Niches);
        DespawnWithPoolName(nicheBlocks, PoolName.NicheBlocks);
        DespawnWithPoolName(tombs, PoolName.Tombs);
        DespawnWithPoolName(mausoleums, PoolName.Mausoleums);
        PoolManager.Instance.Despawn(PoolName.Grounds, gameObject);
    }

    void DespawnWithPoolName(List<GameObject> objs, PoolName poolName)
    {
        foreach (GameObject obj in objs)
            PoolManager.Instance.Despawn(poolName, obj);
    }
}
