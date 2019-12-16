using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Events;
using System;

public class ManualMotor : Motor
{
    #region Variables
    [Header("Initialisation")]
    public Vector3 InitialLocalPosition;
    public Vector3 InitialLocalEuler;
    public Vector3 InitialGlobalPosition;
    public Vector3 InitialGlobalEuler;

    public Vector3 MinLocalEuler;
    public Vector3 MinGlobalEuler;
    public Vector3 MaxLocalEuler;
    public Vector3 MaxGlobalEuler;
    public bool SetOnAwake = false;
    public bool UpdateOnAwake = false;

    [Header("Axis constrains (one axis allowed)")]
    //public MovementAxis RotationAxis;
    //public MovementAxis MovementAxis;
    public bool UpdateAngularSpeed = false;

    [Header("Movement Options")]
    public bool Global = false;
    public bool Local = true;
    public bool Physical = false;

    [Header("Numerical  constrains")]
    public bool NumericConstraints = false;
    public Vector3 BoundaryOfMovementMin;
    public Vector3 BoundaryOfMovementMax;
    float MinMaxPositionLerper;
    float lastLerpFactor = float.NegativeInfinity;

    [Header("Collider  constrains")]
    public bool ColliderConstraints = false;

    [Header("Current state")]
    public bool IsChangingState = false;
    public bool IsInitialState = true;

    public float GoToFactor = 0;
    public float RotateToFactor = 0;
    public float AngularSpeed = 0;

    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        if (UpdateOnAwake)
        {
            InitialLocalPosition = transform.localPosition;
            InitialGlobalPosition = transform.position;
            InitialGlobalEuler = transform.eulerAngles;
            InitialLocalEuler = transform.localEulerAngles;
        }
        if (SetOnAwake)
        {
            transform.localPosition = InitialLocalPosition;
            transform.position = InitialGlobalPosition;
            transform.eulerAngles = InitialGlobalEuler;
            transform.localEulerAngles = InitialLocalEuler;
        }
    }

    private void Update()
    {
        if (Physical)
        {
            Speed = rb.velocity.magnitude;
        }
        if (UpdateAngularSpeed)
        {
            AngularSpeed = rb.angularVelocity.magnitude;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (ColliderConstraints)
        {
            //TO DO, obsługa OnTriggerEnter
        }
    }
    #endregion

    #region Movement
    public void GoToLocal(Vector3 target)
    {
        if (Locked || !Active) return;

        if (!IsChangingState)
            StartCoroutine(GoTo(target));
    }

    public void GoToGlobal(Vector3 target)
    {

    }

    public override void Move(float powerFactor)
    {
        if (Locked || !Active) return;
        Vector3 TransformAxis = Vector3.zero;
        //TransformAxis[(int)MovementAxis] = 1;

        if (Physical)
        {
            if (Mathf.Abs(Speed) < MaxSpeed)
            {
                if (powerFactor > 0)
                {
                    rb.AddForce(TransformAxis * powerFactor * MaxForce * rb.mass);
                }
                else if (powerFactor < 0)
                {
                    rb.AddForce(TransformAxis * powerFactor * MaxReverseForce * rb.mass);
                }
            }
            return;
        }

        if (NumericConstraints)
        {
            if (powerFactor == 0) return;
            MinMaxPositionLerper += powerFactor * MaxSpeed * MaxSpeed;
            if (MinMaxPositionLerper > 1) MinMaxPositionLerper = 1;
            if (MinMaxPositionLerper < -1) MinMaxPositionLerper = -1;

            if (MinMaxPositionLerper > 0)
            {
                if (Local) transform.localPosition = Vector3.Lerp(InitialLocalPosition, BoundaryOfMovementMax, MinMaxPositionLerper);
                if (Global) transform.position = Vector3.Lerp(InitialGlobalPosition, BoundaryOfMovementMax, MinMaxPositionLerper);
            }
            if (MinMaxPositionLerper < 0)
            {
                if (Local) transform.localPosition = Vector3.Lerp(InitialLocalPosition, BoundaryOfMovementMin, -MinMaxPositionLerper);
                if (Global) transform.position = Vector3.Lerp(InitialGlobalPosition, BoundaryOfMovementMin, -MinMaxPositionLerper);
            }
        }
    }

    public void MoveMinMax(float LerpFactor)
    {
        if (!Active) return;
        if (Locked)
        {
            if (LockType == LockType.both) return;
            if (LockType == LockType.positive && lastLerpFactor > LerpFactor) return;
            if (LockType == LockType.negative && lastLerpFactor < LerpFactor) return;
        }

        if (NumericConstraints)
        {
            if (Local) transform.localPosition = Vector3.Lerp(InitialLocalPosition, InitialLocalPosition+BoundaryOfMovementMax, LerpFactor);
            if (Global) transform.position = Vector3.Lerp(InitialGlobalPosition, InitialGlobalPosition+BoundaryOfMovementMax, LerpFactor);
        }
        lastLerpFactor = LerpFactor;
    }

    public void RotateMinMax(float LerpFactor)
    {
        if (Locked || !Active) return;

        if (NumericConstraints)
        {
            if (Local) transform.localEulerAngles = Vector3.Lerp(InitialLocalEuler, InitialLocalEuler + BoundaryOfMovementMax, LerpFactor);
            if (Global) transform.eulerAngles = Vector3.Lerp(InitialGlobalEuler, InitialGlobalEuler + BoundaryOfMovementMax, LerpFactor);
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        IsChangingState = false;
    }

    #region Coroutines
    IEnumerator GoTo(Vector3 target)
    {
        IsChangingState = true;
        float t = 0;
        Vector3 startPos = transform.localPosition;
        while (t <= 1)
        {
            var lerper = t * t * t * (t * (6f * t - 15f) + 10f);
            GoToFactor = lerper;
            transform.localPosition = Vector3.Lerp(startPos, target, lerper);
            t += Time.deltaTime / (Vector3.Distance(startPos, target) / MaxSpeed);
            yield return new WaitForEndOfFrame();
            if (MinMaxPositionLerper != 0) MinMaxPositionLerper = Mathf.Lerp(MinMaxPositionLerper, 0, lerper);
        }
        transform.localPosition = target;
        GoToFactor = 1;

        IsChangingState = false;
        if (transform.localPosition == InitialLocalPosition)
            IsInitialState = true;
        else
            IsInitialState = false;
    }
    #endregion
    #endregion

    #region Rotation
    public override void Rotate(float rotationFactor)
    {
        if (Locked || !Active) return;

        Vector3 AllowedRotationAxis = Vector3.zero;
        //AllowedRotationAxis[(int)RotationAxis] = 1;

        if (Global)
        {
            //transform.eulerAngles = Vector3.Lerp(MinGlobalEuler, MaxGlobalEuler, (rotationFactor + 1) / 2);
        }
        else if (Local)
        {
            if (transform.localEulerAngles.x <= MinLocalEuler.x && transform.localEulerAngles.y <= MinLocalEuler.y && transform.localEulerAngles.z <= MinLocalEuler.z)
            {
                if (rotationFactor < 0) return;
            }
            if (transform.localEulerAngles.x >= MaxLocalEuler.x && transform.localEulerAngles.y >= MaxLocalEuler.y && transform.localEulerAngles.z >= MaxLocalEuler.z)
            {
                if (rotationFactor > 0) return;
            }

            transform.Rotate(MaxRotationSpeed * rotationFactor);
        }
        else if (Physical)
        {
            // MaxRotationSpeed = degrees/s
            // degrees/180 = radians
            rb.AddTorque(Vector3.Scale(AllowedRotationAxis, MaxRotationSpeed) * rotationFactor / 180 * Mathf.PI * rb.mass);
        }

    }

    public void RotateFromToLocal(Vector3 from, Vector3 to)
    {
        if (Locked) return;
        if (!IsChangingState)
        {
            StartCoroutine(RotateFromTo(from, to));
        }
    }

    public void RotateToLocal(Vector3 to)
    {
        if (Locked) return;
        if (!IsChangingState)
        {
            StartCoroutine(RotateTo(to));
        }
    }

    #region Coroutines
    IEnumerator RotateFromTo(Vector3 startRot, Vector3 targetRot)
    {
        IsChangingState = true;
        float t = 0;
        float angleBetween = Quaternion.Angle(Quaternion.Euler(startRot), Quaternion.Euler(targetRot));
        while (t < 1)
        {
            var lerper = t * t * t * (t * (6f * t - 15f) + 10f);
            if (IsInitialState) RotateToFactor = lerper;
            else RotateToFactor = Mathf.Abs(1 - lerper);
            transform.localEulerAngles = new Vector3(Mathf.Lerp(startRot.x, targetRot.x, lerper), Mathf.Lerp(startRot.y, targetRot.y, lerper), Mathf.Lerp(startRot.z, targetRot.z, lerper));
            //t += Time.deltaTime / (angleBetween / MaxRotationSpeed[(int)RotationAxis]);
            yield return new WaitForEndOfFrame();
        }
        IsChangingState = false;
        if (targetRot == MaxLocalEuler) IsInitialState = false;
        if (targetRot == MinLocalEuler) IsInitialState = true;
    }
    IEnumerator RotateTo(Vector3 targetRot)
    {
        IsChangingState = true;
        float t = 0;
        Vector3 startRot = transform.localEulerAngles;
        float angleBetween = Quaternion.Angle(Quaternion.Euler(startRot), Quaternion.Euler(targetRot));

        while (t < 1)
        {
            var lerper = t * t * t * (t * (6f * t - 15f) + 10f);
            if (IsInitialState) RotateToFactor = lerper;
            else RotateToFactor = Mathf.Abs(1 - lerper);
            transform.localEulerAngles = new Vector3(Mathf.Lerp(startRot.x, targetRot.x, lerper), Mathf.Lerp(startRot.y, targetRot.y, lerper), Mathf.Lerp(startRot.z, targetRot.z, lerper));
            //t += Time.deltaTime / (angleBetween / MaxRotationSpeed[(int)RotationAxis]);
            yield return new WaitForEndOfFrame();
        }
        transform.localEulerAngles = targetRot;

        IsChangingState = false;

        if (targetRot == MaxLocalEuler) IsInitialState = false;
        if (targetRot == MinLocalEuler) IsInitialState = true;
    }
    #endregion
    #endregion

}