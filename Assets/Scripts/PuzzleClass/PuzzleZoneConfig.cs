using UnityEngine;
using System.Collections.Generic;

public enum DifficultyLevel { Easy, Medium, Hard }

[System.Serializable]
public class DecorationRule
{
    public PoolName poolName;
    //public SpawnTarget spawnIn;
    public int minCount;
    public int maxCount;
}

[CreateAssetMenu(fileName = "PuzzleZoneConfig", menuName = "Puzzle/Zone Config")]
public class PuzzleZoneConfig : ScriptableObject
{
    [Header("Identidad")]
    public string zoneName;
    public DifficultyLevel difficulty;

    [Header("Prefab del puzzle (auto-contenido con PuzzleZoneLinker)")]
    public GameObject puzzleZonePrefab; // ← el prefab con todo dentro
}