using UnityEngine;
using TMPro;

public class NoteInteractable : Interactable
{

    public TextAsset noteText;
    
    public TextMeshProUGUI noteTextUI;
    
    public MenuController menuController;
    
    public Page page;
 

    public override void Interact()
    {
       Debug.Log(noteText.text);

       Cursor.lockState = CursorLockMode.None;
       Cursor.visible = true;
       if (noteTextUI != null)
       {
           if (menuController)
           {
               menuController.PushPage(page);
           }
           noteTextUI.text = noteText.text;
       }
    }
}
