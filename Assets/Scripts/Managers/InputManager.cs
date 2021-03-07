using System.Collections.Generic;
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
    public float cameraMovementSpeed;
    public float cameraMovementTime;
    public float cameraRotationSpeed;
    public float cameraZoomSpeed;

    [Space]

    public LayerMask unitsLayerMask;
    public LayerMask enemiesLayerMask;
    public LayerMask groundLayerMask;

    [Space]

    public GameObject movementOrderCursor;
    public Color moveCommandColor = Color.green;
    public Color attackMoveCommandColor = Color.red;

    private Vector2 LMBDownMousePos, currentMousePos; //screen coordinates
    private Rect selectionRect; //screen coordinates
    private bool LMBClickedDown, boxSelectionInitiated;
    private float timeOfClick, scrollDelta;

    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    private const float CLICK_TOLERANCE = .5f; //the player has this time to release the mouse button for it to be registered as a click

    public bool buildingPlacementInitiated { get; set; }

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();
        }
    }

    void Start()
    {
        GameObject movementOrderCursorInstance = Instantiate<GameObject>(movementOrderCursor);
        movementOrderCursor = movementOrderCursorInstance;
    }

    void Update()
    {
        GameManager gameManager = GameManager.Instance;
        UIManager uiManager = UIManager.Instance;
        CameraManager cameraManager = CameraManager.Instance;
#if UNITY_EDITOR
        float outsideBorderThickness = 100f;
        mouseMovesCamera = new Rect(-outsideBorderThickness, -outsideBorderThickness, Screen.width + outsideBorderThickness, Screen.height + outsideBorderThickness).Contains(Input.mousePosition);
#endif
        switch (gameManager.gameMode)
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
                    && !buildingPlacementInitiated
                    && Vector2.Distance(LMBDownMousePos, currentMousePos) > .1f)
                {
                    uiManager.ToggleSelectionRectangle(true);
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

                    uiManager.SetSelectionRectangle(selectionRect);
                }

                //-------------- LEFT MOUSE BUTTON UP --------------
                if (Input.GetMouseButtonUp(0))
                {
                    if (boxSelectionInitiated)
                    {
                        if (!Input.GetButton("AddToSelection"))
                        {
                            gameManager.ClearSelection();
                        }

                        //consider the mouse release as the end of a box selection
                        List<Unit> allSelectables = new List<Unit>(gameManager.playerFaction.data.units);
                        for (int i = allSelectables.Count - 1; i >= 0; i--)
                        {
                            Vector2 screenPos = mainCamera.WorldToScreenPoint(allSelectables[i].transform.position);
                            if (!selectionRect.Contains(screenPos))
                            {
                                allSelectables.RemoveAt(i);
                            }
                        }
                        if (allSelectables.Count > 0)
                        {
                            gameManager.AddToSelection(allSelectables);
                        }

                        //hide the box
                        uiManager.ToggleSelectionRectangle(false);
                    }
                    else if (buildingPlacementInitiated)
                    {
                    }
                    else
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            gameManager.ClearSelection();
                        }

                        if (Time.unscaledTime < timeOfClick + CLICK_TOLERANCE)
                        {
                            //consider the mouse release as a click
                            RaycastHit hit;
                            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitsLayerMask))
                            {
                                ClickableObject newSelectedUnit = hit.collider.GetComponent<ClickableObject>();
                                if (newSelectedUnit != null && newSelectedUnit.faction == gameManager.playerFaction)
                                {
                                    gameManager.SetSelection(newSelectedUnit);
                                }
                                else if (newSelectedUnit != null)//TEST
                                {
                                    gameManager.SetSelection(newSelectedUnit);
                                }
                            }
                        }
                    }

                    LMBClickedDown = false;
                    boxSelectionInitiated = false;
                }

                //-------------- RIGHT MOUSE BUTTON DOWN --------------
                if (Input.GetMouseButtonDown(1)
                    && gameManager.GetSelectionLength() > 0
                    && !(gameManager.GetSelectionLength() == 1 && gameManager.selectionOnType == GameManager.SelectionOnType.Units && gameManager.GetPlattoonUnits()[0].faction != gameManager.playerFaction)
                    && !EventSystem.current.IsPointerOverGameObject() && !boxSelectionInitiated && !buildingPlacementInitiated)
                {
                    RaycastHit hit;
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                    bool moveCommand = false;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitsLayerMask))
                    {
                        InteractableObject targetUnit = hit.collider.GetComponent<InteractableObject>();
                        if (targetUnit != null)
                        {
                            bool followUpCommand = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                            bool forceCommand = Input.GetKey(KeyCode.LeftControl);
                            bool attackCommand = Input.GetButton("Attack");
                            if (targetUnit.GetType() == typeof(InteractableObject) || FactionTemplate.IsAlliedWith((targetUnit as ClickableObject).faction, gameManager.playerFaction))
                            {
                                if (forceCommand && attackCommand)
                                {
                                    gameManager.AttackTarget(targetUnit, followUpCommand);
                                    Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                                }
                                else
                                {
                                    gameManager.CustomActionOnTarget(targetUnit, followUpCommand);
                                    Debug.DrawLine(ray.origin, hit.point, Color.yellow, 1f);
                                }
                            }
                            else
                            {
                                gameManager.AttackTarget(targetUnit, followUpCommand);
                                Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                            }
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
                        if (CameraManager.GetCameraScreenPointOnGround(mainCamera, Input.mousePosition, out hitPoint, groundLayerMask))
                        {
                            if (gameManager.selectionOnType == GameManager.SelectionOnType.Units)
                            {
                                bool followUpCommand = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                                bool attackComand = Input.GetButton("Attack");
                                if (attackComand)
                                {
                                    gameManager.AttackMoveSelectedUnitsTo(hitPoint, followUpCommand);
                                    Debug.DrawLine(ray.origin, hitPoint, Color.Lerp(Color.yellow, Color.red, 0.6f), 1f);
                                    AnimateMoveOrderCursor(hitPoint, attackMoveCommandColor);
                                }
                                else
                                {
                                    gameManager.MoveSelectedUnitsTo(hitPoint, followUpCommand);
                                    Debug.DrawLine(ray.origin, hitPoint, Color.green, 1f);
                                    AnimateMoveOrderCursor(hitPoint, moveCommandColor);
                                }
                            }
                            else
                            {
                                gameManager.SetWaypoint(hitPoint);
                                Debug.DrawLine(ray.origin, hitPoint, Color.yellow, 1f);
                                AnimateMoveOrderCursor(hitPoint, moveCommandColor);
                            }
                        }
                    }
                }

                //-------------- GAMEPLAY CAMERA MOVEMENT --------------
                if (!boxSelectionInitiated)
                {
                    Vector3 amountToMove = Vector3.zero;
                    Quaternion amountToRotate = Quaternion.identity;
                    float amountToZoom = 0f;
                    bool mouseIsMovingCamera = false;
                    bool keyboardIsMovingCamera = false;

                    if (mouseMovesCamera)
                    {
                        //This check doesn't allow the camera to move with the mouse if we're currently framing a platoon
                        if (Application.isFocused && !cameraManager.isFramingPlatoon)
                        {
                            if (Input.mousePosition.x >= Screen.width - panBorderThickness)
                            {
                                amountToMove.x += cameraMovementSpeed;
                                mouseIsMovingCamera = true;
                            }
                            if (Input.mousePosition.x <= panBorderThickness)
                            {
                                amountToMove.x -= cameraMovementSpeed;
                                mouseIsMovingCamera = true;
                            }

                            if (Input.mousePosition.y >= Screen.height - panBorderThickness)
                            {
                                amountToMove.z += cameraMovementSpeed;
                                mouseIsMovingCamera = true;
                            }
                            if (Input.mousePosition.y <= panBorderThickness)
                            {
                                amountToMove.z -= cameraMovementSpeed;
                                mouseIsMovingCamera = true;
                            }

                            if (Input.GetMouseButtonDown(2))
                            {
                                Ray ray = cameraManager.gameplayCamera.ScreenPointToRay(Input.mousePosition);
                                float entry;
                                if (CameraManager.groundPlane.Raycast(ray, out entry))
                                {
                                    dragStartPosition = ray.GetPoint(entry);
                                }
                            }
                            if (Input.GetMouseButton(2))
                            {
                                Ray ray = cameraManager.gameplayCamera.ScreenPointToRay(Input.mousePosition);
                                float entry;
                                if (CameraManager.groundPlane.Raycast(ray, out entry))
                                {
                                    dragCurrentPosition = ray.GetPoint(entry);

                                    amountToMove = dragStartPosition - dragCurrentPosition;
                                    cameraManager.MoveGameplayCameraTo(cameraManager.gameplayCamera.transform.parent.position + amountToMove);
                                }
                            }
                        }
                        scrollDelta = Input.mouseScrollDelta.y;
                        float deadZone = 0.01f;
                        if (scrollDelta < -deadZone || scrollDelta > deadZone)
                        {
                            amountToZoom -= scrollDelta * 10f * cameraZoomSpeed;
                            mouseIsMovingCamera = true;
                        }
                    }

                    if (!mouseIsMovingCamera)
                    {
                        //Keyboard movements only happen if mouse is not causing the camera to move already
                        float horKeyValue = Input.GetAxis("Horizontal");
                        float vertKeyValue = Input.GetAxis("Vertical");
                        if (horKeyValue != 0f || vertKeyValue != 0f)
                        {
                            amountToMove = new Vector3(horKeyValue, 0f, vertKeyValue) * cameraMovementSpeed;
                            keyboardIsMovingCamera = true;
                        }

                        float rotKeyValue = 0f;
                        if (Input.GetKey(KeyCode.Q))
                        {
                            rotKeyValue = -1f;
                        }
                        if (Input.GetKey(KeyCode.E))
                        {
                            rotKeyValue = 1f;
                        }
                        if (rotKeyValue != 0f)
                        {
                            amountToRotate = Quaternion.Euler(Vector3.up * rotKeyValue * cameraRotationSpeed);
                            keyboardIsMovingCamera = true;
                        }

                        if (Input.GetKey(KeyCode.R))
                        {
                            amountToZoom -= cameraZoomSpeed;
                            keyboardIsMovingCamera = true;
                        }
                        if (Input.GetKey(KeyCode.F))
                        {
                            amountToZoom += cameraZoomSpeed;
                            keyboardIsMovingCamera = true;
                        }
                    }

                    if (mouseIsMovingCamera || keyboardIsMovingCamera)
                    {
                        cameraManager.MoveGameplayCamera(amountToMove);
                        cameraManager.RotateGameplayCamera(amountToRotate);
                        cameraManager.ZoomGameplayCamera(amountToZoom);
                    }
                }
                scrollDelta = Mathf.MoveTowards(scrollDelta, 0f, Time.deltaTime);
                break;

            case GameManager.GameMode.Cutscene:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    gameManager.ResumeTimeline();
                }
                break;
        }
    }

    public void AnimateMoveOrderCursor(Vector3 point, Color color)
    {
        MovementCursor mover = movementOrderCursor.GetComponentInChildren<MovementCursor>();
        mover.AnimateOnPos(point, color);
    }
}
