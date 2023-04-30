using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float pushingPower;
    // [SerializeField] private float forceThreshold = 100f; // Set the force threshold as desired

    private float totalCollisionForce = 0f;

    private float agentCollisionForce = 0f;

    private Vector2 agentCollisionVec = Vector2.zero;

    private List<Collision2D> activeCollisions = new List<Collision2D>();

    private int wallCount = 0;

    private Rigidbody rb;

    private Agent agent;
    private bool isCollidingWithWall = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public float GetPushingPower()
    {
        return pushingPower;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(gameObject.name + " Is colliding!");
        activeCollisions.Add(collision);
        // Calculate the collision force for this frame

        // Get the Rigidbody2D components of the colliding objects
        Rigidbody2D rb1 = GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = collision.rigidbody;

        Vector2 relativeVelocity = Vector2.zero;
        // Calculate the relative velocity between the two colliding objects
        if (rb2 != null) {
            relativeVelocity = rb2.velocity - rb1.velocity;
        }
        else {
            relativeVelocity = rb1.velocity;
        }
        

        // Calculate the impulse using the mass of the colliding objects
        float impulse = collision.otherCollider.attachedRigidbody.mass * relativeVelocity.magnitude;
        float collisionForce = impulse / Time.fixedDeltaTime;
        
        // Update the total collision force
        totalCollisionForce += collisionForce;

        // Check if the agent is colliding with a wall, 
        if (collision.collider.CompareTag("Wall"))
        {
            isCollidingWithWall = true;
            wallCount++;
        }
        else {
            //Colliding with agent
            agentCollisionForce += collisionForce;
        }

        // Handle collisions with other pushable objects
        PushableObject otherPushable = collision.collider.GetComponent<PushableObject>();
        if (otherPushable != null)
        {
            float otherPushingPower = otherPushable.GetPushingPower();

            if (pushingPower < otherPushingPower)
            {
                Vector2 forceDirection = (transform.position - collision.transform.position).normalized;
                float forceMultiplier = otherPushingPower - pushingPower;
                float forceAmount = forceMultiplier * 50f; // Adjust this value to achieve desired pushing force
                if (isCollidingWithWall) {
                    forceAmount *= 0.3f;
                }

                agentCollisionVec += collisionForce * forceDirection;

                rb.AddForce(forceDirection * forceAmount);
            }
        }
        else {
            //Colliding with wall
            float wallRotation = collision.collider.transform.eulerAngles.z;
            float radians = wallRotation * Mathf.Deg2Rad;
            Vector2 wallNorm = new Vector2(Mathf.Cos(radians + Mathf.PI / 2), Mathf.Sin(radians + Mathf.PI / 2));

            //Only 1 collision can occur between a circle and a box
            Vector2 collisionPoint = collision.contacts[0].point;

            float dwi = (transform.position - collision.transform.position).magnitude;
            float r = (transform.position - collision.transform.position).magnitude; // TODO : ANOTHER WAY TO GET RADIUS OF AGENT
            rb.AddForce(wallNorm/dwi);
            
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Calculate the collision force for the last frame of the collision
        // Get the Rigidbody2D components of the colliding objects
        Rigidbody2D rb1 = GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = collision.rigidbody;

        Vector2 relativeVelocity = Vector2.zero;
        // Calculate the relative velocity between the two colliding objects
        if (rb2 != null) {
            relativeVelocity = rb2.velocity - rb1.velocity;
        }
        else {
            relativeVelocity = rb1.velocity;
        }

        // Calculate the impulse using the mass of the colliding objects
        float impulse = collision.otherCollider.attachedRigidbody.mass * relativeVelocity.magnitude;
        float collisionForce = impulse / Time.fixedDeltaTime;

        if (collision.collider.CompareTag("Wall"))
        {
            wallCount--;
            if (wallCount == 0)
                isCollidingWithWall = false;
        }
        else {
            agentCollisionForce -= collisionForce;
            Vector2 forceDirection = (transform.position - collision.transform.position).normalized;
            agentCollisionVec -= collisionForce * forceDirection;
        }

        // Subtract the collision force from the current total collision force
        totalCollisionForce -= collisionForce;

        activeCollisions.RemoveAll(c => c.collider.GetInstanceID() == collision.collider.GetInstanceID());
    }

    public bool IsCollidingWithWall()
    {
        return isCollidingWithWall;
    }

    public float getAgentCollisionForce() 
    {
        return agentCollisionForce;
    }

    public float getTotalCollisionForce()
    {
        return totalCollisionForce;
    }

    public List<Collision2D> GetActiveCollisions()
    {
        return activeCollisions;
    }

    public Vector2 getAgentCollisionVec() 
    {
        return agentCollisionVec;
    }

        //     // Check if the total collision force exceeds the force threshold
        // if (collisionForce > forceThreshold)
        // {
        //     // Perform desired actions when the force threshold is exceeded
        //     Debug.Log("Collision force exceeded the threshold! Falling!");
        //     agent.AgentFalls();
        // }
}
