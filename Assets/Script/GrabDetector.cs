using UnityEngine;

public class GrabDetector : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private bool _isGrabbed = false;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
    }
    
    // Call this when you detect a grab gesture
    public void Grab(Transform handAnchor)
    {
        _isGrabbed = true;
        _rigidbody.isKinematic = true; // Makes the object follow the hand smoothly
        transform.SetParent(handAnchor);
    }
    
    // Call this when you detect a release gesture
    public void Release()
    {
        _isGrabbed = false;
        _rigidbody.isKinematic = false;
        transform.SetParent(null);
    }
}