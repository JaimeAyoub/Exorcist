using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonNavigation : MonoBehaviour
{
    [SerializeField] private MenuController menuController;
    
    [SerializeField] private bool isBackButton = false;
    [SerializeField] private Page pageToOpen;

    [SerializeField] private GameObject nextButtonToFocus;
    [SerializeField] private CanvasGroup currentMenuCanvasGroup;

    
    [SerializeField] private CanvasGroup returnMenuCanvasGroup;

    public void ExecuteNavigation()
    {
        if (currentMenuCanvasGroup != null)
        {
            currentMenuCanvasGroup.interactable = false;
            currentMenuCanvasGroup.blocksRaycasts = false;
        }

        if (isBackButton)
        {
            menuController.PopPage(); 

            if (returnMenuCanvasGroup != null)
            {
                returnMenuCanvasGroup.interactable = true;
                returnMenuCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            if (pageToOpen != null)
            {
                CanvasGroup nextGroup = pageToOpen.GetComponent<CanvasGroup>();
                if (nextGroup != null)
                {
                    nextGroup.interactable = true;
                    nextGroup.blocksRaycasts = true;
                }
                menuController.PushPage(pageToOpen); 
            }
        }

        if (nextButtonToFocus != null)
        {
            EventSystem.current.SetSelectedGameObject(null); 
            EventSystem.current.SetSelectedGameObject(nextButtonToFocus); 
        }
    }
}