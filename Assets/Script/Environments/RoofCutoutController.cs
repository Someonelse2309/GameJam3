using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class RoofCutoutController : MonoBehaviour
{
    [SerializeField] private Material roofMaterial;
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private float radius = 2.0f;
    [SerializeField] private float feather = 1.5f;

    void Update()
    {
        if (roofMaterial != null)
        {
            roofMaterial.SetVector("_PlayerPos", transform.position);
            roofMaterial.SetFloat("_Radius", radius);
            roofMaterial.SetFloat("_Feather", feather);
        }
    }
}
