using System.Collections.Generic;
using System.Linq;
using Fruits;
using UnityEngine;
using UnityEngine.XR;

public class Cut : MonoBehaviour
{
    private bool IsTriggerHeld()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        var right = allDevices.FirstOrDefault(d => d.name.Contains("Right"));
        if (right.name != null)
        {
            right.TryGetFeatureValue(CommonUsages.triggerButton, out var triggered);
            if (triggered)
            {
                return true;
            }
        }

        return false;
    }

    private bool triggerRegistered;
    private void Update()
    {
        if (IsTriggerHeld() && triggerRegistered == false)
        {
            CutFruits();
            triggerRegistered = true;
        }

        if (IsTriggerHeld() == false)
        {
            triggerRegistered = false;
        }
    }

    private void CutFruits()
    {
        foreach (var fruit in FindObjectsOfType<Fruit>())
        {
            fruit.Cut(transform.position, transform.up);
        }
    }
}
