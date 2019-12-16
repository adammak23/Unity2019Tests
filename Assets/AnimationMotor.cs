using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMotor : Motor
{
    [SerializeField] private float maxSpeedTime;
    [SerializeField] private float breakSpeedFactor = 1;
    [SerializeField] private float maxSpeedWithLoad;
    [SerializeField] private float maxSpeedTimeWithLoad;
    [Range(-1, 1)] [SerializeField] private float accelDifferenceFactor;

    [SerializeField] private Animator animator;
    //[SerializeField] private CargoManager cargoManager;
    [SerializeField] private float smoothEndTreshhold = 0.02f;
    [SerializeField] private float animationState;

    //Faster setting up speed property with speedId
    private static readonly int speedId = Animator.StringToHash("speed");

    private float destSpeed,
        lastSpeed,
        tempFactor,
        currentLerp,
        tempRotation,
        currMaxSpeed,
        currMaxSpeedTime,
        breakMultiplier;

    public override void Move(float powerFactor)
    {
        //if (cargoManager)
        //{
        //    //Adjusting max speed and acceleration depending on current load
        //    currMaxSpeed = Mathf.Lerp(initMaxSpeed, maxSpeedWithLoad, cargoManager.CurrentLoad / cargoManager.MaxLoad);
        //    currMaxSpeedTime = Mathf.Lerp(maxSpeedTime, maxSpeedTimeWithLoad,
        //        cargoManager.CurrentLoad / cargoManager.MaxLoad);
        //}
        //else
        //{
        //    currMaxSpeed = initMaxSpeed;
        //    currMaxSpeedTime = maxSpeedTime;
        //}

        animationState = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (EmergencyBrake) MaxSpeed = 0;
        if (powerFactor < 0 && animationState < smoothEndTreshhold) currMaxSpeed = 0;
        if (powerFactor >= 0 && animationState > (1 - smoothEndTreshhold)) currMaxSpeed = 0;

        destSpeed = MaxSpeed * powerFactor;

        // Set move to 0 speed if no input 
        if (destSpeed == 0 && Mathf.Abs(lastSpeed) < MoveThreshold)
        {
            lastSpeed = 0;
            Speed = 0;
            animator.SetFloat(speedId, 0);
            currentLerp = currMaxSpeedTime / 2;
            return;
        }

        // Am I accellerating downwards or upwards? (to accomodate accelDifferenceFactor)
        if (lastSpeed >= destSpeed)
        {
            // Accelerating Down
            breakMultiplier = (Speed > 0) ? breakSpeedFactor : 1; // Breaking while going Down
            currentLerp -= (1 + accelDifferenceFactor) * Time.deltaTime * 0.5f * breakMultiplier;
        }
        else
        {
            // Accelerating Up
            breakMultiplier = (Speed < 0) ? breakSpeedFactor : 1; // Breaking while going Up
            currentLerp += (1 - accelDifferenceFactor) * Time.deltaTime * 0.5f * breakMultiplier;
        }

        currentLerp = Mathf.Clamp(currentLerp, 0, currMaxSpeedTime);

        Speed = Mathf.Lerp(-currMaxSpeed, currMaxSpeed, Mathf.SmoothStep(0.0f, 1.0f, currentLerp / currMaxSpeedTime));

        animator.SetFloat(speedId, Speed);

        lastSpeed = Speed;

        IsMoving = (Mathf.Abs(Speed) > MoveThreshold);
        //if (IsMoving) OnSpeedChange.Invoke(Mathf.Abs(Speed / initMaxSpeed));

        // Speed += powerFactor * Time.deltaTime;
        // Speed = Mathf.Clamp(Speed, -MaxSpeed, MaxSpeed);
        // animator.SetFloat("speed", Speed * animationSpeedFactor);
    }

    public override void Rotate(float rotationFactor)
    {
        throw new System.NotImplementedException();
    }
}
