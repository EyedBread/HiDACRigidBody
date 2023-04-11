using UnityEngine;
using System.Collections.Generic;

public class AgentCollisionHandler : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> collidedObjects = new List<Transform>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!collidedObjects.Contains(other.transform))
        {
            collidedObjects.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (collidedObjects.Contains(other.transform))
        {
            collidedObjects.Remove(other.transform);
        }
    }
}