using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NicheBlock : MonoBehaviour
{
    [SerializeField] GameObject nichePrefab;
    [SerializeField] Transform bigBlock, side1, side2;

    GameObject[] niches;

    public void Initialize(NicheBlockGenerationData genData)
    {
        GenerateSideNiches(side1, genData);
        GenerateSideNiches(side2, genData);
    }

    void GenerateSideNiches(Transform side, NicheBlockGenerationData genData)
    {
        bigBlock.localScale = new Vector3(genData.bigBlockWidth, genData.bigBlockHeight, genData.bigBlockZSize);
        side1.localPosition = new Vector3(0f, 0f, -((genData.bigBlockZSize / 2f) - genData.nicheZSize / 4f));
        side2.localPosition = new Vector3(0f, 0f, (genData.bigBlockZSize / 2f) - genData.nicheZSize / 4f);

        List<Vector3> nichePositionsList = genData.nichePositions.ToList();
        int nichesNumber = Random.Range(genData.minNiches, genData.maxNiches);
        niches = new GameObject[nichesNumber];
        for (int i = 0 ; i < niches.Length ; i++)
        {
            int randomNichePosition = Random.Range(0, nichePositionsList.Count);
            niches[i] = PoolManager.Instance.Spawn(PoolName.Niches, side, nichePositionsList[randomNichePosition], Quaternion.identity);
            niches[i].transform.localScale = new Vector3(genData.nicheWidth, genData.nicheHeight, genData.nicheZSize);
            nichePositionsList.RemoveAt(randomNichePosition);
        }
    }
}
