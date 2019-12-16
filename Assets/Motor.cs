using UnityEngine;
using UnityEngine.Events;

public abstract class Motor : MonoBehaviour
{
    [Header("Motor Default Parameters")]
    [Tooltip("m/s")]
    public float MaxSpeed;
    public float initMaxSpeed;

    [SerializeField] public Rigidbody rb;
    [SerializeField] protected Vector3 MaxRotationSpeed;
    [SerializeField] protected float MaxForce;
    [SerializeField] protected float MaxReverseForce;
    [SerializeField] protected float MaxBrakingForce;
    [Tooltip("m/s")]
    [SerializeField] public float Speed;
    [SerializeField] public float Rotation;
    [SerializeField] private float moveDelay;
    [SerializeField] private float rotateDelay;

    [Tooltip("Maximum speed (m/s) not considered as motion")]
    [SerializeField] public float MoveThreshold = 0.001f;
    [Tooltip("Maximum angular speed (m/s) not considered as motion")]
    [SerializeField] public float RotateThreshold = 0.001f;

    /// <summary>
    /// Fires when Locked and Unlocked
    /// </summary>
    public BoolEvent OnLock;
    public UnityEvent OnMove, OnStop;
    public FloatEvent OnSpeedChange;
    public bool EmergencyBrake = false;
    public bool Bypass = false;
    public bool Locked
    {
        get
        {
            return locked;
        }
        set
        {
            if (locked != value)
            {
                locked = value;
                OnLock.Invoke(value);
            }
        }
    }
    public LockType LockType = LockType.both;
    private bool locked = false;
    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
        set
        {
            if (isMoving != value)
            {
                isMoving = value;
                if (isMoving) OnMove.Invoke();
                else OnStop.Invoke();
            }
        }
    }
    private bool isMoving = false;
    public bool Malfunction = false;
    public bool Active = false;

    private float moveDelayTimer = 0;
    private float rotateDelayTimer = 0;

    public void Start()
    {
        initMaxSpeed = MaxSpeed;
    }

    public void Activate(float state)
    {
        if (state == 1) Active = true;
        else if (state == 0) Active = false;
    }

    public void Lock(float state)
    {
        if (Malfunction) { Locked = true; return; }

        if (state == 1) Locked = true;
        else Locked = false;
    }
    public void Lock(bool state)
    {
        if (Malfunction) { Locked = true; return; }

        Locked = state;
    }
    public void Lock(object state)
    {
        if (Malfunction) { Locked = true; return; }

        if (state != null)
        {
            Locked = true;
        }
        else
        {
            Locked = false;
        }
    }
    public void SwitchLock()
    {
        if (!IsMoving) Locked = !Locked;
    }

    public void Stop(object state)
    {
        if (state != null)
        {
            EmergencyBrake = true;
        }
        else
        {
            EmergencyBrake = false;
        }
    }
    public void Stop(bool state)
    {
        EmergencyBrake = state;
    }
    public void Emergency(float state)
    {
        if (state == 1) EmergencyBrake = true;
        else if (state == 0) EmergencyBrake = false;
    }

    public void ChangeMaxSpeed(float multiplier)
    {
        MaxSpeed = initMaxSpeed * multiplier;
    }
    public void UseBypass(float state)
    {
        if (state == 1) Bypass = true;
        else Bypass = false;

    }

    // input delay
    public void TryMove(float value)
    {
        if (!Active) value = 0;
        if (EmergencyBrake && !Bypass) value = 0;

        if (locked)
        {
            if (LockType == LockType.both) value = 0;
            if (LockType == LockType.positive && value > 0) value = 0;
            if (LockType == LockType.negative && value < 0) value = 0;
        }
        if (value == 0)
        {
            Move(value);
            moveDelayTimer = 0;
        }
        else
        {
            if (Speed < MoveThreshold)
            {
                // increment timer
                moveDelayTimer += Time.deltaTime;

                if (moveDelayTimer >= moveDelay)
                {
                    Move(value);
                }
            }
            else
            {
                Move(value);
            }
        }
    }
    public void TryRotate(float value)
    {
        if (!Active) value = 0;
        if (EmergencyBrake && !Bypass) value = 0;

        if (locked)
        {
            if (LockType == LockType.both) value = 0;
            if (LockType == LockType.positive && value > 0) value = 0;
            if (LockType == LockType.negative && value < 0) value = 0;
        }

        if (value == 0)
        {
            Rotate(value);
            rotateDelayTimer = 0;
        }
        else
        {

            if (Rotation < RotateThreshold)
            {
                // increment timer
                rotateDelayTimer += Time.deltaTime;

                if (rotateDelayTimer >= rotateDelay)
                {
                    Rotate(value);
                }
            }
            else
            {
                Rotate(value);
            }
        }
    }

    public abstract void Move(float powerFactor);
    public abstract void Rotate(float rotationFactor);
}