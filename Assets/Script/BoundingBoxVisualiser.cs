using UnityEngine;

[ExecuteInEditMode]
public class BoundingBoxVisualizer : MonoBehaviour
{
    public Color boxColor = Color.yellow;

    void OnDrawGizmos()
    {
        Gizmos.color = boxColor;
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
