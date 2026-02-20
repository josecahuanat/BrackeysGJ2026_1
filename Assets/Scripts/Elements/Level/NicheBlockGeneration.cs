using UnityEngine;

public class NicheBlockGeneration : MonoBehaviour
{
    [SerializeField] NicheBlockGenerationData genData;

    public NicheBlockGenerationData GenData => genData;

    [ContextMenu("Set Niche Positions")]
    void SetNicheGenerationData()
    {
        genData.nichePositions = new Vector3[genData.rows * genData.columns];

        bool evenRows = genData.rows % 2f == 0f;
        int startRows = evenRows? (genData.rows / 2) : (genData.rows-1) / 2;

        bool evenColumns = genData.columns % 2f == 0f;
        int startColumns = evenColumns? (genData.columns / 2) : (genData.columns-1) / 2;

        float highestX = 0, highestY = 0;

        int index = 0;
        float startXPos = evenRows? genData.nicheWidth / 2f : 0f;
        float startXSpacing = evenRows? genData.nicheWidthSpacing / 2f : 0f;
        float startYPos = evenColumns? genData.nicheHeight / 2f : 0f;
        float startYSpacing = evenColumns? genData.nicheHeightSpacing / 2f : 0f;
        for (int i=-startRows ; i<=startRows ; i++)
        {
            if (i==0 && evenRows)
                continue;

            int customI = Mathf.Abs(i);
            if (evenRows)
                customI--;

            float xPos = startXPos + startXSpacing + (genData.nicheWidth * customI) + (genData.nicheWidthSpacing * customI);
            if (i < 0)
                xPos = -xPos;

            for (int j=-startColumns ; j<=startColumns ; j++)
            {
                if (j==0 && evenColumns)
                    continue;
                
                int customJ = Mathf.Abs(j);
                if (evenColumns)
                    customJ--;

                float yPos = startYPos + startYSpacing + (genData.nicheHeight * customJ) + (genData.nicheHeightSpacing * customJ);
                if (j < 0)
                    yPos = -yPos;

                if (xPos > highestX)
                    highestX = xPos;

                if (yPos > highestY)
                    highestY = yPos;


                if (yPos > genData.nichePositionHighestY)
                    genData.nichePositionHighestY = yPos;

                genData.nichePositions[index] = new Vector3(xPos, yPos, 0f);
                index++;
            }
        }

        genData.bigBlockWidth = (highestX + (genData.nicheWidth / 2f) + genData.nicheWidthSpacing) * 2f;
        genData.bigBlockHeight = (highestY + (genData.nicheHeight / 2f) + genData.nicheHeightSpacing) * 2f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
