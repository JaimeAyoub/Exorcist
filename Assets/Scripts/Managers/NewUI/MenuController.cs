using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    [SerializeField] private Page initialPage;
    [SerializeField] private GameObject firstFocusItem;

    private Canvas _rootCanvas;

    private readonly Stack<Page> _pageStack = new Stack<Page>();

    private void Awake()
    {
        _rootCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        if (firstFocusItem != null)
        {
            EventSystem.current.SetSelectedGameObject(firstFocusItem);
        }

        if (initialPage != null)
        {
            PushPage(initialPage);
        }
    }

    private void OnCancel()
    {
        if (_rootCanvas.enabled && _rootCanvas.gameObject.activeInHierarchy)
        {
            if (_pageStack.Count != 0)
            {
                PopPage();
            }
        }
    }

    public bool IsPageInStack(Page page)
    {
        return _pageStack.Contains(page);
    }

    public bool IsPageOnTopOfStack(Page page)
    {
        return _pageStack.Count > 0 && page == _pageStack.Peek();
    }

    public void PushPage(Page page)
    {
        page.Enter(true);

        if (_pageStack.Count > 0)
        {
            var currentPage = _pageStack.Peek();

            if (currentPage.exitOnNewPagePush)
            {
                currentPage.Exit(false);
            }
        }

        _pageStack.Push(page);
    }

    private void PopPage()
    {
        if (_pageStack.Count > 1)
        {
            var page = _pageStack.Pop();
            page.Exit(true);

            var newCurrentPage = _pageStack.Peek();
            if (newCurrentPage.exitOnNewPagePush)
            {
                newCurrentPage.Enter(false);
            }
        }
        else
        {
            Debug.LogWarning("Trying to pop a page but only 1 page remains in the stack!");
        }
    }

    public void PopAllPages()
    {
        for (int i = 1; i < _pageStack.Count; i++)
        {
            PopPage();
        }
    }
}