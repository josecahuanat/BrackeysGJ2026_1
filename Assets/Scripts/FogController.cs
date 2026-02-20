using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class FogController : MonoBehaviour
{
    public Material fullScreenFogMaterial;
    public float duration = 10f;
    private float elapsed = 0f;
    public bool isActive = false;

    void Update()
    {

        if (!isActive) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        fullScreenFogMaterial.SetFloat("_FogSize", t);
    }

    public void Activate() { elapsed = 0f; isActive = true; }
}
