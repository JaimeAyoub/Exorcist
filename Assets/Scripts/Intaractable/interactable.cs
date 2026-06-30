using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string messageToShow = "Press E to Interact";


    public abstract void Interact();
}