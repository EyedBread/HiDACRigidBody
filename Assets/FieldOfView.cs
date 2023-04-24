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
    [SerializeField] public LayerMask agentMask;

    // public Rigidbody2D rb;
    private CircleCollider2D agentCollider;

    private Agent agent;
    public float visLong;
    public float visWide;

    public float myPersonalSpace;

    public bool useNaive = false;

    public float myRadius;

    public float detectionRadius = 5.0f; // The range within which agents will be detected

    Agent[] allAgents;
    // List<Agent> otherAgents;

    Wall[] allWalls;


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
        //agentMask = LayerMask.GetMask("Agent");
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

        if (true)
        {
            //FindVisibleObjects(agentMask, visibleAgents);
            //FindVisibleObjects(wallMask, visibleWalls);
            //FindVisibleObjects(fallenAgentMask, visibleFallenAgents);

            FindVisibleObjects();

        }
        else
        {
            Debug.Log("Naive!!!!!!!!!!!!!");
            FindPossibleCollidersNaive();
        }

    }

    /* void OnDrawGizmos()
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
    }*/


    //--------------------------------NAIVE FUNCTIONS START------------------------------------
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



        foreach (Agent otheragent in allAgents) // check 
        {
            if (otheragent == agent)
            { //SKIP FINDING YOURSELF
                continue;
            }

            Vector2 otherPos = otheragent.transform.position;
            float otherRadius = otheragent.radius;

            // float distance = (myPos - otherPos).magnitude - otherRadius;

            float d = GeometryUtils.PtToLineDist(otherPos, myPos, myDir, visLong);

            float er = otherRadius + visWide;
            Vector2 meToYou = otherPos - myPos;
            // Debug.Log("MeToYOu: " + meToYou + ", myDir: " + myDir + gameObject.name);
            if (d <= er && Vector2.Dot(meToYou, myDir) > 0)
            {
                visibleAgents.Add(otheragent.transform);
                // Debug.Log("Found agent");
            }


            //CHECK FOR COLLISION
            float distance = (myPos - otherPos).magnitude;

            if (distance < myRadius + myPersonalSpace)
            {
                collidedAgents.Add(otheragent.transform);
            }
        }

        foreach (Wall wall in allWalls)
        {

            if (AgentHasVisionOfWall(myPos, alpha, visLong, visWide, wall.getStart(), wall.getEnd()))
            {
                visibleWalls.Add(wall.transform);
            }

            if (AgentHasCollisionWithWall(myPos, alpha, visLong, visWide, wall.getStart(), wall.getEnd(), transform.localScale.y))
            {
                collidedWalls.Add(wall.transform);
            }
        }


    }


    //SIMPLIFIED FUNCTION, DON'T ACCOUNT FOR THICKNESS OF WALL
    public static bool AgentHasVisionOfWall(Vector2 agentPosition, float theta, float vislong, float viswide, Vector2 start, Vector2 end)
    {
        // Calculate rotation matrix
        float[,] R = new float[,]
        {
            { Mathf.Cos(theta), -Mathf.Sin(theta) },
            { Mathf.Sin(theta),  Mathf.Cos(theta) }
        };

        // Rotate wall coordinates
        Vector2 r_start = RotateAndTranslate(R, start, agentPosition);
        Vector2 r_end = RotateAndTranslate(R, end, agentPosition);

        // Check if wall is within agent's FOV
        //TODO : ADD SO THE START AND END POINTS DOESN'T NEED TO BE IN THE RECTANGLE, ONLY THE LINE, or does it already?
        if ((r_start.x < 0 && r_end.x < 0) || (r_start.x > vislong && r_end.x > vislong) ||
            (r_start.y < -viswide / 2 && r_end.y < -viswide / 2) || (r_start.y > viswide / 2 && r_end.y > viswide / 2))
        {
            return false;
        }

        return true;
    }

    public static bool AgentHasCollisionWithWall(Vector2 agentPosition, float theta, float vislong, float viswide, Vector2 start, Vector2 end, float thickness)
    {
        // Calculate rotation matrix
        float[,] R = new float[,]
        {
            { Mathf.Cos(theta), -Mathf.Sin(theta) },
            { Mathf.Sin(theta),  Mathf.Cos(theta) }
        };

        // Rotate wall coordinates
        Vector2 r_start = RotateAndTranslate(R, start, agentPosition);
        Vector2 r_end = RotateAndTranslate(R, end, agentPosition);

        // Calculate wall vector and normalize it
        Vector2 wallDirection = (r_end - r_start).normalized;

        // Calculate wall normal
        Vector2 wallNormal = new Vector2(-wallDirection.y, wallDirection.x);

        // Calculate the new start and end points considering the wall thickness
        Vector2 halfThicknessOffset = wallNormal * (thickness / 2);
        Vector2 r_start1 = r_start + halfThicknessOffset;
        Vector2 r_start2 = r_start - halfThicknessOffset;
        Vector2 r_end1 = r_end + halfThicknessOffset;
        Vector2 r_end2 = r_end - halfThicknessOffset;

        // Define agent's FOV rectangle vertices
        Vector2[] fovVertices = new Vector2[]
        {
            new Vector2(0, -viswide / 2),
            new Vector2(vislong, -viswide / 2),
            new Vector2(vislong, viswide / 2),
            new Vector2(0, viswide / 2)
        };

        // Check if any FOV vertex is inside the wall rectangle
        if (IsPointInsideRectangle(fovVertices[0], r_start1, r_start2, r_end1, r_end2) ||
            IsPointInsideRectangle(fovVertices[1], r_start1, r_start2, r_end1, r_end2) ||
            IsPointInsideRectangle(fovVertices[2], r_start1, r_start2, r_end1, r_end2) ||
            IsPointInsideRectangle(fovVertices[3], r_start1, r_start2, r_end1, r_end2))
        {
            return true;
        }

        return false;
    }

    private static Vector2 RotateAndTranslate(float[,] R, Vector2 point, Vector2 agentPosition)
    {
        Vector2 translatedPoint = point - agentPosition;
        Vector2 rotatedPoint = new Vector2(
            R[0, 0] * translatedPoint.x + R[0, 1] * translatedPoint.y,
            R[1, 0] * translatedPoint.x + R[1, 1] * translatedPoint.y
        );

        return rotatedPoint;
    }

    private static bool IsPointInsideRectangle(Vector2 point, Vector2 rectA, Vector2 rectB, Vector2 rectC, Vector2 rectD)
    {
        float areaRectangle = TriangleArea(rectA, rectB, rectC) + TriangleArea(rectA, rectC, rectD);
        float areaSum = TriangleArea(point, rectA, rectB) +
                        TriangleArea(point, rectB, rectC) +
                        TriangleArea(point, rectC, rectD) +
                        TriangleArea(point, rectD, rectA);

        // Use an epsilon value to account for floating-point rounding errors
        float epsilon = 0.0001f;
        return Mathf.Abs(areaRectangle - areaSum) < epsilon;
    }

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2);
    }

    //---------------------------NAIVE FUNCTIONS END-------------------------------------------------------
    /*
        void FindVisibleObjects(LayerMask targetMask, List<Transform> visibleList)
        {
            //Direction of agent
            float alpha = transform.eulerAngles.z; // rotation around the z-axis
            float alphaRadians = alpha * Mathf.Deg2Rad; // deg to radians
            Vector2 myDir = new Vector2(Mathf.Cos(alphaRadians), Mathf.Sin(alphaRadians)); // direction vector


            //used to align direction vector myDir with the agent's field of view.
            Matrix2x2 transf = new Matrix2x2(Mathf.Cos(alphaRadians),-Mathf.Sin(alphaRadians),Mathf.Sin(alphaRadians),Mathf.Cos(alphaRadians)); 

            Collider2D[] targetsInRange = Physics2D.OverlapBoxAll((Vector2) transform.position + transf*myDir*visLong/2, new Vector2(visWide, visLong), alpha, targetMask);

            Debug.Log("targetsInRange size: " + targetsInRange.Length);

            for (int i = 0; i < targetsInRange.Length; i++)
            {
                Transform target = targetsInRange[i].transform;

                // Check if the target is the same as the agent itself, and skip it
                if (target == transform)
                {
                    continue;
                }

                Vector3 directionToTarget = (target.position - transform.position).normalized; // from agent to target

                if (Mathf.Abs(Vector3.Angle(transform.up, directionToTarget)) <= 90) // In front of the agent
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask); //look for blocking obstacles

                    if (hit)
                    {
                        Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
                    }

                    // Draw the ray in the scene view (use Color.red or any other desired color)
                    Debug.DrawLine(transform.position, transform.position + directionToTarget * distanceToTarget, Color.red, 0.1f);

                    if (!hit)
                    {
                        Debug.Log("No Obstacles!!!!!!!!!!!!!!!!!");
                        visibleList.Add(target);
                    }
                }
            }
        } */

    void FindVisibleObjects()
    {
        //Update values that may have changed
        myPersonalSpace = agent.getPersonalSpace();
        visLong = agent.vislong;
        visWide = agent.viswide;

        // Direction of agent
        float alpha = transform.eulerAngles.z;
        float alphaRadians = alpha * Mathf.Deg2Rad;
        Vector2 myDir = new Vector2(Mathf.Cos(alphaRadians), Mathf.Sin(alphaRadians));
        myDir.Normalize();

        // Used to align direction vector myDir with the agent's field of view
        Matrix2x2 transf = new Matrix2x2(Mathf.Cos(alphaRadians), -Mathf.Sin(alphaRadians), Mathf.Sin(alphaRadians), Mathf.Cos(alphaRadians));

        // Find agents in range
        Collider2D[] agentsInRange = Physics2D.OverlapBoxAll((Vector2)transform.position + transf * myDir * visLong / 2, new Vector2(visWide, visLong), alpha, agentMask);
        CheckVisibleAndCollidedObjects(agentsInRange, visibleAgents, collidedAgents, myPersonalSpace, true);

        // Find walls in range
        Collider2D[] wallsInRange = Physics2D.OverlapBoxAll((Vector2)transform.position + transf * myDir * visLong / 2, new Vector2(visWide, visLong), alpha, wallMask);
        CheckVisibleAndCollidedObjects(wallsInRange, visibleWalls, collidedWalls, myPersonalSpace, false);
    }

    void CheckVisibleAndCollidedObjects(Collider2D[] targetsInRange, List<Transform> visibleList, List<Transform> collidedList, float personalSpace, bool isAgent)
    {
        for (int i = 0; i < targetsInRange.Length; i++)
        {
            Transform target = targetsInRange[i].transform;

            // Check if the target is the same as the agent itself, and skip it
            if (target == transform)
            {
                continue;
            }

            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Mathf.Abs(Vector3.Angle(transform.up, directionToTarget)) <= 90)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                //RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask);

                //visibleAgents.Add(otheragent.transform);
                visibleList.Add(target);

                //if (!hit)
                //{
                //    visibleList.Add(target);
                //}
            }

            if (isAgent)
            {
                // Check for collision
                float distance = (transform.position - target.position).magnitude;
                if (distance < myRadius + personalSpace)
                {
                    collidedList.Add(target);
                }
            }
        }
    }

}