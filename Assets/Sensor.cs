using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sensor : MonoBehaviour
{
    public Collider Collider;
    public ObjectEvent OnSensorChange;
    public bool IgnoreCargoFields = false;
    [Tooltip("Checks for state in almost every update - performance heavy!")]
    public bool OnStay = true;

    [Tooltip("Checks for state only on Enter - performance optimized")]
    public bool OnEnter = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!OnEnter) return;

        if (other != Collider)
        {
            Collider = other;
            OnSensorChange.Invoke(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!OnStay) return;

        if (other != Collider)
        {
            Collider = other;
            OnSensorChange.Invoke(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == Collider)
        {
            Collider = null;
            OnSensorChange.Invoke(null);
        }
    }
}
