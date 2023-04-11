using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{

    //public static Random rand = new Random();
    /*procedure taken from geometric tools for computer graphics*/
    public static float PtToLineDist(Vector2 pos, Vector2 start, Vector2 dir, float len)
    {
        dir.Normalize();
        Vector2 diff = pos - start;
        float t = Vector2.Dot(dir, diff);

        if (t >= len)
        {
            Vector2 end = start + dir * len;
            diff = pos - end;
            return diff.magnitude;
        }

        if (t <= 0.0f)
        {
            diff = pos - start;
            return diff.magnitude;
        }

        Vector2 end2 = start + dir * t;
        diff = pos - end2;
        return diff.magnitude;
    }

    public static Vector2 CrossAndRecross(Vector2 a, Vector2 b)
    {
        Vector3 a3 = new Vector3(a.x, a.y, 0);
        Vector3 b3 = new Vector3(b.x, b.y, 0);
        Vector3 crossProductResult = Vector3.Cross(Vector3.Cross(a3, b3), a3);
        return new Vector2(crossProductResult.x, crossProductResult.y);
    }
}
