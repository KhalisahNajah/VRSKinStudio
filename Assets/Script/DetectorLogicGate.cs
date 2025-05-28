using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum LogicType
{
    AND,
    OR
}

public class DetectorLogicGate : MonoBehaviour
{
    public LogicType logicType = LogicType.AND;
    public List<MonoBehaviour> detectorScripts; // Scripts that implement IDetector
    public bool invertResult = false;

    private bool gateActive = false;

    public delegate void GateStateChanged(bool active);
    public event GateStateChanged OnGateStateChanged;

    void Update()
    {
        var detectors = detectorScripts.OfType<IDetector>().ToList();
        if (detectors.Count == 0) return;

        bool result = logicType switch
        {
            LogicType.AND => detectors.All(d => d.IsActive),
            LogicType.OR => detectors.Any(d => d.IsActive),
            _ => false
        };

        if (invertResult)
            result = !result;

        if (result != gateActive)
        {
            gateActive = result;
            OnGateStateChanged?.Invoke(gateActive);
        }
    }

    public bool IsGateActive()
    {
        return gateActive;
    }
}

public interface IDetector
{
    bool IsActive { get; }
}
