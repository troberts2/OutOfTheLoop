using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float followRange = .1f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    [SerializeField] private float padding = 0.5f; // Offset if your sprite has width/height
    [SerializeField] private float tiltThreshold = 0.1f;
    private Camera mainCam;
    public bool canMove = true;
    [SerializeField] private bool isTiltControl = true;
    private Rigidbody2D rb;
    private CosmeticManager cosmeticManager;

    private int _currentState;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int TakeHit = Animator.StringToHash("TakeHit");
    [SerializeField] private float _takeHitDuration = .25f;
    private static readonly int Die = Animator.StringToHash("die");
    private float _lockedTill;
    private bool _takeHit;
    private bool _die;
    private bool isMoving;

    private void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        cosmeticManager = GetComponent<CosmeticManager>();
    }

    private void OnEnable()
    {
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
        PlayerCollision.OnPlayerHurt += OnPlayerHurt;
        GameManager.OnGameReset += OnGameReset;
        AdManager.OnPlayerContinueReward += OnPlayerContinueAdReward;

        // Enable EnhancedTouch if needed (for touch + sensors)
        EnhancedTouchSupport.Enable();

        // Enable accelerometer
        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
    }

    private void OnDisable()
    {
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
        PlayerCollision.OnPlayerHurt -= OnPlayerHurt;
        GameManager.OnGameReset -= OnGameReset;
        AdManager.OnPlayerContinueReward -= OnPlayerContinueAdReward;

        EnhancedTouchSupport.Disable();
    }

    private void OnPlayerHurt()
    {
        _takeHit = true;
    }

    private void OnPlayerDeath()
    {
        canMove = false;
        playerJoystick.gameObject.SetActive(false);
        //anim
        _die = true;
    }

    private void OnGameReset()
    {
        canMove = true;
        animator.Rebind();
        _lockedTill = Time.time;
        playerJoystick.gameObject.SetActive(true);
    }

    private void OnPlayerContinueAdReward()
    {
        canMove = true;
        animator.Rebind();
        playerJoystick.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }

        if (moveInput.magnitude > 0.1)
        {
            /*animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);*/
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                if (moveInput.x > 0)
                {
                    animator.SetFloat("MoveX", 1);
                }
                else if (moveInput.y < 0)
                {
                    animator.SetFloat("MoveX", -1);
                }

                animator.SetFloat("MoveY", 0);
            }
            else
            {
                if (moveInput.y > 0)
                {
                    animator.SetFloat("MoveY", 1);
                }
                else if (moveInput.y < 0)
                {
                    animator.SetFloat("MoveY", -1);
                }

                animator.SetFloat("MoveX", 0);
            }
            UpdateCosmeticDirection(moveInput, isMoving ? 1 : 0);
        }

        var state = GetState();

        _die = false;
        _takeHit = false;

        if (state == _currentState) return;
        animator.CrossFade(state, 0, 0);
        _currentState = state;
    }

    /*Vector3 worldPos;
    float distToMouse;*/
    Vector3 moveInput;
    [SerializeField] private VirtualJoystick playerJoystick;

    void FixedUpdate()
    {
        switch(GameManager.Instance.movementType)
        {
            case MovementType.FingerFollow:
                TouchPosition();
                break;
            case MovementType.JoyStick:
                JoystickPosition();
                break;
            case MovementType.Tilt:
                TiltPosition();
                break;
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        UpdateBounds();

        pos.x = Mathf.Clamp(pos.x, minBounds.x + padding, maxBounds.x - padding);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + padding, maxBounds.y - padding);

        transform.position = pos;
    }

    void UpdateBounds()
    {
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        minBounds = bottomLeft;
        maxBounds = topRight;
    }

    private void JoystickPosition()
    {
        Vector2 input = playerJoystick.GetInput();
        moveInput = new Vector3(input.x, input.y, 0);
        if(canMove)
        {
            rb.MovePosition(rb.position + (Vector2)moveInput * followSpeed * Time.fixedDeltaTime);
        }

        //animations
        if (moveInput.magnitude >= 0.1f)
            animator.SetFloat("Speed", 1);
        else
            animator.SetFloat("Speed", 0);
        /*if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            if (moveInput.x > 0)
            {
                animator.SetFloat("MoveX", 1);
            }
            else
            {
                animator.SetFloat("MoveX", -1);
            }

            animator.SetFloat("MoveY", 0);
        }
        else
        {
            if (moveInput.y > 0)
            {
                animator.SetFloat("MoveY", 1);
            }
            else
            {
                animator.SetFloat("MoveY", -1);
            }

            animator.SetFloat("MoveX", 0);
        }*/
        UpdateCosmeticDirection(moveInput, moveInput.magnitude);
    }

    Vector3 worldPos;
    float distToMouse;
    bool isHolding = false;
    private void TouchPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.x, touch.y));
            isHolding = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.IsPressed())
        {
            var mouse = Mouse.current.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y));
            isHolding = true;
        }
        else
        {
            isHolding = false;
        }

        if(isHolding)
        {
            worldPos.z = transform.position.z; // Maintain original z position
            distToMouse = Vector2.Distance(transform.position, worldPos);

            Vector2 dir = (worldPos - transform.position).normalized;

            //follow mouse
            if (distToMouse > followRange && canMove)
                rb.MovePosition(rb.position + dir * followSpeed * Time.fixedDeltaTime);

            moveInput = (worldPos - transform.position).normalized;
        }
        else
        {
            distToMouse = 0;
        }
        

        // animations
        if (distToMouse > followRange)
        {
            animator.SetFloat("Speed", 1);
            isMoving = true;
        }
        else
        {
            animator.SetFloat("Speed", 0);
            isMoving = false;
        }

        
    }


    private void TiltPosition()
    {
        if (Accelerometer.current == null)
            return; // device doesn’t support accelerometer

        // Read acceleration (x,y,z)
        Vector3 tilt = Accelerometer.current.acceleration.ReadValue();

        // Subtract calibration so "rest" = neutral
        moveInput = tilt - GameManager.Instance.calibrationOffset;

        // Optional: ignore Z tilt (forward/back angle)
        moveInput.z = 0f;

        // Apply threshold (dead zone)
        if (moveInput.magnitude < tiltThreshold || !canMove)
        {
            moveInput = Vector3.zero;
            return;
        }

        moveInput = moveInput.normalized;

        // Move player proportionally to tilt
        rb.MovePosition(rb.position + (Vector2)moveInput * followSpeed * Time.fixedDeltaTime);

        //animations
        if (moveInput.magnitude >= tiltThreshold)
            animator.SetFloat("Speed", 1);
        else
            animator.SetFloat("Speed", 0);
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            if (moveInput.x > 0)
            {
                animator.SetFloat("MoveX", 1);
            }
            else
            {
                animator.SetFloat("MoveX", -1);
                Debug.Log("Idle left");
            }

            animator.SetFloat("MoveY", 0);
        }
        else
        {
            if (moveInput.y > 0)
            {
                animator.SetFloat("MoveY", 1);
            }
            else
            {
                animator.SetFloat("MoveY", -1);
            }

            animator.SetFloat("MoveX", 0);
        }
        UpdateCosmeticDirection(moveInput, moveInput.magnitude);
    }

    private CosmeticManager.Direction currentDir;
    private void UpdateCosmeticDirection(Vector2 moveInput, float speed)
    {
        // Only update if there is input
        if (moveInput.magnitude > 0.1f)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                // Horizontal movement dominates
                if(moveInput.x > 0)
                {
                    cosmeticManager.SetDirection(CosmeticManager.Direction.Right, moveInput, speed);
                    currentDir = CosmeticManager.Direction.Right;
                }
                else
                {
                    cosmeticManager.SetDirection(CosmeticManager.Direction.Left, moveInput, speed);
                    currentDir = CosmeticManager.Direction.Left;
                }
            }
            else
            {
                // Vertical movement dominates
                if (moveInput.y > 0)
                {
                    cosmeticManager.SetDirection(CosmeticManager.Direction.Up, moveInput, speed);
                    currentDir = CosmeticManager.Direction.Up;
                }
                else
                {
                    cosmeticManager.SetDirection(CosmeticManager.Direction.Down, moveInput, speed);
                    currentDir = CosmeticManager.Direction.Down;
                }
            }
        }
        else
        {
            cosmeticManager.SetDirection(currentDir, Vector3.zero, 0);
        }
    }

    private int GetState()
    {
        if (Time.time < _lockedTill) return _currentState;

        // Priorities
        if (_die) return LockState(Die, 1000f);
        if (_takeHit) return LockState(TakeHit, _takeHitDuration);

        if (!isMoving) return Idle;
        else return Walk;


        int LockState(int s, float t)
        {
            _lockedTill = Time.time + t;
            return s;
        }
    }
}
