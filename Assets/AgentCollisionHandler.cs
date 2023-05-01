using UnityEngine;
using System.Collections.Generic;

public class AgentCollisionHandler : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> collidedObjects = new List<Transform>();

    void OnCollisionEnter2D(Collision2D collision)
    {
        Transform other = collision.transform;
        if (!collidedObjects.Contains(other))
        {
            collidedObjects.Add(other);
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
