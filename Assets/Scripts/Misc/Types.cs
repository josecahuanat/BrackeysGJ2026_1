using UnityEngine;

public enum PoolName
{
    None = -1, Niches = 0, NicheBlocks = 1, Tombs = 2, Mausoleums = 3, Grounds = 4, 
    
    //Ground Variation Names
    GroundVariationXBlock = 5,

    //Interactable Item Names
    Lever = 6,
}

[System.Serializable]
public class NicheBlockGenerationData
{
    [Header("Block Info")]
    public float bigBlockZSize;
    public int columns, rows;

    [Header("Niche Info")]
    public float nicheWidth;
    public float nicheHeight;
    public float nicheWidthSpacing;
    public float nicheHeightSpacing;
    public float nicheZSize;

    [Header("Initialization Data")]
    public Vector3[] nichePositions;
    public float bigBlockWidth, bigBlockHeight;
    public int minNiches, maxNiches;
}