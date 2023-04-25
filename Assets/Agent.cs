using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.IO;
using System;



public class Agent : MonoBehaviour {


    public const float epsilon = 0.05f;

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
    void AgentFalls()
    {
        // Your falling logic here

        // Change the agent's layer to "FallenAgent"
        gameObject.layer = LayerMask.NameToLayer("FallenAgent");

        Debug.Log(gameObject.name + " HAS FALLEN");

        // Adjust the agent's collider
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        if (circleCollider != null)
        {
            // float radius = circleCollider.radius;
            // Destroy(circleCollider);
            

            // BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();

            // boxCollider.size = new Vector2(radius,radius); //TODO : FINETUNE THIS
            // boxCollider.isTrigger = true;
            //TODO : ADD OFFSET
        }

        // BoxCollider2D collider = GetComponent<BoxCollider2D>();
        // collider.size = new Vector2(1, 0.5f); // Adjust the size as needed
        // collider.offset = new Vector2(0, -0.25f); // Adjust the offset as needed
        // collider.isTrigger = true;
    }

        // Follows equation 6, 7, 8, 9
    Vector2 calcAgentForce(Transform visibleAgent)
    {
        Vector2 meToYou = visibleAgent.position - transform.position;

        Rigidbody2D visibleAgentRigidbody = visibleAgent.GetComponent<Rigidbody2D>();
        Agent otherAgent = visibleAgent.GetComponent<Agent>();
            // Access the visible agent's velocity
            // otherVel = visibleAgentRigidbody.velocity;
        Vector2 otherVel = otherAgent.getVelocity();

        //Other Agent Avoidance: Overtaking and bi-directional flow
        //that agent is walking in the opposite direction and with distance smaller than vislong-1.5
        if (Vector2.Dot(otherVel.normalized, vel.normalized) > 0.8 && meToYou.magnitude > vislong - 1.5 ) {
            return Vector2.zero;
        }


        Vector2 tforce = GeometryUtils.CrossAndRecross(meToYou, vel);
        tforce.Normalize();

        float distweight, dirweight;
        distweight = Mathf.Pow(meToYou.magnitude - vislong, 2);

        if (Vector2.Dot(vel, otherVel) > 0)
        {
            dirweight = 1.2f;
        }
        else
        {
            dirweight = 2.4f;
        }

        // Debug.Log( "Dotproduct between the 2 velocities: " + Mathf.Abs(Vector2.Dot(vel, otherVel)) + " with vels " + vel + " and " + otherVel);
        if (Mathf.Abs(Vector2.Dot(vel.normalized, otherVel.normalized) - 1) <= epsilon * rightHandAngleMultiplier || 
            Mathf.Abs(Vector2.Dot(vel.normalized, otherVel.normalized) + 1) <= epsilon * rightHandAngleMultiplier ||
            Mathf.Abs(Vector2.Dot(vel.normalized, meToYou.normalized) - 1) <= epsilon * rightHandAngleMultiplier  ||
            Mathf.Abs(Vector2.Dot(vel.normalized, meToYou.normalized) + 1) <= epsilon * rightHandAngleMultiplier)
        {
            // Debug.Log("RIGHT HAND RULE APPLIED FOR AGENT " + gameObject.name);
            Vector2 rforce = Vector2.Perpendicular(vel); // Tangent to the right
            tforce += rforce * 0.05f;
        }

        Vector2 myDir = vel.normalized;
        Vector2 otherDir = otherVel.normalized;
    
        if (Vector2.Dot(myDir, otherDir) >= 0.655f && !panic && meToYou.magnitude < waitingRadius && waitTime <= 0)
        {
            // Debug.Log("meToYou" + meToYou + ", magnitude: " + meToYou.magnitude + ", waitingRadius: " + waitingRadius);
            waiting = true;
            waitTime = rand.Next(1,50);
            // Debug.Log("AGENT " + gameObject.name + " IS WAITING");
        
        }

        return tforce * distweight * dirweight;
    }


    void Start()
    {
        pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        // Debug.Log("Position: " + pos);
        rb = this.GetComponent<Rigidbody2D>();
        fieldOfView = GetComponent<FieldOfView>();
        agentCollisionHandler = this.GetComponent<AgentCollisionHandler>();
        agentCollider = GetComponent<CircleCollider2D>();
        lastPos = transform.position;
        radius = agentCollider.radius;
        if (isFallen) 
            AgentFalls();
    }


    Vector2 lastPos;

    // Update is called once per frame
    void FixedUpdate()
    {
        //Skip looping
        if (isFallen) {
            rb.velocity = Vector2.zero;
            return;
        }
            

        float lambda = 1.0f;

        Vector2 repelForceFromAgents = Vector2.zero;
        Vector2 repelForceFromWalls = Vector2.zero;

        Vector2 currentForce = prevForce;

        Vector2 currentRepelForce = Vector2.zero;

        Vector2 forceAttractor = getDirection(attractor) * attractorWeight;

        currentForce += forceAttractor;

        // Debug.Log("Current force: " + currentForce);

        // vel = rb.velocity;

        // Debug.Log("Current vel: " + vel);

        fieldOfView.FindVisibleTargets();
        //CASE AGENT --------------------------------------------------------------
        foreach (Transform visibleAgent in fieldOfView.visibleAgents) 
        {
            // Debug.Log("AGENT " + gameObject.name + " sees " +visibleAgent.name);
            
            Rigidbody2D visibleAgentRigidbody = visibleAgent.GetComponent<Rigidbody2D>();
            CircleCollider2D visibleAgentCircleCollider = visibleAgent.GetComponent<CircleCollider2D>();
            float visibleAgentRadius = visibleAgentCircleCollider.radius;

            Agent otherAgent = visibleAgent.GetComponent<Agent>();

            Vector3 direction = visibleAgent.position - transform.position;

            float distance = Vector3.Distance(transform.position, visibleAgent.position);

            
            if (distance < R && Vector2.Dot(direction, vel) > 0) //A semi circle
            {
                num_agents_ahead++;
            }



            currentForce += calcAgentForce(visibleAgent) * agentWeight; //NORMAL FORCES


        }
        
        //Repulsion forces with other agents!!!
        foreach(Transform collidedAgent in fieldOfView.collidedAgents)
        {
                // i is this agent, j is the other agent
                // d_ji is the distance between their centers
                // ep is the person
                // formula for agent: (pos_i - pos_j)*(r_i + ep_i + r_j - d_ji)/ d_ji
                Agent otherAgent = collidedAgent.GetComponent<Agent>();
                Rigidbody2D collidedAgentRigidbody = collidedAgent.GetComponent<Rigidbody2D>();
                CircleCollider2D collidedAgentCircleCollider = collidedAgent.GetComponent<CircleCollider2D>();

                // Interact with collided agents
                Vector2 jtoi = transform.position - collidedAgent.position;

                

                float collidedAgentRadius = collidedAgentCircleCollider.radius;
                

                float k = (agentCollider.radius + personalSpace + collidedAgentRadius - jtoi.magnitude) / (jtoi.magnitude);

                repelForceFromAgents += jtoi * k;
        }

        //CASE WALL ----------------------------------------------------------------
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
            }

            Vector2 tempForce = GeometryUtils.CrossAndRecross(wallNorm, rb.velocity);
            tempForce.Normalize();
            tempForce *= wallWeight;
            currentForce += tempForce;

            
        }

        //Repulsion forces with other walls
        foreach( Transform collidedWall in fieldOfView.collidedWalls)
        {
            Debug.Log("COLLIDING WITH WALL");
            float distance = (collidedWall.position - transform.position).magnitude;
            lambda = 0.3f;
            BoxCollider2D wallCollider = collidedWall.GetComponent<BoxCollider2D>();
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

        //CASE FALLEN AGENT -------------------------------------------------------
        foreach (Transform visibleFallenAgent in fieldOfView.visibleFallenAgents)
        {

            //NOT IMPLEMENTED YET
            Vector2 d = (transform.position - visibleFallenAgent.transform.position); // obstacle k to agent i?

            Vector2 tempForce = GeometryUtils.CrossAndRecross(d, rb.velocity);
            tempForce.Normalize();
            tempForce *= fallenWeight;

            Debug.Log("Force: " + tempForce);

            currentForce += tempForce;

            // Debug.Log("HI");

        }

        // gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
        repelForceFromAgents *= lambda;
        repelForce = repelForceFromAgents + repelForceFromWalls;

        if (repelForce.magnitude > repelsionMagThreshold) {
            //FALL DOWN AND DIE!
            Debug.Log(gameObject.name + " HAS FALLEN");
            isFallen = true;
            rb.velocity = Vector2.zero;
            AgentFalls();
        }

        if (Vector2.Dot(vel, repelForceFromAgents) < 0 && !panic)
        {
            Debug.Log("STOPPING");
            stopping = true;
            stoptime = rand.Next(1,150);
            vel = Vector2.zero;
        }

        if (waiting)
        {
            vel = Vector2.zero;
        }

        //CurrentForce is already filled up

        

        currentForce.Normalize(); //Normalize force vector

        prevForce = currentForce;

        // Debug.Log("Normalized force vec: " + currentForce);




        //TIME TO APPLY FORCES
        // if (vel != Vector2.zero) { 

        // }        
        float alpha = computeAlpha();
        // Debug.Log("Alpha: " + alpha);
        Beta = 0; //For now when no fallen agents


        Vector2 fallenAgentVec = Vector2.zero;
        float velMagnitude = computeVel(Time.fixedDeltaTime);
        // velMagnitude = 2.5f;
        // Debug.Log("velMagnitude: " + velMagnitude);
        float moveFactor = alpha*velMagnitude;
        Vector2 move = moveFactor * ((1 - Beta)*currentForce + Beta*fallenAgentVec);
        // Vector2 desiredPosition = move + repelForce;
        // Debug.Log("AGENT " + gameObject.name + " IS MOVING TO " + ((Vector2) (transform.position) + (move ) + repelForce) + " FROM " + (Vector2) (transform.position));

        // Vector2 lastPos = transform.position;
        rb.MovePosition((Vector2) (transform.position) + (move )*Time.fixedDeltaTime + 0.1f*repelForce);
        // rb.MovePosition(Vector2.zero);
        vel = ( (Vector2)transform.position - lastPos ) / Time.fixedDeltaTime; 
        Vector2 dir = vel.normalized;
        float angleInRadians = Mathf.Atan2(dir.y, dir.x);
        float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
        rb.MoveRotation(angleInDegrees); //TODO : Why doesn't this work????
        transform.rotation = Quaternion.Euler(0, 0, angleInDegrees);
        // Debug.Log("angle " + rb.rotation);
        // Debug.Log("Pos: " + transform.position + ", last pos: " + lastPos + ", deltaT: " + Time.fixedDeltaTime); 
        lastPos = transform.position;
        
        rb.velocity = vel; 

        

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
        repelForce = Vector2.zero;
        // visObjects.Clear();
        // collideObjects.Clear();
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

    private Vector2 _norm;
    public Vector2 norm
    {
        get => _norm;
        set => _norm = value;
    }

    private ObjType _myType;
    public ObjType myType
    {
        get => _myType;
        set => _myType = value;
    }

    // Weights - avoidance weights
    public float attractorWeight = 1;
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

    // States whether an agent is colliding with another
    // bool isColliding = false;
    // List<CrowdObject> collideObjects = new List<CrowdObject>();

    // // Lists of visible objects
    // // Should probably be pointers, will change when needed 
    // List<CrowdObject> visObjects = new List<CrowdObject>();

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
    public float viswide = 2.0f;

    // Attractor
    public Vector2 attractor;

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


    public Vector2 getDirection(Vector2 pos)
    {
        // Implementation goes here
        return pos - (Vector2)transform.position;
    }

    public Vector2 getPos()
    {
        // Implementation goes here
        return _pos;
    }

    public Vector2 getVelocity()
    {
        // Implementation goes here
        return vel;
    }

    public float getRadius()
    {
        // Implementation goes here
        return radius;
    }

}