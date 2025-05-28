using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Leap;

[System.Serializable]
public class MassageStep
{
    [Header("Step Configuration")]
    public string instruction;
    public string zoneName;
    public Vector3 requiredDirection;
    public float requiredDuration = 3f;
    public float toleranceAngle = 30f;
    
    [Header("Visual Feedback")]
    public GameObject visualIndicator;
    public Color zoneHighlightColor = new Color(0, 1, 0, 0.3f);
    public ParticleSystem successParticles;
}

public class MassageTutorialManager : MonoBehaviour
{
    [Header("Leap Motion References")]
    public LeapProvider leapProvider;
    public float pinchThreshold = 0.7f;
    
    [Header("Tutorial Configuration")]
    public List<MassageStep> steps;
    public bool loopTutorial = true;
    
    [Header("UI References")]
    public Text instructionText;
    public Slider progressSlider;
    public GameObject completionPanel;
    public Text completionText;
    
    // Current state
    private int currentStepIndex = 0;
    private MassageStep currentStep;
    private float currentStepProgress = 0f;
    private bool isInZone = false;
    private Vector3 lastHandPosition;
    private GameObject currentZone;
    private Material originalZoneMaterial;
    private Material highlightMaterial;
    
    // Audio
    private AudioSource audioSource;
    public AudioClip stepStartSound;
    public AudioClip stepCompleteSound;
    public AudioClip tutorialCompleteSound;
    
    void Start()
    {
        // Set up Leap Motion provider if not assigned
        if (leapProvider == null)
        {
            leapProvider = FindObjectOfType<LeapServiceProvider>();
            if (leapProvider == null)
            {
                Debug.LogError("No LeapProvider found in scene!");
                enabled = false;
                return;
            }
        }
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create highlight material
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.SetFloat("_Mode", 2); // Fade mode
        highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
        highlightMaterial.renderQueue = 3000;
        
        // Initialize tutorial
        InitializeSteps();
        StartStep(0);
    }
    
    void InitializeSteps()
    {
        // Validate steps
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("No steps configured for the tutorial!");
            enabled = false;
            return;
        }
        
        // Hide all visual indicators initially
        foreach (var step in steps)
        {
            if (step.visualIndicator != null)
            {
                step.visualIndicator.SetActive(false);
            }
            
            if (step.successParticles != null)
            {
                step.successParticles.Stop();
            }
        }
        
        // Hide completion UI
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (currentStep == null || leapProvider == null) return;
        
        Frame frame = leapProvider.CurrentFrame;
        if (frame.Hands.Count == 0) return;
        
        Hand hand = frame.Hands[0];
        Vector3 handPosition = hand.PalmPosition;
        
        // Check for two finger pose or pinch
        bool correctPose = IsTwoFingerPose(hand) || hand.PinchStrength > pinchThreshold;
        
        // Zone detection logic
        bool inZone = false;
        if (currentZone != null && correctPose)
        {
            Collider zoneCollider = currentZone.GetComponent<Collider>();
            if (zoneCollider != null && zoneCollider.bounds.Contains(handPosition))
            {
                inZone = true;
                
                if (isInZone)
                {
                    // Calculate movement direction and angle
                    Vector3 movementDirection = (handPosition - lastHandPosition).normalized;
                    float angle = Vector3.Angle(movementDirection, currentStep.requiredDirection);
                    
                    // Update progress if moving in correct direction
                    if (angle < currentStep.toleranceAngle)
                    {
                        currentStepProgress += Time.deltaTime;
                        float progress = currentStepProgress / currentStep.requiredDuration;
                        UpdateProgressUI(progress);
                        
                        // Visual feedback based on progress
                        UpdateZoneHighlight(progress);
                    }
                }
            }
        }
        
        // Handle zone entry/exit
        if (inZone != isInZone)
        {
            isInZone = inZone;
            if (isInZone)
            {
                OnZoneEnter();
            }
            else
            {
                OnZoneExit();
            }
        }
        
        lastHandPosition = handPosition;
        
        // Check for step completion
        if (currentStepProgress >= currentStep.requiredDuration)
        {
            CompleteCurrentStep();
        }
    }
    
    bool IsTwoFingerPose(Hand hand)
    {
        int extendedCount = 0;
        foreach (Finger finger in hand.fingers)
        {
            if (finger.IsExtended) extendedCount++;
            if (extendedCount > 2) return false;
        }
        return extendedCount == 2;
    }
    
    void StartStep(int index)
    {
        if (index < 0 || index >= steps.Count)
        {
            Debug.Log("Tutorial steps completed!");
            TutorialComplete();
            return;
        }
        
        // Reset step state
        currentStepIndex = index;
        currentStep = steps[index];
        currentStepProgress = 0f;
        isInZone = false;
        
        // Find the zone GameObject
        currentZone = GameObject.Find(currentStep.zoneName);
        if (currentZone == null)
        {
            Debug.LogError($"Zone '{currentStep.zoneName}' not found in scene!");
            return;
        }
        
        // Save original material
        var zoneRenderer = currentZone.GetComponent<Renderer>();
        if (zoneRenderer != null)
        {
            originalZoneMaterial = zoneRenderer.material;
            
            // Apply highlight material
            highlightMaterial.color = currentStep.zoneHighlightColor;
            zoneRenderer.material = highlightMaterial;
        }
        
        // Show visual indicator
        if (currentStep.visualIndicator != null)
        {
            currentStep.visualIndicator.SetActive(true);
            currentStep.visualIndicator.transform.position = currentZone.transform.position;
            currentStep.visualIndicator.transform.rotation = Quaternion.LookRotation(currentStep.requiredDirection);
        }
        
        // Update UI
        UpdateInstructionUI(currentStep.instruction);
        UpdateProgressUI(0f);
        
        // Play sound
        if (stepStartSound != null)
        {
            audioSource.PlayOneShot(stepStartSound);
        }
        
        Debug.Log($"Started step {index}: {currentStep.instruction}");
    }
    
    void OnZoneEnter()
    {
        Debug.Log("Entered massage zone");
        // You could add haptic feedback here
    }
    
    void OnZoneExit()
    {
        Debug.Log("Exited massage zone");
        // Reset zone visual if needed
    }
    
    void CompleteCurrentStep()
    {
        Debug.Log($"Completed step {currentStepIndex}");
        
        // Play success particles
        if (currentStep.successParticles != null)
        {
            currentStep.successParticles.Play();
        }
        
        // Play sound
        if (stepCompleteSound != null)
        {
            audioSource.PlayOneShot(stepCompleteSound);
        }
        
        // Clean up current step
        if (currentStep.visualIndicator != null)
        {
            currentStep.visualIndicator.SetActive(false);
        }
        
        // Restore original material
        var zoneRenderer = currentZone.GetComponent<Renderer>();
        if (zoneRenderer != null && originalZoneMaterial != null)
        {
            zoneRenderer.material = originalZoneMaterial;
        }
        
        // Move to next step
        StartStep(currentStepIndex + 1);
    }
    
    void TutorialComplete()
    {
        Debug.Log("Tutorial completed!");
        
        // Show completion UI
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
        
        if (completionText != null)
        {
            completionText.text = "Great job! You've completed the skincare routine.";
        }
        
        // Play sound
        if (tutorialCompleteSound != null)
        {
            audioSource.PlayOneShot(tutorialCompleteSound);
        }
        
        // Loop tutorial if enabled
        if (loopTutorial)
        {
            Invoke("RestartTutorial", 5f);
        }
    }
    
    void RestartTutorial()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        StartStep(0);
    }
    
    void UpdateInstructionUI(string instruction)
    {
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
    }
    
    void UpdateProgressUI(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }
    }
    
    void UpdateZoneHighlight(float progress)
    {
        if (currentZone != null)
        {
            var zoneRenderer = currentZone.GetComponent<Renderer>();
            if (zoneRenderer != null)
            {
                Color highlightColor = currentStep.zoneHighlightColor;
                highlightColor.a = Mathf.Lerp(0.3f, 1f, progress);
                highlightMaterial.color = highlightColor;
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up material
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
    }
}