using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Wall : MonoBehaviour
{

    public Vector2 wallNormal;

    // Start is called before the first frame update
    void Start()
    {
        // Get the wall's BoxCollider2D component
        BoxCollider2D wallCollider = GetComponent<BoxCollider2D>();

        // Calculate the wall's normal based on its rotation
        float wallRotation = wallCollider.transform.eulerAngles.z;
        float radians = wallRotation * Mathf.Deg2Rad;
        wallNormal = new Vector2(Mathf.Cos(radians + Mathf.PI / 2), Mathf.Sin(radians + Mathf.PI / 2));

        start = new Vector2(transform.position.x + Mathf.Cos(radians) * transform.localScale.x/2, transform.position.y + Mathf.Sin(radians) * transform.localScale.x/2); //TODO : CHECK START AND END

        end = new Vector2(transform.position.x + Mathf.Cos(radians - Mathf.PI) * transform.localScale.x/2, transform.position.y + Mathf.Sin(radians - Mathf.PI) * transform.localScale.x/2);

        Debug.Log("Wall normal: " + wallNormal + ", start: " + start + ", end: " + end);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //two lines in start + direction format along with their lengths
    public bool TestLineIntersection(Vector2 p1, Vector2 d1, float l1, Vector2 p2, Vector2 d2, float l2)
    {
        d1.Normalize();
        d2.Normalize();

        Vector2 delta = p2 - p1;
        float s, t;

        float crossProduct = Vector3.Cross(d1, d2).z;

        if (Mathf.Abs(crossProduct) < Mathf.Epsilon)
        {
            return false;
        }

        s = Vector3.Cross(delta, d2).z / crossProduct;
        t = Vector3.Cross(delta, d1).z / crossProduct;

        if (s <= l1 && s >= 0.0f && t <= l2 && t >= 0.0f)
        {
            return true;
        }

        return false;
    }


    private Vector2 start;
    private Vector2 end;

    public Vector2 pos { get; set; }
    public Vector2 norm { get; set; }
    public ObjType myType { get; set; }

    public Vector2 getStart() => start;

    public Vector2 getPos() => start;

    public Vector2 getEnd() => end;
    public float getRadius() => 0.0f;

    public bool isVisible(Vector2 pos, Vector2 dir, float vislength, float viswidth)
    {
        if (GeometryUtils.PtToLineDist(start, pos, dir, vislength) <= viswidth)
        {
            return true;
        }

        Vector2 walldir = end - start;
        float walllen = walldir.magnitude;
        Vector2 tan = Vector3.Cross(dir, Vector3.forward).normalized;
        Vector2 start1 = pos + tan * viswidth;
        Vector2 start2 = pos - tan * viswidth;
        return
            TestLineIntersection(pos, dir, vislength, start, walldir, walllen) ||
            TestLineIntersection(pos, dir, vislength, start, walldir, walllen);
    }

    public Vector2 getNorm()
    {
        // Implement getNorm functionality
        return norm;
    }

    //find the closest distance from our line to the point
    public float getDistance(Vector2 pos)
    {
        // Implement getDistance functionality
        Vector2 v = getDirection(pos);
        return v.magnitude;
    }

    public Vector2 getDirection(Vector2 pos)
    {
        // Implement getDirection functionality

        Vector2 dir = Vector2.zero;
        Vector2 pmins = Vector2.zero;
        Vector2 closept = Vector2.zero;

        //get wall direction
        dir = end - start;
        float len = dir.magnitude;
        dir.Normalize();

        //find the point along the line closest
        pmins = pos - start;
        float t = Vector2.Dot(dir, pmins);

        //if not part of the line segment, the start or the end is closest
        if (t <= 0)
        {
            return pos - start;
        }

        if (t > len)
        {
            return pos - end;
        }

        //otherwise, use t
        closept = start + dir * t;
        Vector2 res = pos - closept;
        return res;
    }

    public ObjType getType() => myType;

    public Vector2 getVelocity() => Vector2.zero;

    public Vector2 Startw
    {
        get => start;
        set => start = value;
    }

    public Vector2 End
    {
        get => end;
        set => end = value;
    }
}
