using UnityEngine;

public class TombProGen : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] Transform[] itemPositions;

    public Transform[] ItemPositions => itemPositions;
}
