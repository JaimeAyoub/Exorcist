using System;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Animator animator;


    private void OnEnable()
    {
        PlayerInputHandler.MovementEvent += OnMove;
        PlayerInputHandler.StopMovementEvent += OnStop;
    }

    private void OnDisable()
    {
        PlayerInputHandler.MovementEvent -= OnMove;
        PlayerInputHandler.StopMovementEvent -= OnStop;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMove()
    {
        // aquí decides si caminar o correr
        if (playerInputHandler.SprintTriggered)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void OnStop()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
    }
}