using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundVariation : MonoBehaviour
{
    [System.Serializable]
    struct StructureGroup
    {
        public PoolName poolName;
        public int minStructures;
        public Transform[] parents;
    }

    [SerializeField] PuzzleZoneLinker activePuzzleLinker;
    [SerializeField] PoolName poolName;
    [SerializeField] StructureGroup[] structureGroups;

    List<GameObject> niches, nicheBlocks, tombs, mausoleums;

    public void Initialize()
    {
        niches = new List<GameObject>();
        nicheBlocks = new List<GameObject>();
        tombs = new List<GameObject>();
        mausoleums = new List<GameObject>();

        List<Transform> allItemPositions = new List<Transform>();
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
                    allItemPositions.AddRange(structure.GetComponent<Niche>().ItemPositions);
                    break;
                    
                    case PoolName.NicheBlocks:
                    structure.GetComponent<NicheBlock>().Initialize(parent.GetComponent<NicheBlockGeneration>().GenData);
                    nicheBlocks.Add(structure); 
                    allItemPositions.AddRange(structure.GetComponent<NicheBlock>().ItemPositions);
                    break;
                    
                    case PoolName.Tombs: 
                    tombs.Add(structure);
                    allItemPositions.AddRange(structure.GetComponent<TombProGen>().ItemPositions);
                    break;
                    
                    case PoolName.Mausoleums: 
                    mausoleums.Add(structure);
                    allItemPositions.AddRange(structure.GetComponent<Mausoleum>().ItemPositions);
                    break;
                }
            }
        }
        
        activePuzzleLinker.Spawn(allItemPositions);
    }

    public void Despawn()
    {
        DespawnWithPoolName(niches, PoolName.Niches);
        DespawnWithPoolName(nicheBlocks, PoolName.NicheBlocks);
        DespawnWithPoolName(tombs, PoolName.Tombs);
        DespawnWithPoolName(mausoleums, PoolName.Mausoleums);
        PoolManager.Instance.Despawn(poolName, gameObject);
    }

    void DespawnWithPoolName(List<GameObject> objs, PoolName poolName)
    {
        foreach (GameObject obj in objs)
            PoolManager.Instance.Despawn(poolName, obj);
    }
}
