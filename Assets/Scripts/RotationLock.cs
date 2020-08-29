using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLock : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
