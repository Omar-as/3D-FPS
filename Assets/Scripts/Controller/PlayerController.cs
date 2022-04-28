using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.iOS;

/// <summary>
/// This class handles the movement of the player with given input from the input manager
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the player moves")]
    public float moveSpeed = 2f;
    
    [Tooltip("The speed at which the player rotates to look left and right(calculated in degrees")]
    public float lookSpeed = 60f;
    
    [Tooltip("The power at which the player jumps")]
    public float jumpPower = 8f;
    
    [Tooltip("The strength of gravity. Its gravity.....")]
    public float gravity = 9.81f;

    [Header("Jump Timing")] 
    public float jumpTimeLeniency = 0.1f;

    private float timeToStopBeingLenient = 0;
    
    
    [Header("Required Referances")]
    [Tooltip("The player shooter script that shoots projectiles")]
    public Shooter playerShooter;

    public Health playerHealth;
    
    public List<GameObject> disableWhileDead;
    
    private bool doubleJumpAvailable = false;

    private CharacterController controller;
    private InputManager inputManager;
    
    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        SetUpCharecterController();
        SetUpInputManager();
    }

    private void SetUpCharecterController()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.Log("The player does not have a player component attached to it!!!");
        }
    }

    void SetUpInputManager()
    {
        inputManager = InputManager.instance;    
    }

    
    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
            foreach (GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(false);
            }
            return;
        }
        else
        {
            foreach (GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(true);
            }
        }
        ProcessesMovement();
        ProcessRotation();
    }

    Vector3 moveDirection;

    void ProcessesMovement()
    {
        // get input from input manager
        float leftRightHorizontal = inputManager.horizontalMoveAxis;
        float forwardBackwardInput = inputManager.verticalMoveAxis;
        bool jumpPressed = inputManager.jumpPressed;
        
        //Handle the control of the player on the ground
        if (controller.isGrounded)
        {
            doubleJumpAvailable = true;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;
            // set the movement direction to be the received input, set y to 0 since we are on the ground
            moveDirection = new Vector3(leftRightHorizontal, 0, forwardBackwardInput);

            // Set the move direction in relation to the transform
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * moveSpeed;

            if (jumpPressed)
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            moveDirection = new Vector3(leftRightHorizontal * moveSpeed, moveDirection.y,
                forwardBackwardInput * moveSpeed);
            moveDirection = transform.TransformDirection(moveDirection);

            if (jumpPressed && Time.time < timeToStopBeingLenient)
            {
                moveDirection.y = jumpPower;
            }
            else if (jumpPressed && doubleJumpAvailable)
            {
                moveDirection.y = jumpPower;
                doubleJumpAvailable = false;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        if (controller.isGrounded &&  moveDirection.y < 0)
        {
            moveDirection.y = -0.3f;
        }
        
        controller.Move(moveDirection * Time.deltaTime);
    }

    void ProcessRotation()
    {
        float horizontalLookInput = inputManager.horizontalLookAxis;
        Vector3 platyerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(platyerRotation.x,platyerRotation.y + horizontalLookInput * lookSpeed * Time.deltaTime,platyerRotation.z));
        
    }
}
