using UnityEngine;

public class Niche : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] Transform[] itemPositions;

    public Transform[] ItemPositions => itemPositions;
}
