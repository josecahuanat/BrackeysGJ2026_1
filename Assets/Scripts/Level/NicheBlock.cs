using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NicheBlock : MonoBehaviour
{
    [SerializeField] Transform bigBlock, side1, side2;
    [SerializeField] float bigBlockZSize;
    [SerializeField] int columns, rows;
    [SerializeField] float nicheWidth, nicheHeight, nicheWidthSpacing, nicheHeightSpacing, nicheZSize;
    [SerializeField] Vector3[] nichePositions;
    [SerializeField] GameObject nichePrefab;
    [SerializeField] int minNiches, maxNiches;

    GameObject[] niches;

    void Start()
    {
        GenerateSideNiches(side1);
        GenerateSideNiches(side2);
    }

    void Update()
    {
        
    }

    void GenerateSideNiches(Transform side)
    {
        List<Vector3> nichePositionsList = nichePositions.ToList();
        int nichesNumber = Random.Range(minNiches, maxNiches);
        niches = new GameObject[nichesNumber];
        for (int i = 0 ; i < niches.Length ; i++)
        {
            niches[i] = PoolManager.Instance.Spawn(PoolName.Niches, Vector3.zero, Quaternion.identity);
            niches[i].transform.SetParent(side);
            int randomNichePosition = Random.Range(0, nichePositionsList.Count);
            niches[i].transform.localPosition = nichePositionsList[randomNichePosition];
            nichePositionsList.RemoveAt(randomNichePosition);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Set Niche Positions")]
    void SetNichePositions()
    {
        nichePositions = new Vector3[rows * columns];

        bool evenRows = rows % 2f == 0f;
        int startRows = evenRows? (rows / 2) : (rows-1) / 2;

        bool evenColumns = columns % 2f == 0f;
        int startColumns = evenColumns? (columns / 2) : (columns-1) / 2;

        float highestX = 0, highestY = 0;

        int index = 0;
        float startXPos = evenRows? nicheWidth / 2f : 0f;
        float startXSpacing = evenRows? nicheWidthSpacing / 2f : 0f;
        float startYPos = evenColumns? nicheHeight / 2f : 0f;
        float startYSpacing = evenColumns? nicheHeightSpacing / 2f : 0f;
        for (int i=-startRows ; i<=startRows ; i++)
        {
            if (i==0 && evenRows)
                continue;

            int customI = Mathf.Abs(i);
            if (evenRows)
                customI--;

            float xPos = startXPos + startXSpacing + (nicheWidth * customI) + (nicheWidthSpacing * customI);
            if (i < 0)
                xPos = -xPos;

            for (int j=-startColumns ; j<=startColumns ; j++)
            {
                if (j==0 && evenColumns)
                    continue;
                
                int customJ = Mathf.Abs(j);
                if (evenColumns)
                    customJ--;

                float yPos = startYPos + startYSpacing + (nicheHeight * customJ) + (nicheHeightSpacing * customJ);
                if (j < 0)
                    yPos = -yPos;

                if (xPos > highestX)
                    highestX = xPos;

                if (yPos > highestY)
                    highestY = yPos;

                nichePositions[index] = new Vector3(xPos, yPos, 0f);
                index++;
            }
        }

        float bigBlockWidth = (highestX + (nicheWidth / 2f) + nicheWidthSpacing) * 2f;
        float bigBlockHeight = (highestY + (nicheHeight / 2f) + nicheHeightSpacing) * 2f;
        bigBlock.localScale = new Vector3(bigBlockWidth, bigBlockHeight, bigBlockZSize);
        side2.transform.localPosition = new Vector3(0f, 0f, (bigBlockZSize / 2f) - nicheZSize / 4f);
        side1.transform.localPosition = new Vector3(0f, 0f, -((bigBlockZSize / 2f) - nicheZSize / 4f));

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
