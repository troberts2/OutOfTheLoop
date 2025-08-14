using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float followRange = .1f;
    [SerializeField] private Animator animator;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    [SerializeField] private float padding = 0.5f; // Offset if your sprite has width/height
    private Camera mainCam;
    private bool canMove = true;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
        GameManager.OnGameReset += OnGameReset;
        AdManager.OnPlayerContinueReward += OnPlayerContinueAdReward;
    }

    private void OnDisable()
    {
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
        GameManager.OnGameReset -= OnGameReset;
        AdManager.OnPlayerContinueReward -= OnPlayerContinueAdReward;
    }

    private void OnPlayerDeath()
    {
        canMove = false;
    }

    private void OnGameReset()
    {
        canMove = true;
        animator.Rebind();
    }

    private void OnPlayerContinueAdReward()
    {
        canMove = true;
        animator.Rebind();
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z; // Maintain original z position
        float distToMouse = Vector2.Distance(transform.position, mouseWorldPos);

        //follow mouse
        if(distToMouse > followRange && canMove)
            transform.position = Vector3.Lerp(transform.position, mouseWorldPos, followSpeed * Time.deltaTime);

        //animations
        Vector2 moveInput = (mouseWorldPos - transform.position).normalized;
        animator.SetFloat("Speed", distToMouse);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
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
}
