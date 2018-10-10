using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that makes the gameobject rotate towards main camera, like a billboard
/// </summary>
public class LookToCam : MonoBehaviour
{
    private void Update()
    {
        Quaternion oldRot = transform.rotation;
        transform.LookAt(Camera.main.transform, Vector3.up);
        transform.rotation = Quaternion.Euler(new Vector3(oldRot.eulerAngles.x, transform.rotation.eulerAngles.y, oldRot.eulerAngles.z));
    }
}
