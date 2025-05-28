using UnityEngine;

public class FaceMassageDetector : MonoBehaviour
{
    public Collider targetBoundingBox;
    public Transform fingerTipTransform;
    public Vector3 requiredDirection = Vector3.up;
    public float directionThreshold = 0.7f; // Cosine similarity threshold

    public float checkInterval = 0.1f;
    private Vector3 previousPosition;
    private float timer = 0f;

    private bool active = false;

    public bool IsActive() => active;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;

            if (targetBoundingBox.bounds.Contains(fingerTipTransform.position))
            {
                Vector3 movementDir = (fingerTipTransform.position - previousPosition).normalized;
                float dot = Vector3.Dot(movementDir, requiredDirection);

                if (dot > directionThreshold)
                {
                    active = true;
                }
                else
                {
                    active = false;
                }
            }
            else
            {
                active = false;
            }

            previousPosition = fingerTipTransform.position;
        }
    }
}
