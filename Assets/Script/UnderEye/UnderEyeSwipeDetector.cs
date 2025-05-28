using UnityEngine;
using Leap;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class UnderEyeTouchDetector : MonoBehaviour
{
    [Header("Leap Motion")]
    public LeapServiceProvider leapProvider; // Assign in Inspector

    [Header("Button Settings")]
    public GameObject pressableObject;
    public float activationDistance = 0.02f; // 2cm threshold
    public float cooldownTime = 0.5f;

    [Header("Popup Settings")]
    public Canvas popupCanvas;
    public float popupDuration = 3f;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    private Vector3 _initialPosition;
    private bool _isActive = false;
    private float _lastActivationTime;

    void Start()
    {
        if (leapProvider == null)
            leapProvider = FindObjectOfType<LeapServiceProvider>();

        _initialPosition = pressableObject.transform.localPosition;

        if (popupCanvas != null)
            popupCanvas.enabled = false;
    }

    void Update()
    {
        if (leapProvider == null || Time.time - _lastActivationTime < cooldownTime)
            return;

        Frame frame = leapProvider.CurrentFrame;
        if (frame == null) return;

        bool shouldActivate = CheckFingerProximity(frame);

        if (shouldActivate && !_isActive)
        {
            Activate();
        }
        else if (!shouldActivate && _isActive)
        {
            Deactivate();
        }
    }

    private bool CheckFingerProximity(Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            foreach (Finger finger in hand.fingers)
            {
                // Convert finger position to local space
                Vector3 fingerPos = transform.InverseTransformPoint(finger.TipPosition);
                
                if (Vector3.Distance(fingerPos, _initialPosition) < activationDistance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void Activate()
    {
        _isActive = true;
        _lastActivationTime = Time.time;
        
        if (popupCanvas != null)
        {
            popupCanvas.enabled = true;
            if (popupDuration > 0)
                Invoke(nameof(Deactivate), popupDuration);
        }
        
        OnActivated.Invoke();
        Debug.Log("Under-eye area activated!");
    }

    private void Deactivate()
    {
        _isActive = false;
        
        if (popupCanvas != null)
            popupCanvas.enabled = false;
        
        OnDeactivated.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (pressableObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pressableObject.transform.position, activationDistance);
        }
    }
}