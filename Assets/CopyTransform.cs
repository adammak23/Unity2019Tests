using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    public Transform CopyFrom;
    public Vector3 PositionOffset;
    public Vector3 EulerOffset;

    public bool UseNormalUpdate = true;
    public bool UseLateUpdate = false;
    public bool UseFixedUpdate = false;

    public bool CopyPosition = false;
    public bool CopyRotation = false;
    public bool CopyEuler = false;

    Vector3 StartingEulerAngles;

    private void Start()
    {
        StartingEulerAngles = transform.eulerAngles;
    }

    void Update()
    {
        if (!UseNormalUpdate) return;

        if (CopyPosition)
        {
            transform.position = CopyFrom.transform.position;
            transform.localPosition += PositionOffset;
        }

        if (CopyRotation)
        {
            transform.rotation = CopyFrom.transform.rotation;
            transform.eulerAngles = StartingEulerAngles + EulerOffset;
        }

        if (CopyEuler)
        {
            transform.eulerAngles = new Vector3(CopyFrom.eulerAngles.x, CopyFrom.eulerAngles.y, CopyFrom.eulerAngles.z) + EulerOffset;
        }
    }

    void LateUpdate()
    {
        if (!UseLateUpdate) return;

        if (CopyPosition)
        {
            transform.position = CopyFrom.transform.position + PositionOffset;
        }

        if (CopyRotation)
        {
            transform.rotation = CopyFrom.transform.rotation;
            transform.eulerAngles = StartingEulerAngles + EulerOffset;
        }

        if (CopyEuler)
        {
            transform.eulerAngles = new Vector3(CopyFrom.eulerAngles.x, CopyFrom.eulerAngles.y, CopyFrom.eulerAngles.z) + EulerOffset;
        }
    }

    void FixedUpdate()
    {
        if (!UseFixedUpdate) return;

        if (CopyPosition)
        {
            transform.position = CopyFrom.transform.position + PositionOffset;
        }

        if (CopyRotation)
        {
            transform.rotation = CopyFrom.transform.rotation;
            transform.eulerAngles = StartingEulerAngles + EulerOffset;
        }

        if (CopyEuler)
        {
            transform.eulerAngles = new Vector3(CopyFrom.eulerAngles.x, CopyFrom.eulerAngles.y, CopyFrom.eulerAngles.z) + EulerOffset;
        }
    }

}
