using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NicheBlock : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] List<Transform> itemPositions;

    public Transform[] ItemPositions => itemPositions.ToArray();

    [Header("Generation Settings")]
    [SerializeField] GameObject nichePrefab;
    [SerializeField] Transform side1, side2;

    GameObject[] niches;

    public void Initialize(NicheBlockGenerationData genData)
    {
        GenerateSideNiches(side1, genData);
        GenerateSideNiches(side2, genData);
    }

    void GenerateSideNiches(Transform side, NicheBlockGenerationData genData)
    {
        List<Vector3> nichePositionsList = genData.nichePositions.ToList();
        int nichesNumber = Random.Range(genData.minNiches, genData.maxNiches);
        niches = new GameObject[nichesNumber];
        for (int i = 0 ; i < niches.Length ; i++)
        {
            int randomNichePosition = Random.Range(0, nichePositionsList.Count);
            niches[i] = PoolManager.Instance.Spawn(PoolName.Niches, side, nichePositionsList[randomNichePosition], Quaternion.identity);
            niches[i].transform.localScale = new Vector3(genData.nicheWidth, genData.nicheHeight, 1f);
            if (nichePositionsList[randomNichePosition].y != genData.nichePositionHighestY)
                itemPositions.AddRange(niches[i].GetComponent<Niche>().ItemPositions);
            nichePositionsList.RemoveAt(randomNichePosition);
        }
    }
}
