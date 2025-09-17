using UnityEngine;

public class Note : MonoBehaviour
{
    public Sprite noteImage; // La imagen de la nota que se mostrará

    public void ShowNote()
    {
        if (UIManager.Instance != null && noteImage != null)
        {
            UIManager.Instance.ShowNote(noteImage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.ShowTextDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.HideTextDoor();
        }
    }
}