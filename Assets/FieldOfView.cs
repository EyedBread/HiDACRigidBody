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

    [SerializeField] public LayerMask agentMask;
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
        // agentMask = LayerMask.GetMask("Agent");
        wallMask = LayerMask.GetMask("Wall");
        fallenAgentMask = LayerMask.GetMask("FallenAgent");
        obstacleMask = LayerMask.GetMask("Obstacle") | LayerMask.GetMask("Wall");
        allAgents = FindObjectsOfType<Agent>();
        agent = GetComponent<Agent>();
        agentCollider = GetComponent<CircleCollider2D>();
        myRadius = agentCollider.radius;
        allWalls = FindObjectsOfType<Wall>();
        myPersonalSpace = agent.getPersonalSpace();

    }

    public void FindVisibleTargets()
    {
        visibleAgents.Clear();
        visibleWalls.Clear();
        visibleFallenAgents.Clear();
        collidedAgents.Clear();
        collidedObstacles.Clear();
        collidedWalls.Clear();
        if (true) {
            FindVisibleObjects();
        }
        else {
            FindPossibleCollidersNaive();
        }

    }


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
                if (!otheragent.fallen())
                    visibleAgents.Add(otheragent.transform);
                // Debug.Log("Found agent");
                else {
                    // Debug.Log("I see fallen agent!");
                    visibleFallenAgents.Add(otheragent.transform); //TODO : DOESN'T CHECK FOR THE FALLEN AGENTS BOX BUT RATHER CIRCLE, MAY NEED TO CHANGE LATER
                }
                    
            }
                

            //CHECK FOR COLLISION
            float distance = (myPos - otherPos).magnitude;

            if (distance < myRadius + myPersonalSpace) {
                if (!otheragent.fallen())
                    collidedAgents.Add(otheragent.transform);
                //DON'T DO REPULSION FORCES WITH FALLEN AGENTS
            }
        }

        foreach(Wall wall in allWalls)
        {

            if (AgentHasVisionOfWall(myPos, alpha, visLong, visWide, wall.getStart(), wall.getEnd())) {
                visibleWalls.Add(wall.transform);
            }

            if (AgentHasCollisionWithWall(myPos, alpha, visLong, visWide, wall.getStart(), wall.getEnd(), transform.localScale.y)) {
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

        // Find agents in range
        Collider2D[] agentsInRange = Physics2D.OverlapBoxAll((Vector2)transform.position + myDir * visLong / 2, new Vector2(visLong, visWide), alpha, agentMask);
        CheckVisibleAndCollidedObjects(agentsInRange, visibleAgents, collidedAgents, myPersonalSpace, true);

        //Find fallen agents in range
        Collider2D[] fallenAgentsInRange = Physics2D.OverlapBoxAll((Vector2)transform.position + myDir * visLong / 2, new Vector2(visLong, visWide), alpha, fallenAgentMask);
        CheckVisibleAndCollidedObjects(fallenAgentsInRange, visibleFallenAgents, collidedAgents, myPersonalSpace, true);

        DebugDrawBox((Vector2)transform.position + myDir * visLong / 2, new Vector2(visLong, visWide), alpha, Color.yellow, 0.01f);

        // Find walls in range
        Collider2D[] wallsInRange = Physics2D.OverlapBoxAll((Vector2)transform.position + myDir * visLong / 2, new Vector2(visLong, visWide), alpha, wallMask);
        if (wallsInRange.Length > 0) {
            // Debug.Log(gameObject.name + "Sees WALL");
        }
        CheckVisibleAndCollidedObjects(wallsInRange, visibleWalls, collidedWalls, myPersonalSpace, false);
    }

    void DebugDrawBox( Vector2 point, Vector2 size, float angle, Color color, float duration) {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x/2f;
        Vector2 up = orientation * Vector2.up * size.y/2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
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

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask);

            if (!hit || hit.collider.transform == target)
            {
                // Debug.Log(gameObject.name + " Sees " + target.name);
                visibleList.Add(target);
            }

            // if (isAgent)
            // {
            //     Agent agent = target.GetComponent<Agent>();
            //     // Check for collision
            //     //float distance = (transform.position - target.position).magnitude;
            //     if (distanceToTarget < myRadius + personalSpace && !agent.fallen())
            //     {
            //         collidedList.Add(target);
            //     }
            // }
            // else
            // {
            //     // Debug.Log(" I SEE WALL");
            //     // Check for collision with wall
            //     //float distanceToTarget1 = Vector3.Distance(transform.position, target.position);
            //     //float distance = (transform.position - target.position).magnitude;

            //     //BoxCollider2D wallCollider = target.GetComponent<BoxCollider2D>();
            //     //Vector2 worldSize = target.TransformVector(wallCollider.size);
            //     //Debug.Log(worldSize);
            //     //float wallThickness = Mathf.Min(Mathf.Abs(worldSize.x), Mathf.Abs(worldSize.y));

            //     BoxCollider2D wallCollider = target.GetComponent<BoxCollider2D>();
            //     float wallThickness = wallCollider.size.y;
            //     //Debug.Log(wallThickness);

            //     //bool colliderContainsTransform = wallCollider.bounds.Contains(transform.position);
            //     //Debug.Log(colliderContainsTransform);

            //     RaycastHit2D hit1 = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, wallMask);
            //     if (hit1.collider != null && hit1.collider.transform == target && distanceToTarget < myRadius + wallThickness / 2)
            //     {
            //         Debug.Log(gameObject.name + " collided with " + target.name);
            //         collidedList.Add(target);
            //     }
            // }
        }
    }
}