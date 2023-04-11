using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using System.IO;

public enum ObjType { AGENT, WALL, OBSTACLE, FALLEN_AGENT};

public interface CrowdObject
{


    Vector2 pos { get; set; }
    Vector2 norm { get; set; }
    ObjType myType { get; set; }    


    // Returns whether the object is visible within the vision rectangle presented
    public bool isVisible(Vector2 pos, Vector2 dir, float vislength, float viswidth);

    // Returns the norm of the object in the return parameter
    public Vector2 getNorm();

    // Gets the distance from the position argument to the object
    public float getDistance(Vector2 pos);

    // Gets the direction vector from the calling position to the object
    public Vector2 getDirection(Vector2 pos);

    // Gets the type of object
    public ObjType getType();

    // Returns a zero-vector for all non-agent CrowdObjects
    public Vector2 getVelocity();

    // Functions needed by Agent - leaky abstractions
    public Vector2 getPos();

    public float getRadius();

}
