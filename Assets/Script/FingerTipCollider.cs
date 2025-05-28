using UnityEngine;

public class FingerTipCollider : MonoBehaviour
{
    void Start()
    {
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = 0.01f; // Adjust size
        sc.isTrigger = true;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Don't let physics move it
        rb.useGravity = false; // Don't let gravity affect it
    }
}