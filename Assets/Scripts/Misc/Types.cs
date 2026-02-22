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
    public int columns;
    public int rows;

    [Header("Niche Info")]
    public float nicheWidth;
    public float nicheHeight;

    [Header("Initialization Data")]
    public Vector3[] nichePositions;
    public float nichePositionHighestY;
    public int minNiches, maxNiches;
}