using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;

    [Header("Look Parameters")]
    [SerializeField] public float xMouseSensitivity = 0.1f;
    [SerializeField] public float yMouseSensitivity = 1.0f;
    [SerializeField] private float upDownLookRange = 80f;

    [Header("Footstep Parameters")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.3f;
    [SerializeField] private float footstepVolume = 0.7f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private GameObject cameraHolder;

    private Vector2 alignedRotation;
    private Vector3 currentMovement;
    private float verticalRotation;
    private float stepTimer;
    private bool wasGrounded;

    private float CurrentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1);
    private float CurrentStepInterval => playerInputHandler.SprintTriggered ? sprintStepInterval : walkStepInterval;

    //INTEGRAR CINEMACHINE
    public CinemachineCamera mainCamera;

    void Awake()
    {
        alignedRotation = new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        stepTimer = 0f;
        wasGrounded = characterController.isGrounded;
    }

    void Update()
    {
        if (!Application.isPlaying || CombatManager.instance.isCombat) return;

        HandleMovement();
        HandleRotation();
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (characterController.isGrounded && IsMoving())
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= CurrentStepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = CurrentStepInterval * 0.8f; // Reset parcial del timer
        }
    }

    private void PlayFootstepSound()
    {
        AudioManager.instance.PlaySFXRandom(SoundType.PASOS, 0.1f, 0.3f, footstepVolume);
    }

    private bool IsMoving()
    {
        return playerInputHandler.MovementInput.magnitude > 0.1f && characterController.velocity.magnitude > 0.1f;
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if (playerInputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        currentMovement.x = worldDirection.x * CurrentSpeed;
        currentMovement.z = worldDirection.z * CurrentSpeed;

        HandleJumping();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void ApplyHorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }

    private void ApplyVerticalRotation(float rotationAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        cameraHolder.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        float mouseXRotation = playerInputHandler.RotationInput.x * xMouseSensitivity * Time.deltaTime;
        float mouseYRotation = playerInputHandler.RotationInput.y * yMouseSensitivity * Time.deltaTime;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    public bool IsMove()
    {
        return currentMovement.magnitude > 0.1f;
    }
}