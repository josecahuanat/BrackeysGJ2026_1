using UnityEngine;

public class Mausoleum : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] Transform[] itemPositions;

    public Transform[] ItemPositions => itemPositions;
}
