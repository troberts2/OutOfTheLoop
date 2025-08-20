using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float followRange = .1f;
    [SerializeField] private Animator animator;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    [SerializeField] private float padding = 0.5f; // Offset if your sprite has width/height
    [SerializeField] private float tiltThreshold = 0.1f;
    private Camera mainCam;
    private bool canMove = true;
    [SerializeField] private bool isTiltControl = true;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
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
        GameManager.OnGameReset -= OnGameReset;
        AdManager.OnPlayerContinueReward -= OnPlayerContinueAdReward;

        EnhancedTouchSupport.Disable();
    }

    private void OnPlayerDeath()
    {
        canMove = false;
        playerJoystick.gameObject.SetActive(false);
    }

    private void OnGameReset()
    {
        canMove = true;
        animator.Rebind();
        playerJoystick.gameObject.SetActive(true);
    }

    private void OnPlayerContinueAdReward()
    {
        canMove = true;
        animator.Rebind();
        playerJoystick.gameObject.SetActive(true);
    }

    /*Vector3 worldPos;
    float distToMouse;*/
    Vector3 moveInput;
    [SerializeField] private VirtualJoystick playerJoystick;

    void Update()
    {
        if (GameManager.Instance.isTiltControls)
        {
            TiltPosition();
            //animations
            if (moveInput.magnitude >= tiltThreshold)
                animator.SetFloat("Speed", 1);
            else
                animator.SetFloat("Speed", 0);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
        else
        {
            TouchPosition();
            //animations
            if (moveInput.magnitude >= 0.1f)
                animator.SetFloat("Speed", 1);
            else
                animator.SetFloat("Speed", 0);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
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

    private void TouchPosition()
    {
        /*if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.x, touch.y));
        }
        else if (Mouse.current != null)
        {
            var mouse = Mouse.current.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y));
        }

        worldPos.z = transform.position.z; // Maintain original z position
        distToMouse = Vector2.Distance(transform.position, worldPos);

        //follow mouse
        if (distToMouse > followRange && canMove)
            transform.position = Vector3.MoveTowards(transform.position, worldPos, followSpeed * Time.deltaTime);

        moveInput = (worldPos - transform.position).normalized;*/
        Vector2 input = playerJoystick.GetInput();
        moveInput = new Vector3(input.x, input.y, 0);
        if(canMove)
        {
            transform.position += moveInput * followSpeed * Time.deltaTime;
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
        transform.position += moveInput * followSpeed * Time.deltaTime;
    }
}
