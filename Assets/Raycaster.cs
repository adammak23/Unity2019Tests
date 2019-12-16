using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Raycaster : MonoBehaviour
{
    public BoolEvent OnObjectDetected;
    public bool Active = false;
    public float RaycastDistance = 1;
    public LayerMask RaycastLayer;
    public float RaycastInterval = 0.1f;

    private void OnEnable()
    {
        StartCoroutine(Raycast());
    }

    IEnumerator Raycast()
    {
        while (true)
        {
            yield return new WaitForSeconds(RaycastInterval);
            if (!Active) continue;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit, RaycastDistance, RaycastLayer))
            {
                OnObjectDetected.Invoke(true);
            }
            else
            {
                OnObjectDetected.Invoke(false);
            }
        }
    }
}
