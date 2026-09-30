using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string messageToShow = "Press E to Interact";
    public event Action<string> OnMessageChanged;


    public abstract void Interact();

    protected void RaiseMessageChanged(string message)
    {
        messageToShow = message;
        OnMessageChanged?.Invoke(messageToShow);
    }
}