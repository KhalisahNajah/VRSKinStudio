using UnityEngine;
using System;
using System.Collections.Generic;

public class DetectorLogicGates : MonoBehaviour
{
    public enum GateType { AND, OR }
    public GateType gateType = GateType.AND;

    public bool negateResult = false;
    public List<DetectorElement> detectors = new List<DetectorElement>();

    public Action OnActivate;
    public Action OnDeactivate;

    private bool lastState = false;

    void Update()
    {
        bool result = (gateType == GateType.AND);

        foreach (var detector in detectors)
        {
            if (gateType == GateType.AND)
                result &= detector.IsActive();
            else if (gateType == GateType.OR)
                result |= detector.IsActive();
        }

        if (negateResult)
            result = !result;

        if (result != lastState)
        {
            if (result) OnActivate?.Invoke();
            else OnDeactivate?.Invoke();
            lastState = result;
        }
    }
}

[Serializable]
public class DetectorElement
{
    public MonoBehaviour detectorScript;
    public string methodName = "IsActive";

    public bool IsActive()
    {
        var method = detectorScript.GetType().GetMethod(methodName);
        return (bool)method.Invoke(detectorScript, null);
    }
}
