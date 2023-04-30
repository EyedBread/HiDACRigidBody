using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float pushingPower;
    
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public float GetPushingPower()
    {
        return pushingPower;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PushableObject otherPushable = collision.collider.GetComponent<PushableObject>();
        
        if (otherPushable != null)
        {
            float otherPushingPower = otherPushable.GetPushingPower();
            
            if (pushingPower < otherPushingPower)
            {
                Vector3 forceDirection = (transform.position - collision.transform.position).normalized;
                float forceMultiplier = otherPushingPower - pushingPower;
                float forceAmount = forceMultiplier * 50f; // Adjust this value to achieve desired pushing force
                
                rb.AddForce(forceDirection * forceAmount);
            }
        }
    }
}