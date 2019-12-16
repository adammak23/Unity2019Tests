using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class koparaController : MonoBehaviour
{
    // otwarcie-zamknięcie łychy
    public float angularlimit = 0;
    public bool Closed;
    public bool IsGrounded;

    public GameObject DiggingObject;
    public FakeLoad fakeLoad;
    public BoolEvent Dig;
    public BoolEvent Trim;
    public FloatEvent OnAngularLimitChange;
    Vector3 lastPosition;
    Vector3 Speed;

    bool CanDig()
    {
        return Closed && IsMovingUp() && IsGrounded && fakeLoad.debugLoad <= fakeLoad.MaxLoad;
    }
    bool IsMovingUp()
    {
        return Speed.y < 0;
    }
    void CalculateSpeed()
    {
        if (lastPosition != transform.position)
        {
            Speed = lastPosition - transform.position;
            lastPosition = transform.position;
        }
        else Speed = Vector3.zero;
    }
    void MoveJaws()
    {
        if (angularlimit < 0) angularlimit = 0;
        if (angularlimit > 1) angularlimit = 1;

        if (angularlimit == 0)
        {
            Closed = true;
        }
        else
        {
            Closed = false;
        }
        OnAngularLimitChange.Invoke(angularlimit);
    }

    // Trymowanie chałdy - rozgarnianie bokami przechylając łychę
    bool CanTrim()
    {
        //   Potentially more restrictions:
        // - allow trimming only when grabber is moving in X or Z directions
        // - when fakeLoad.debugLoad == 0
        return !IsGrounded;
    }

    // TODO: change update to be coroutine
    void Update()
    {
        CalculateSpeed();

        Dig.Invoke(CanDig());

        Trim.Invoke(CanTrim());

        MoveJaws();
    }

    public void IsGroundedCheck(bool detected)
    {
        IsGrounded = detected;
    }
}
