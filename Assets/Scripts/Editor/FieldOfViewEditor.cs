using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private FieldOfView fow;

    void OnEnable()
    {
        fow = (target as FieldOfView);
    }

    public void OnSceneGUI()
    {
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360f, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2f, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2f, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        //Handles.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
        //foreach (Transform visibleTarget in fow.lastVisibleTargets)
        //{
        //    Handles.DrawLine(fow.transform.position, visibleTarget.transform.position);
        //}
    }
}
