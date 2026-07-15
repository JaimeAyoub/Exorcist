using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NoteInteractable : Interactable
{
    public TextAsset noteText;

    public MenuController menuController;

    public PlayerInputHandler playerInputHandler;

    private InputAction _closection;


    private void Start()
    {
        _closection = InputSystem.actions.FindAction("CloseNote");
    }


    private void Update()
    {
        if (_closection.WasPerformedThisFrame())
        {
            CloseNote();
            Debug.Log("Close Note");
        }
    }

    public override void Interact()
    {
        if (menuController)
        {
            if (playerInputHandler != null)
            {
                menuController.PushNotePage(noteText.text);
                playerInputHandler.SetNote();
                GameManager.Instance.EnableCursor();
            }
        }
    }

    private void CloseNote()
    {
        menuController.PopPage();
        playerInputHandler.SetGameplay();
        GameManager.Instance.DisableCursor();
    }
}