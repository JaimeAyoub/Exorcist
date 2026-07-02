using UnityEngine;

public class NoteInteractable : Interactable
{

    public TextAsset noteText;
 

    public override void Interact()
    {
       Debug.Log(noteText.text);
    }
}
