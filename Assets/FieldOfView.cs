using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Matrix2x2
{
    public float m00, m01;
    public float m10, m11;

    public Matrix2x2(float m00, float m01, float m10, float m11)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m10 = m10;
        this.m11 = m11;
    }

    public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
    {
        return new Matrix2x2(
            a.m00 * b.m00 + a.m01 * b.m10,
            a.m00 * b.m01 + a.m01 * b.m11,
            a.m10 * b.m00 + a.m11 * b.m10,
            a.m10 * b.m01 + a.m11 * b.m11
        );
    }

    public static Vector2 operator *(Matrix2x2 a, Vector2 b)
    {
        return new Vector2(
            a.m00 * b.x + a.m01 * b.y,
            a.m10 * b.x + a.m11 * b.y
        );
    }
}

public class FieldOfView : MonoBehaviour
{
    // public Rigidbody2D rb;
    private CircleCollider2D agentCollider;

    private Agent agent;
    public float visLong;
    public float visWide;

    public float myPersonalSpace;

    public bool useNaive = true;

    public float myRadius;

    public float detectionRadius = 5.0f; // The range within which agents will be detected

    Agent[] allAgents;
    // List<Agent> otherAgents;

    Wall[] allWalls;

    public LayerMask agentMask;
    public LayerMask wallMask;
    public LayerMask fallenAgentMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleAgents = new List<Transform>();
    [HideInInspector]
    public List<Transform> visibleWalls = new List<Transform>();
    [HideInInspector]
    public List<Transform> visibleFallenAgents = new List<Transform>();

    [HideInInspector]
    public List<Transform> collidedAgents = new List<Transform>();

    [HideInInspector]
    public List<Transform> collidedWalls = new List<Transform>();

    [HideInInspector]
    public List<Transform> collidedObstacles = new List<Transform>();

    void Start()
    {
        agentMask = LayerMask.GetMask("Agent");
        wallMask = LayerMask.GetMask("Wall");
        fallenAgentMask = LayerMask.GetMask("FallenAgent");
        obstacleMask = LayerMask.GetMask("Obstacle");
        allAgents = FindObjectsOfType<Agent>();
        agent = GetComponent<Agent>();
        agentCollider = GetComponent<CircleCollider2D>();
        myRadius = agentCollider.radius;
        allWalls = FindObjectsOfType<Wall>();
        myPersonalSpace = agent.getPersonalSpace();
        // StartCoroutine(FindTargetsWithDelay(0.2f));

    }

    // IEnumerator FindTargetsWithDelay(float delay)
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(delay);
    //         FindVisibleTargets();
    //     }
    // }

    public void FindVisibleTargets()
    {
        visibleAgents.Clear();
        visibleWalls.Clear();
        visibleFallenAgents.Clear();
        collidedAgents.Clear();
        collidedObstacles.Clear();
        collidedWalls.Clear();
        if (!useNaive) {
            FindVisibleObjects(agentMask, visibleAgents);
            FindVisibleObjects(wallMask, visibleWalls);
            FindVisibleObjects(fallenAgentMask, visibleFallenAgents);
        }
        else {
            FindPossibleCollidersNaive();
        }

    }

    void OnDrawGizmos()
    {

        float alpha = transform.eulerAngles.z;
        float alphaRadians = alpha * Mathf.Deg2Rad;
        Vector2 myDir = new Vector2(Mathf.Cos(alphaRadians), Mathf.Sin(alphaRadians));
        Matrix2x2 transf = new Matrix2x2(Mathf.Cos(alphaRadians),-Mathf.Sin(alphaRadians),Mathf.Sin(alphaRadians),Mathf.Cos(alphaRadians));

        Vector2 boxCenter = (Vector2)transform.position + transf * myDir * visLong / 2;
        Vector2 boxSize = new Vector2(visLong, visWide);

        // Set the Gizmos color
        Gizmos.color = Color.green;

        // Save the current Gizmos matrix
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        // Set the Gizmos matrix with the position, rotation, and scale
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, alpha), Vector3.one);

        // Draw the wireframe cube
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        // Reset the Gizmos matrix to the previous value
        Gizmos.matrix = oldGizmosMatrix;
    }

    void FindPossibleCollidersNaive()
    {
        //Update values that may have changed
        myPersonalSpace = agent.getPersonalSpace();
        visLong = agent.vislong;
        visWide = agent.viswide;
        Vector2 myPos = agent.transform.position;
        float alpha = transform.eulerAngles.z;
        float alphaRadians = alpha * Mathf.Deg2Rad;
        Vector2 myDir = new Vector2(Mathf.Cos(alphaRadians), Mathf.Sin(alphaRadians));
        myDir.Normalize();
        

        foreach(Agent otheragent in allAgents)
        {
            if (otheragent == agent) { //SKIP FINDING YOURSELF
                continue;
            }

            Vector2 otherPos = otheragent.transform.position;
            float otherRadius = otheragent.radius;
            
            // float distance = (myPos - otherPos).magnitude - otherRadius;

            float d = GeometryUtils.PtToLineDist(otherPos, myPos, myDir, visLong);

            float er = otherRadius + visWide;
            Vector2 meToYou = otherPos - myPos;
            // Debug.Log("MeToYOu: " + meToYou + ", myDir: " + myDir + gameObject.name);
            if (d <= er && Vector2.Dot(meToYou, myDir) > 0) {
                visibleAgents.Add(otheragent.transform);
                // Debug.Log("Found agent");
            }
                

            //CHECK FOR COLLISION
            float distance = (myPos - otherPos).magnitude;

            if (distance < myRadius + myPersonalSpace) {
                collidedAgents.Add(otheragent.transform);
            }
        }

        foreach(Wall wall in allWalls)
        {

        }


    }

    void FindVisibleObjects(LayerMask targetMask, List<Transform> visibleList)
    {
        //Direction of agent
        float alpha = transform.eulerAngles.z;
        float alphaRadians = alpha * Mathf.Deg2Rad;
        Vector2 myDir = new Vector2(Mathf.Cos(alphaRadians), Mathf.Sin(alphaRadians));

        // Vector2 myDir = agent.getVelocity().normalized;
        Matrix2x2 transf = new Matrix2x2(Mathf.Cos(alpha),-Mathf.Sin(alpha),Mathf.Sin(alpha),Mathf.Cos(alpha));
        Collider2D[] targetsInRange = Physics2D.OverlapBoxAll((Vector2) transform.position + transf*myDir*visLong/2, new Vector2(visWide, visLong), alpha, targetMask);
        // OnDrawGizmos();
        Debug.Log("targetsInRange size: " + targetsInRange.Length);
        for (int i = 0; i < targetsInRange.Length; i++)
        {
            Transform target = targetsInRange[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Mathf.Abs(Vector3.Angle(transform.up, directionToTarget)) <= 90)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask);

                // Draw the ray in the scene view (use Color.red or any other desired color)
                Debug.DrawLine(transform.position, transform.position + directionToTarget * distanceToTarget, Color.red, 0.2f);

                if (!hit)
                {
                    visibleList.Add(target);
                }
            }
        }
    }
}