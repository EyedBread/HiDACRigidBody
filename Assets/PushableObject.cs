using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float pushingPower;
    private bool isCollidingWithWall = false;
    
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public float GetPushingPower()
    {
        return pushingPower;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if the agent is colliding with a wall
        if (collision.collider.CompareTag("Wall"))
        {
            isCollidingWithWall = true;
        }

        // Handle collisions with other pushable objects
        PushableObject otherPushable = collision.collider.GetComponent<PushableObject>();
        if (otherPushable != null)
        {
            float otherPushingPower = otherPushable.GetPushingPower();

            if (pushingPower < otherPushingPower)
            {
                Vector3 forceDirection = (transform.position - collision.transform.position).normalized;
                float forceMultiplier = otherPushingPower - pushingPower;
                float forceAmount = forceMultiplier * 50f; // Adjust this value to achieve desired pushing force
                if (isCollidingWithWall)
                    forceAmount *= 0.3f; //Lambda


                rb.AddForce(forceDirection * forceAmount);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            isCollidingWithWall = false;
        }
    }

    public bool IsCollidingWithWall()
    {
        return isCollidingWithWall;
    }
}