using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Rigidbody_Extension
{
    public static void Freeze(this Rigidbody2D rigidbody2D, bool kinematic = true)
    {
        rigidbody2D.velocity = Vector3.zero;
        rigidbody2D.angularVelocity = 0;
        rigidbody2D.isKinematic = kinematic;
    }
}
