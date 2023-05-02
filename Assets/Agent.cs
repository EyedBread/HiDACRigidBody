
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using System.IO;
using System;



public class Agent : MonoBehaviour {
    public const float epsilon = 0.05f;
    [SerializeField] public NavMeshAgent dummyAgent;
    private List<Vector3> waypoints;
    private int currentCornerIndex = 0;
    private Vector3 previousTargetPosition;

    private FieldOfView fieldOfView;

    public bool isFallen = false;

    //The threshold in which the magnitude of the repulsionforce vector turns the agent into a fallen agent
    public float repelsionMagThreshold = 5.0f;

    private Vector2 prevForce = new Vector2(0, 0);

    public float detectionRadius = 5.0f; // The range within which agents will be detected

    // public float angleInDegrees = 0.0f; // The angle
    // public float angleInRadians = 0.0f; // The angle

    public Rigidbody2D rb;
    // Start is called before the first frame update

    private CircleCollider2D agentCollider;

    private AgentCollisionHandler agentCollisionHandler;
    private HashSet<Transform> FindNearbyObjects(float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        HashSet<Transform> nearbyObjects = new HashSet<Transform>();

        foreach (Collider2D collider in colliders)
        {
            if (collider.transform != transform)
            {
                nearbyObjects.Add(collider.transform);
            }
        }

        return nearbyObjects;
    }
    public void AgentFalls()
    {
        // Your falling logic here
        isFallen = true;

        // Change the agent's layer to "FallenAgent"
        gameObject.layer = LayerMask.NameToLayer("FallenAgent");

        // Debug.Log(gameObject.name + " HAS FALLEN");

        // Adjust the agent's collider
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        // PushableObject pushableObject= GetComponent<PushableObject>();

        // if (pushableObject != null) {
        //     Destroy(pushableObject);
        // }

        if (circleCollider != null)
        {
            Destroy(circleCollider);
        }

    }

        // Follows equation 6, 7, 8, 9
    Vector2 calcAgentForce(Transform visibleAgent)
    {
        Vector2 meToYou = visibleAgent.position - transform.position;

        Rigidbody2D visibleAgentRigidbody = visibleAgent.GetComponent<Rigidbody2D>();
        Agent otherAgent = visibleAgent.GetComponent<Agent>();
            // Access the visible agent's velocity
        Vector2 otherVel = otherAgent.getVelocity();

        //Other Agent Avoidance: Overtaking and bi-directional flow
        //that agent is walking in the opposite direction and with distance smaller than vislong-1.5
        // Debug.Log( "Dotproduct between the 2 velocities: " + Mathf.Abs(Vector2.Dot(vel.normalized, otherVel.normalized)) + " with vels " + vel.normalized + " and " + otherVel.normalized);
        if (Vector2.Dot(otherVel.normalized, vel.normalized) > 0.8 && meToYou.magnitude > vislong - 1.5 ) {
            return Vector2.zero;
        }


        Vector2 tforce = GeometryUtils.CrossAndRecross(meToYou, vel);
        tforce.Normalize();

        float distweight, dirweight;
        distweight = Mathf.Pow(meToYou.magnitude - vislong, 2);

        if (Vector2.Dot(vel, otherVel) > 0)
            dirweight = 1.2f;
        else
            dirweight = 2.4f;
        
        
        if (Mathf.Abs(Vector2.Dot(vel.normalized, otherVel.normalized) - 1) <= epsilon * rightHandAngleMultiplier || 
            Mathf.Abs(Vector2.Dot(vel.normalized, otherVel.normalized) + 1) <= epsilon * rightHandAngleMultiplier ||
            Mathf.Abs(Vector2.Dot(vel.normalized, meToYou.normalized) - 1) <= epsilon * rightHandAngleMultiplier  ||
            Mathf.Abs(Vector2.Dot(vel.normalized, meToYou.normalized) + 1) <= epsilon * rightHandAngleMultiplier)
        {
            // Debug.Log("RIGHT HAND RULE APPLIED FOR AGENT " + gameObject.name);
            Vector2 rforce = Vector2.Perpendicular(vel); // Tangent to the right
            tforce += rforce * 0.01f;
        }

        Vector2 myDir = vel.normalized;
        Vector2 otherDir = otherVel.normalized;
    
        if (Vector2.Dot(myDir, otherDir) >= 0.655f && !panic && meToYou.magnitude < waitingRadius && waitTime <= 0)
        {
            // Debug.Log(gameObject.name + "Is waiting!");
            waiting = true;
            waitTime = rand.Next(1,50);
        }
        return tforce * distweight * dirweight;
    }

    // PushableObject pushableObject;


    void Start()
    {
        // Get the number of child GameObjects
        int childCount = transform.childCount;

        // If there is at least one child GameObject, get the reference to the first one
        if (childCount > 0)
        {
            Transform firstChildTransform = transform.GetChild(0);
            GameObject firstChildGameObject = firstChildTransform.gameObject;

            // Now you have a reference to the first child GameObject
            // Debug.Log("The name of the first child GameObject is: " + firstChildGameObject.name);

            dummyAgent = firstChildGameObject.GetComponent<NavMeshAgent>();
        }

        //TODO : HARDCODED VALUES FOR CHECKING TIME.
        attractorFinalGoal = (Vector2) gameObject.transform.position;
        attractor = gameObject.transform.position;

        // dummyAgent = this.GetComponent<NavMeshAgent>();

        if (dummyAgent == null)
        {
            Debug.LogError("Dummy agent is not assigned or does not have a NavMeshAgent component.");
            return;
        }
        waypoints = new List<Vector3>();
        // Debug.Log("Calling CalculateWaypoints from Start");
        StartCoroutine(CalculateWaypoints());
        previousTargetPosition = attractorFinalGoal;

        dummyAgent.updatePosition = false; // Prevent NavMeshAgent from controlling the GameObject's position
        dummyAgent.updateRotation = false; // Prevent NavMeshAgent from controlling the GameObject's rotation


        pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        // Debug.Log("Position: " + pos);
        rb = this.GetComponent<Rigidbody2D>();
        fieldOfView = GetComponent<FieldOfView>();
        agentCollisionHandler = this.GetComponent<AgentCollisionHandler>();
        agentCollider = GetComponent<CircleCollider2D>();
        // pushableObject = GetComponent<PushableObject>();
        lastPos = transform.position;
        radius = agentCollider.radius;
        if (isFallen) 
            AgentFalls();
        if (panic)
            becomePanicked();
    }

    Vector2 lastPos;

    //For every iteration, decrement by 1. Int is always 0 or positive. For each panicked agent in vision or high crowd ahead, increment by 1
    int panicMeter = 0;

    // Update is called once per frame
    public void AgentFixedUpdate()
    {

        //Skip looping
        if (isFallen) {
            rb.velocity = Vector2.zero;
            return;
        }
        
        // Calculate waypoints when the target position changes
        if (previousTargetPosition != attractorFinalGoal)
        {
            
            StartCoroutine(CalculateWaypoints());
            previousTargetPosition = attractorFinalGoal;
            currentCornerIndex = 0;

        }

        // Use the waypoints as attractors in your HiDAC model
        if (waypoints != null && currentCornerIndex < waypoints.Count)
        {
            Vector3 targetWayPoint = waypoints[currentCornerIndex];
            // Use targetCorner as an attractor in your HiDAC model
            attractor = targetWayPoint;

            float distanceToCurrentWaypoint = Vector3.Distance(transform.position, targetWayPoint);
            // Debug.Log("Distance to current waypoint: " + distanceToCurrentWaypoint);

            // Check if the agent has reached the current corner
            if (Vector3.Distance(transform.position, targetWayPoint) < 1)
            {
                Debug.Log("Reached waypoint !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + currentCornerIndex);
                currentCornerIndex++;
            }
        }

        float lambda = 1.0f;

        Vector2 repelForceFromAgents = Vector2.zero;
        Vector2 repelForceFromWalls = Vector2.zero;

        Vector2 currentForce = prevForce;

        Vector2 currentRepelForce = Vector2.zero;


        Vector2 attract = attractor - (Vector2) transform.position;
        float dist = attract.magnitude;

        if (dist > 0) 
            attract.Normalize();

        if (dist < 1.5)
            attractorWeight = 35;
        else
            attractorWeight = 10;

        Vector2 forceAttractor = attract * attractorWeight;
        //Debug.Log("FORCEATTRACTOR: " + forceAttractor);
        currentForce += forceAttractor;

        fieldOfView.FindVisibleTargets();

        //Get all current collisions with agent
        List<Transform> collidedList = agentCollisionHandler.collidedObjects;

        //CASE AGENT --------------------------------------------------------------
        if (panic)
            agentWeight = 10;
        else
            agentWeight = 4;
        
        num_agents_ahead = fieldOfView.visibleAgents.Count;
        //but when the crowd is very dense, then the right preference is not so obvious and several bidirectional flows can emerge (Di/=2). 
        // Modifying the length of the collision avoidance rectangle and reducing the angle for right preference based on perceived density achieves this behavior. 
        if (num_agents_ahead > 8 ) {
            vislong = 1.5f;
            fieldOfView.visLong = 1.5f;
            rightHandAngleMultiplier = 0.1f;
            panicMeter++; //
        }
        else {
            vislong = 3;
            fieldOfView.visLong = 3;
            rightHandAngleMultiplier = 1.0f;
        }



        foreach (Transform visibleAgent in fieldOfView.visibleAgents) 
        {

            Rigidbody2D visibleAgentRigidbody = visibleAgent.GetComponent<Rigidbody2D>();
            CircleCollider2D visibleAgentCircleCollider = visibleAgent.GetComponent<CircleCollider2D>();
            float visibleAgentRadius = visibleAgentCircleCollider.radius;

            Agent otherAgent = visibleAgent.GetComponent<Agent>();

            if (otherAgent.isPanicked()) {
                panicMeter++;
            }

            Vector3 direction = visibleAgent.position - transform.position;

            float distance = Vector3.Distance(transform.position, visibleAgent.position);

            // if (Vector2.Dot(direction, vel) > 0) //A semi circle // if (distance < R && 
            // {
            //     num_agents_ahead++;
            // }
            currentForce += calcAgentForce(visibleAgent) * agentWeight; //NORMAL FORCES
        }

        //Repulsion forces with other agents!!!
        // foreach(Transform collidedAgent in fieldOfView.collidedAgents)
        // {
        //         // i is this agent, j is the other agent
        //         // d_ji is the distance between their centers
        //         // ep is the person
        //         // formula for agent: (pos_i - pos_j)*(r_i + ep_i + r_j - d_ji)/ d_ji
        //         Agent otherAgent = collidedAgent.GetComponent<Agent>();
        //         Rigidbody2D collidedAgentRigidbody = collidedAgent.GetComponent<Rigidbody2D>();
        //         CircleCollider2D collidedAgentCircleCollider = collidedAgent.GetComponent<CircleCollider2D>();

        //         // Interact with collided agents
        //         Vector2 jtoi = transform.position - collidedAgent.position;

        //         float collidedAgentRadius = collidedAgentCircleCollider.radius;
                
        //         float k = (agentCollider.radius + personalSpace + collidedAgentRadius - jtoi.magnitude) / (jtoi.magnitude);

        //         repelForceFromAgents += jtoi * k;
        // }

        //CASE WALL ----------------------------------------------------------------
        wallWeight = 8;
        foreach (Transform visibleWall in fieldOfView.visibleWalls)
        {
            // Debug.Log("SEE WALL");
            Vector2 wallNorm = Vector2.zero;

            float distance = (visibleWall.position - transform.position).magnitude;

            BoxCollider2D wallCollider = visibleWall.GetComponent<BoxCollider2D>();

            if (wallCollider != null)
            {
                float wallRotation = wallCollider.transform.eulerAngles.z;
                float radians = wallRotation * Mathf.Deg2Rad;
                wallNorm = new Vector2(Mathf.Cos(radians + Mathf.PI / 2), Mathf.Sin(radians + Mathf.PI / 2));
                // Debug.Log(wallNorm);
            }

            Vector2 tempForce = GeometryUtils.CrossAndRecross(wallNorm, rb.velocity);
            // tempForce.Normalize();
            // Debug.Log(tempForce);
            tempForce *= wallWeight;
            currentForce += tempForce;
        }

        //Repulsion forces with other walls
        // foreach( Transform collidedWall in fieldOfView.collidedWalls)
        // {
        //     Debug.Log("COLLIDING WITH WALL");
        //     float distance = (collidedWall.position - transform.position).magnitude;
        //     lambda = 0.3f;
        //     BoxCollider2D wallCollider = collidedWall.GetComponent<BoxCollider2D>();
        //     // Interact with visible and collided walls
        //     float k = (agentCollider.radius + personalSpace - distance) / distance;

        //     float wallRotation = wallCollider.transform.eulerAngles.z;
        //     float radians = wallRotation * Mathf.Deg2Rad;
        //     Vector2 wallNorm = new Vector2(Mathf.Cos(radians + Mathf.PI / 2), Mathf.Sin(radians + Mathf.PI / 2));

        //     Vector2 currWallRepelForce = wallNorm * k;

        //     if (Vector2.Dot(currWallRepelForce,vel) <= 0.0f)
        //     {
        //         repelForceFromWalls += currWallRepelForce;
        //     }
        // }

        //CASE FALLEN AGENT -------------------------------------------------------
        if ((fieldOfView.visibleFallenAgents.Count > 4 || panicMeter > 10) && !panic) {
            //IDK, become panicked when you see 4 or more fallen agents simultaneously
            // And when panicMeter is too high
            becomePanicked();
        }

        Vector2 fallenAgentVec = Vector2.zero;
        foreach (Transform visibleFallenAgent in fieldOfView.visibleFallenAgents)
        {

            Vector2 d = (transform.position - visibleFallenAgent.transform.position); // obstacle k to agent i?

            if (d.magnitude < 3) {
                Beta = 0.5f;
            }

            Vector2 tempForce = GeometryUtils.CrossAndRecross(d, rb.velocity);
            tempForce.Normalize();
            tempForce *= fallenWeight;

            // Debug.Log("Force: " + tempForce);

            fallenAgentVec += tempForce;
        }

        //REPULSION FORCES
        foreach(Transform otherTransform in collidedList) {

            BoxCollider2D wallCollider = otherTransform.GetComponent<BoxCollider2D>();
            CircleCollider2D collidedAgentCircleCollider = otherTransform.GetComponent<CircleCollider2D>();

            if (wallCollider != null) {
                //WALL REPULSION FORCES
                // Debug.Log("COLLIDING WITH WALL");
                float distance = (otherTransform.position - transform.position).magnitude;
                lambda = 0.3f;
                // Interact with visible and collided walls
                float k = (agentCollider.radius + personalSpace - distance) / distance;

                float wallRotation = wallCollider.transform.eulerAngles.z;
                float radians = wallRotation * Mathf.Deg2Rad;
                Vector2 wallNorm = new Vector2(Mathf.Cos(radians + Mathf.PI / 2), Mathf.Sin(radians + Mathf.PI / 2));

                Vector2 currWallRepelForce = wallNorm * k;

                if (Vector2.Dot(currWallRepelForce,vel) <= 0.0f)
                {
                    repelForceFromWalls += currWallRepelForce;
                }
            }
            else if (collidedAgentCircleCollider != null) {
                // i is this agent, j is the other agent
                // d_ji is the distance between their centers
                // ep is the person
                // formula for agent: (pos_i - pos_j)*(r_i + ep_i + r_j - d_ji)/ d_ji
                Agent otherAgent = otherTransform.GetComponent<Agent>();
                Rigidbody2D collidedAgentRigidbody = otherTransform.GetComponent<Rigidbody2D>();

                // Interact with collided agents
                Vector2 jtoi = transform.position - otherTransform.position;

                float collidedAgentRadius = collidedAgentCircleCollider.radius;
                
                float k = (agentCollider.radius + personalSpace + collidedAgentRadius - jtoi.magnitude) / (jtoi.magnitude);

                repelForceFromAgents += jtoi * k;
            }
            
            else {
                //Couldn't determine what I was colliding with, Error!
                Debug.Log("HELP!");
            }

        }

        // gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
        repelForceFromAgents *= lambda;
        repelForce = repelForceFromAgents + repelForceFromWalls;
        // float magRepelForce = pushableObject.getTotalCollisionForce();

        
        if (repelForce.magnitude > repelsionMagThreshold) {
            //FALL DOWN AND DIE!
            // Debug.Log(gameObject.name + " HAS FALLEN");
            rb.velocity = Vector2.zero;
            AgentFalls();
        }

        // repelForceFromAgents = pushableObject.getAgentCollisionVec();

        if (Vector2.Dot(vel, repelForceFromAgents) < 0 && !panic)
        {
            // Debug.Log("STOPPING");
            stopping = true;
            stoptime = rand.Next(1,150);
            vel = Vector2.zero;
        }

        if (waiting)
            vel = Vector2.zero;

        //CurrentForce is already filled up

        currentForce.Normalize(); //Normalize force vector

        prevForce = currentForce;

        //TIME TO APPLY FORCES
        float alpha = computeAlpha();
        
        float velMagnitude = computeVel(Time.fixedDeltaTime);
        float moveFactor = alpha*velMagnitude;
        Vector2 move = moveFactor * ((1 - Beta)*currentForce + Beta*fallenAgentVec);
        // Vector2 desiredPosition = move + repelForce;
        rb.AddForce(currentForce);

        // Vector2 lastPos = transform.position;
        // rb.MovePosition((Vector2) (transform.position) + (move )*Time.fixedDeltaTime ); //+ 0.1f*repelForce
        // rb.MovePosition(Vector2.zero);
        vel = ( (Vector2)transform.position - lastPos ) / Time.fixedDeltaTime; 
        Vector2 dir = vel.normalized;
        float angleInRadians = Mathf.Atan2(dir.y, dir.x);
        float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
        rb.MoveRotation(angleInDegrees); //TODO : Why doesn't this work????
        transform.rotation = Quaternion.Euler(0, 0, angleInDegrees);
        lastPos = transform.position;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

        if (waiting || stopping) {
            rb.velocity = Vector2.zero;
        }
        //RESET
        stoptime--;
        if (stoptime == 0)
        {
            stopping = false;
        }
        waitTime--;

        waiting = false;
        // vislong = 3.0f;
        fieldOfView.visLong = vislong;
        fieldOfView.visWide = viswide;
        rightHandAngleMultiplier = 1.0f;
        num_agents_ahead = 0;
        Beta = 0;
        repelForce = Vector2.zero;

        if (panicMeter > 0)
            panicMeter--;
    }

    private IEnumerator CalculateWaypoints()
    {
        // Debug.Log("Target position: " + attractorFinalGoal);
        // Update the dummy agent's position to match the actual agent's position
        dummyAgent.transform.position = transform.position;

        bool x = dummyAgent.SetDestination(attractorFinalGoal);
        // Debug.Log(x);

        waypoints.Clear();

        // Wait for the path calculation to complete
        while (dummyAgent.pathPending)
        {
            yield return null;
        }

        if (dummyAgent.hasPath)
        {
            // Debug.Log("Dummy agent has a path!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!.");
            foreach (var corner in dummyAgent.path.corners)
            {
                // Debug.Log("added waypoint ");
                waypoints.Add(corner);
            }
        }
        else
        {
            // Debug.Log("Dummy agent does not have a path.");
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 1)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
        }
        else
        {
            //Debug.Log("Waypoints list is empty or contains only one waypoint.");
        }
    }


    // Radius of the agent's influence semicircle for density calculation, set to 2.0 for now
    public float R = 2.0f;

    //Seed generator
    private static System.Random rand = new System.Random();

    // // Position - will assume continuous for now
    private Vector2 _pos;
    public Vector2 pos {
        get => _pos;
        set => _pos = value;
    }

    // Weights - avoidance weights
    public float attractorWeight = 10;
    public float wallWeight = 1;
    public float agentWeight = 1;

    public float fallenWeight = 1;

    public float personalSpace = 1.0f;

    public float acceleration = 0.8f;
    public float maxVelocity = 0.5f;


    // The "size" of the agent
    public float radius;

    // Reduce the righthand preference based on perceived density
    float rightHandAngleMultiplier = 1.0f;

    // Updated in RepulsionOtherAgents
    // float densityAhead;

    int num_agents_ahead = 0;

    // Whether agent is stopping or waiting
    bool stopping = false;
    int stoptime = 0;
    bool waiting = false;
    int waitTime = 0;

    // Radius of area of influence for waiting behaviours
    public float waitingRadius = 1.5f;

    // Whether agent is panicked - idea: make quantitative
    public bool panic = false;

    // Fallen-agent-avoidance parameter
    float Beta = 0.0f;

    // Velocity
    Vector2 vel;

    // This is needed for repulsion forces. Until those are implemented it will be set to <0.0, 0.0> in calculateForces
    Vector2 repelForce;

    // Vision range - to calculate a vision rectangle, look out vislong units along velocity vector, then look by viswide / 2 units. 
    public float vislong = 3.0f;
    public float viswide = 1.5f;

    // Attractor
    public Vector2 attractor;

    // Attractor final goal
    [SerializeField]
    public Vector3 attractorFinalGoal;

    float computeVel(float deltaT) {
        // Implementation goes here
        if (vel.magnitude > maxVelocity)
            // return getSpeed();
            return maxVelocity;
        else
        {
            // v2f dir;
            // v2fNormalize(vel, dir);
            // v2f a = {acceleration*dir[0], acceleration*dir[1]};

            // // cout << "HERE" << endl;
            // v2fAdd(vel, a, deltaT, vel);
            return vel.magnitude + acceleration * Time.fixedDeltaTime;
        }
    }

    void becomePanicked() {
        maxVelocity *= 2;
        acceleration *= 3;
        personalSpace *= 0.6f; //Decrease personalspace to push other agents more easily
        panic = true;
    }

    float computeAlpha() {
        // Implementation goes here
        if (repelForce.magnitude > 0.0 || stopping || waiting)
            return 0.0f;
        else
            return 1.0f;
    }

    public float getPersonalSpace() {
        // Implementation goes here
        return personalSpace;
    }


    // Function to 'reset' at the end of a simulation step 

    public bool fallen() {
        return isFallen;
    }

    public bool isPanicked() {
        return panic;
    }

    public Vector2 getDirection(Vector2 pos)
    {
        return pos - (Vector2)transform.position;
    }


    public Vector2 getVelocity()
    {
        // Implementation goes here
        return vel;
    }
}