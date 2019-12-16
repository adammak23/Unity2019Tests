using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundParticle : MonoBehaviour
{
    public bool DestroyOnGroundInteraction = false;
    public bool TimeDisableOnGroundCollision = false;
    public bool DestroyAfterLifetime = false;
    public float Lifetime;
    bool Active = true;
    public float UpdateInterval = 0.1f;
    public UnityEvent OnGroundInteraction;

    private void OnEnable()
    {
        if (DestroyAfterLifetime)
            StartCoroutine(DestroyAfter(Lifetime));
    }
    IEnumerator DestroyAfter(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
    IEnumerator DeactivateForTime(float blocktime)
    {
        Active = false;
        yield return new WaitForSeconds(blocktime);
        Active = true;
    }

    public void Instantiate(Transform position)
    {
        Instantiate(position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!Active) return;

        RaycastHit hit;
        if (collision.collider.Raycast(new Ray(transform.position, -collision.GetContact(0).normal), out hit, 1))
        {

            if (DestroyOnGroundInteraction)
            {
                Destroy(gameObject);
            }
            if (TimeDisableOnGroundCollision)
            {
                StartCoroutine(DeactivateForTime(UpdateInterval));
            }
            OnGroundInteraction.Invoke();
        }
    }


    public void Activate(float state)
    {
        if (state > 0) Active = true;
        else Active = false;
    }
}
