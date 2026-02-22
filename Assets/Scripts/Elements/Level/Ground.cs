using UnityEngine;

public class Ground : MonoBehaviour
{
    public Transform variationParent;
    public PoolName[] variations;
    
    GroundVariation variation;
    PuzzleZoneLinker pzl;

    public PuzzleZoneLinker PZL => pzl;
    public int PuzzleDifficulty => pzl.PuzzleData.difficulty;


    public void Initialize()
    {
        int variationIndex = Random.Range(0, variations.Length);
        GameObject variationGB = PoolManager.Instance.Spawn(variations[variationIndex], variationParent, Vector3.zero, Quaternion.identity);
        variation = variationGB.GetComponent<GroundVariation>();
        pzl = variationGB.GetComponent<PuzzleZoneLinker>();
        variation.Initialize();
    }

    public void Despawn()
    {
        variation.Despawn();
        PoolManager.Instance.Despawn(PoolName.Grounds, gameObject);
    }
}
