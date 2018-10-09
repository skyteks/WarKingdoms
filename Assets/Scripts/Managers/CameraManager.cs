using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public Transform gameplayDummy;

    private bool isFramingPlatoon = false;
    public bool IsFramingPlatoon { get { return isFramingPlatoon; } } //from the outside it's read-only


    public void MoveGameplayCamera(Vector2 amount)
    {
        gameplayDummy.Translate(amount.x, 0f, amount.y, Space.World);
    }
}