using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Game Data/Puzzle Data")]
public class PuzzleData : ScriptableObject
{
    public PuzzleName typeName;
    public PuzzleDifficulty[] difficulties;
}

[System.Serializable]
public class PuzzleDifficulty
{
    public PuzzleItem[] items;
}

[System.Serializable]
public class PuzzleItem
{
    public PuzzleItemName name;
    public int quantity;
    public int spawnPositions;
}

public enum PuzzleItemName
{
    Button = 0, Plate = 1,Door=2, ItemSocket=3, Key=4, Tomb=5
}

public enum PuzzleName
{
    None = 0, OrderedPlacedItems = 1, PlaceItemsDisorder = 2, PressPlates = 3, LightCandle=4
}