using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles input from mouse and keyboard for camera and unit commands
/// </summary>
public class InputManager : Singleton<InputManager>
{
    [Header("Camera")]
    public Camera mainCamera;
    public bool mouseMovesCamera = true;
    public int panBorderThickness = 10;
    public float keyboardPanSpeed = 2f;
    public float mousePanSpeed = 2f;
    public float mouseScrollSpeed = 2f;

    [Space]

    public LayerMask unitsLayerMask;
    public LayerMask enemiesLayerMask;

    //private Vector3 initialSelectionWorldPos, currentSelectionWorldPos; //world coordinates //currently unused
    private Vector2 LMBDownMousePos, currentMousePos; //screen coordinates
    private Rect selectionRect; //screen coordinates
    private bool LMBClickedDown = false, boxSelectionInitiated = false;
    private float timeOfClick, scrollDelta;

    private const float CLICK_TOLERANCE = .5f; //the player has this time to release the mouse button for it to be registered as a click

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();
    }

    void Update()
    {
#if UNITY_EDITOR
        float outsideBorderThickness = 100f;
        mouseMovesCamera = new Rect(-outsideBorderThickness, -outsideBorderThickness, Screen.width + outsideBorderThickness, Screen.height + outsideBorderThickness).Contains(Input.mousePosition);
#endif
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
                        if (!Input.GetButton("AddToSelection"))
                        {
                            GameManager.Instance.ClearSelection();
                        }

                        //consider the mouse release as the end of a box selection
                        List<Unit> allSelectables = GameManager.Instance.GetAllSelectableUnits();
                        for (int i = 0; i < allSelectables.Count; i++)
                        {
                            Vector2 screenPos = mainCamera.WorldToScreenPoint(allSelectables[i].transform.position);
                            if (selectionRect.Contains(screenPos))
                            {
                                GameManager.Instance.AddToSelection(allSelectables[i]);
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
                                if (newSelectedUnit != null && newSelectedUnit.faction == GameManager.Instance.faction)
                                {
                                    GameManager.Instance.SetSelection(newSelectedUnit);
                                }
                            }
                        }
                    }

                    LMBClickedDown = false;
                    boxSelectionInitiated = false;
                }

                //-------------- RIGHT MOUSE BUTTON DOWN --------------
                if (Input.GetMouseButtonDown(1)
                    && GameManager.Instance.GetSelectionLength() > 0
                    && !EventSystem.current.IsPointerOverGameObject())
                {
                    RaycastHit hit;
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                    bool moveCommand = false;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitsLayerMask))
                    {
                        Unit targetOfAttack = hit.collider.GetComponent<Unit>();
                        if (targetOfAttack != null && targetOfAttack.faction != GameManager.Instance.faction)
                        {
                            GameManager.Instance.AttackTarget(targetOfAttack);
                            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                        }
                        else
                        {
                            moveCommand = true;
                        }
                    }
                    else
                    {
                        moveCommand = true;
                    }
                    if (moveCommand)
                    {
                        Vector3 hitPoint;
                        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, Input.mousePosition, out hitPoint);
                        if (!Input.GetButton("Attack"))
                        {
                            GameManager.Instance.MoveSelectedUnitsTo(hitPoint);
                            Debug.DrawLine(ray.origin, hitPoint, Color.green, 1f);
                        }
                        else
                        {
                            GameManager.Instance.AttackMoveSelectedUnitsTo(hitPoint);
                            Debug.DrawLine(ray.origin, hitPoint, Color.Lerp(Color.yellow, Color.red, 0.6f), 1f);
                        }
                    }
                }

                //-------------- GAMEPLAY CAMERA MOVEMENT --------------
                if (!boxSelectionInitiated)
                {
                    Vector3 amountToMove = Vector3.zero;
                    bool mouseIsMovingCamera = false;
                    bool keyboardIsMovingCamera = false;

                    if (mouseMovesCamera)
                    {
                        //This check doesn't allow the camera to move with the mouse if we're currently framing a platoon
                        if (!CameraManager.Instance.isFramingPlatoon)
                        {
                            if (Input.mousePosition.x >= Screen.width - panBorderThickness)
                            {
                                amountToMove.x += Time.deltaTime * mousePanSpeed;
                                mouseIsMovingCamera = true;
                            }
                            if (Input.mousePosition.x <= panBorderThickness)
                            {
                                amountToMove.x -= Time.deltaTime * mousePanSpeed;
                                mouseIsMovingCamera = true;
                            }

                            if (Input.mousePosition.y >= Screen.height - panBorderThickness)
                            {
                                amountToMove.z += Time.deltaTime * mousePanSpeed;
                                mouseIsMovingCamera = true;
                            }
                            if (Input.mousePosition.y <= panBorderThickness)
                            {
                                amountToMove.z -= Time.deltaTime * mousePanSpeed;
                                mouseIsMovingCamera = true;
                            }
                        }

                        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
                        scrollDelta += scrollValue;
                        float deadZone = 0.01f;
                        if (scrollDelta < -deadZone || scrollDelta > deadZone)
                        {
                            amountToMove.y -= scrollDelta * mouseScrollSpeed * 100f * Time.deltaTime;
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
                            amountToMove = new Vector3(horKeyValue, 0f, vertKeyValue) * keyboardPanSpeed;
                            keyboardIsMovingCamera = true;
                        }
                    }

                    if (mouseIsMovingCamera || keyboardIsMovingCamera)
                    {
                        CameraManager.Instance.MoveGameplayCamera(amountToMove);
                    }
                }
                scrollDelta = Mathf.MoveTowards(scrollDelta, 0f, Time.deltaTime);
                break;

            case GameManager.GameMode.Cutscene:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    GameManager.Instance.ResumeTimeline();
                }
                break;
        }
    }


}
