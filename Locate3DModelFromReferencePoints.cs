using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locate3DModelFromReferencePoints 
{
    public static (Vector3, Quaternion) GetTranslateAndRotateFromThreePoints(List<Vector3> localPoints, List<Vector3> globalPoints)
    {
        if (localPoints.Count < 3)
        {
            Debug.LogWarning("At least 2 points are required");
        }
        if (localPoints.Count != globalPoints.Count)
        {
            Debug.LogWarning("The GPS points and local Points are not match!");
            return (new Vector3(), new Quaternion());
        }
        // a-0th, b-1st, c-2nd
        Vector3 local_d = PerpendicularPoint(localPoints[0], localPoints[1], localPoints[2]);
        Vector3 local_forward = local_d - localPoints[0]; local_forward.Normalize();
        Vector3 local_upward = local_d - localPoints[1]; local_upward.Normalize();
        Quaternion localRot = Quaternion.LookRotation(local_forward, local_upward);
        Matrix4x4 localPointTRS = Matrix4x4.TRS(local_d, localRot, Vector3.one);


        Vector3 global_d = PerpendicularPoint(globalPoints[0], globalPoints[1], globalPoints[2]);
        Vector3 global_forward = global_d - globalPoints[0]; global_forward.Normalize();
        Vector3 global_upward = global_d - globalPoints[1]; global_upward.Normalize();
        Quaternion globalRot = Quaternion.LookRotation(global_forward, global_upward);
        Matrix4x4 globalPointTRS = Matrix4x4.TRS(global_d, globalRot, Vector3.one);

        Matrix4x4 modelTRS = globalPointTRS * localPointTRS.inverse;



        Vector3 modelTrans = ExtractTranslationFromMatrix(ref modelTRS);
        Quaternion modelRot = ExtractRotationFromMatrix(ref modelTRS);
        return (modelTrans, modelRot);
    }

    static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    /// <summary>
    /// Extract rotation quaternion from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Quaternion representation of rotation transform.
    /// </returns>
    static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
        return Quaternion.LookRotation(forward, upwards);
    }

    /// <summary>
    /// Find the point d that:
    /// Lies on line bc
    /// ad is perpendicular to bc
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    static Vector3 PerpendicularPoint(Vector3 a, Vector3 b, Vector3 c)
    {
        float dx(float t)
        {
            return b.x + ((c.x - b.x) * t);
        }
        float dy(float t)
        {
            return b.y + ((c.y - b.y) * t);
        }
        float dz(float t)
        {
            return b.z + ((c.z - b.z) * t);
        }

        float leftSide = -(
            ((b.x - a.x) * (c.x - b.x)) +
            ((b.y - a.y) * (c.y - b.y)) +
            ((b.z - a.z) * (c.z - b.z))
            );
        float rightSize = (
            ((c.x - b.x) * (c.x - b.x)) +
            ((c.y - b.y) * (c.y - b.y)) +
            ((c.z - b.z) * (c.z - b.z))
            );
        float t = leftSide / rightSize;
        Debug.Log("t: " + t);
        return new Vector3(dx(t), dy(t), dz(t));
    }
}
