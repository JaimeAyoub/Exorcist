using UnityEngine;

public class KeyInteractable : Interactable
{
    public override void Interact()
    {
        GameManager.Instance.addKey();
        Debug.Log("Llave add");
        Destroy(gameObject);
    }
}