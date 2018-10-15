using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{
    [Header("Camera")]
    public Camera mainCamera;
    public bool mouseMovesCamera = true;
    public Vector2 mouseDeadZone = new Vector2(.8f, .8f);
    public float keyboardSpeed = 4f;
    public float mouseSpeed = 2f;

    [Space]

    public LayerMask unitsLayerMask;
    public LayerMask enemiesLayerMask;

    //private Vector3 initialSelectionWorldPos, currentSelectionWorldPos; //world coordinates //currently unused
    private Vector2 LMBDownMousePos, currentMousePos; //screen coordinates
    private Rect selectionRect; //screen coordinates
    private bool LMBClickedDown = false, boxSelectionInitiated = false;
    private float timeOfClick;

    private const float CLICK_TOLERANCE = .5f; //the player has this time to release the mouse button for it to be registered as a click

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();

#if !UNITY_EDITOR
        //to restore the mouseMovesCamera parameter (which in the player has to be always true)
        //in case someone forgot it on false in the Editor :)
        mouseMovesCamera = true;
#endif
    }

    void Update()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameManager.GameMode.Gameplay:
                currentMousePos = Input.mousePosition;

                //-------------- LEFT MOUSE BUTTON DOWN --------------
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    LMBDownMousePos = currentMousePos;
                    timeOfClick = Time.unscaledTime;
                    LMBClickedDown = true;
                }

                //-------------- LEFT MOUSE BUTTON HELD DOWN --------------
                if (LMBClickedDown
                   && Vector2.Distance(LMBDownMousePos, currentMousePos) > .1f)
                {
                    UIManager.Instance.ToggleSelectionRectangle(true);
                    boxSelectionInitiated = true;
                    LMBClickedDown = false; //this will avoid repeating this block every frame
                }

                if (boxSelectionInitiated)
                {
                    //draw the screen space selection rectangle
                    Vector2 rectPos = new Vector2(
                                          (LMBDownMousePos.x + currentMousePos.x) * .5f,
                                          (LMBDownMousePos.y + currentMousePos.y) * .5f);
                    Vector2 rectSize = new Vector2(
                                           Mathf.Abs(LMBDownMousePos.x - currentMousePos.x),
                                           Mathf.Abs(LMBDownMousePos.y - currentMousePos.y));
                    selectionRect = new Rect(rectPos - (rectSize * .5f), rectSize);

                    UIManager.Instance.SetSelectionRectangle(selectionRect);
                }

                //-------------- LEFT MOUSE BUTTON UP --------------
                if (Input.GetMouseButtonUp(0))
                {
                    if (boxSelectionInitiated)
                    {
                        GameManager.Instance.ClearSelection();

                        //consider the mouse release as the end of a box selection
                        IList<Unit> allSelectables = GameManager.Instance.GetAllSelectableUnits();
                        for (int i = 0; i < allSelectables.Count; i++)
                        {
                            Vector2 screenPos = mainCamera.WorldToScreenPoint(allSelectables[i].transform.position);
                            if (selectionRect.Contains(screenPos))
                            {
                                GameManager.Instance.AddToSelection(allSelectables[i]);
                            }
                            else
                            {
                                //GameManager.Instance.RemoveFromSelection(allSelectables[i]); //Not necessary anymore, selection is cleared above
                            }
                        }

                        //hide the box
                        UIManager.Instance.ToggleSelectionRectangle(false);
                    }
                    else
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            GameManager.Instance.ClearSelection();
                        }

                        if (Time.unscaledTime < timeOfClick + CLICK_TOLERANCE)
                        {
                            //consider the mouse release as a click
                            RaycastHit hit;
                            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitsLayerMask))
                            {
                                Unit newSelectedUnit = hit.collider.GetComponent<Unit>();
                                if (newSelectedUnit != null && newSelectedUnit.template.faction == GameManager.Instance.faction)
                                {
                                    GameManager.Instance.SetSelection(newSelectedUnit);
                                }
                            }
                        }
                    }

                    LMBClickedDown = false;
                    boxSelectionInitiated = false;
                }

                //-------------- RIGHT MOUSE BUTTON UP --------------
                if (Input.GetMouseButtonDown(1)
                    && GameManager.Instance.GetSelectionLength() > 0
                    && !EventSystem.current.IsPointerOverGameObject())
                {
                    RaycastHit hit;
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitsLayerMask))
                    {
                        Unit targetOfAttack = hit.collider.GetComponent<Unit>();
                        if (targetOfAttack != null && targetOfAttack.template.faction != GameManager.Instance.faction)
                        {
                            GameManager.Instance.AttackTarget(targetOfAttack);
                            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                        }
                    }
                    else
                    {
                        Vector3 commandPoint;
                        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, Input.mousePosition, out commandPoint);
                        GameManager.Instance.SentSelectedUnitsTo(commandPoint);
                        Debug.DrawLine(ray.origin, commandPoint, Color.green, 1f);
                    }
                }

                //-------------- GAMEPLAY CAMERA MOVEMENT --------------
                if (!boxSelectionInitiated)
                {
                    Vector2 amountToMove = new Vector2(0f, 0f);
                    bool mouseIsMovingCamera = false;
                    bool keyboardIsMovingCamera = false;

                    //This check doesn't allow the camera to move with the mouse if we're currently framing a platoon
                    if (mouseMovesCamera
                        && !CameraManager.Instance.IsFramingPlatoon)
                    {
                        Vector3 mousePosition = Input.mousePosition;
                        mousePosition.x -= Screen.width / 2f;
                        mousePosition.y -= Screen.height / 2f;

                        //horizontal
                        float horizontalDeadZone = Screen.width * mouseDeadZone.x;
                        float absoluteXValue = Mathf.Abs(mousePosition.x);
                        if (absoluteXValue > horizontalDeadZone)
                        {
                            //camera needs to move horizontally
                            amountToMove.x = (absoluteXValue - horizontalDeadZone) * Mathf.Sign(mousePosition.x) * .01f * mouseSpeed;
                            mouseIsMovingCamera = true;
                        }

                        //vertical
                        float verticalDeadZone = Screen.height * mouseDeadZone.y;
                        float absoluteYValue = Mathf.Abs(mousePosition.y);
                        if (absoluteYValue > verticalDeadZone)
                        {
                            //camera needs to move horizontally
                            amountToMove.y = (absoluteYValue - verticalDeadZone) * Mathf.Sign(mousePosition.y) * .01f * mouseSpeed;
                            mouseIsMovingCamera = true;
                        }
                    }

                    //Keyboard movements only happen if mouse is not causing the camera to move already
                    if (!mouseIsMovingCamera)
                    {
                        float horKeyValue = Input.GetAxis("Horizontal");
                        float vertKeyValue = Input.GetAxis("Vertical");
                        if (horKeyValue != 0f || vertKeyValue != 0f)
                        {
                            amountToMove = new Vector2(horKeyValue, vertKeyValue) * keyboardSpeed;
                            keyboardIsMovingCamera = true;
                        }
                    }

                    if (mouseIsMovingCamera || keyboardIsMovingCamera)
                    {
                        CameraManager.Instance.MoveGameplayCamera(amountToMove * .5f);
                    }
                }
                break;

            case GameManager.GameMode.Cutscene:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    GameManager.Instance.ResumeTimeline();
                }
                break;
        }
    }


}
