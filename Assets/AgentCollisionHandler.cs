using UnityEngine;
using System.Collections.Generic;

public class AgentCollisionHandler : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> collidedObjects = new List<Transform>();

    public float personalSpace;
    public Agent agent;

    public Rigidbody2D rb;

    void Start() {
        agent = GetComponent<Agent>();
        personalSpace = agent.getPersonalSpace();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        Agent otherAgent = collision.collider.GetComponent<Agent>();
        if (otherAgent != null)
        {
            float otherPersonalSpace = otherAgent.getPersonalSpace();

            if (personalSpace > otherPersonalSpace)
            {
                // Vector2 forceDirection = (transform.position - collision.transform.position).normalized;
                // float forceMultiplier = otherPersonalSpace - personalSpace;
                // float forceAmount = forceMultiplier * 50f; // Adjust this value to achieve desired pushing force

                // rb.AddForce(forceDirection * forceAmount);
                Transform other = collision.transform;
                if (!collidedObjects.Contains(other))
                {
                    collidedObjects.Add(other);
                }
            }
            else {

            }
        }
        else {
            Transform other = collision.transform;
            if (!collidedObjects.Contains(other))
            {
                collidedObjects.Add(other);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Transform other = collision.transform;
        if (collidedObjects.Contains(other))
        {
            collidedObjects.Remove(other);
        }
    }
}
