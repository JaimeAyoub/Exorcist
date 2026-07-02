using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    [SerializeField] private Page initialPage;
    [SerializeField] private Page menuPage;
    [SerializeField] private GameObject firstFocusItem;

  //  [SerializeField] private PlayerInputHandler playerInputHandler;

    private Canvas _rootCanvas;

    private readonly Stack<Page> _pageStack = new Stack<Page>();
    [SerializeField, Header("DEBUG - Stack Visualizacion")]
    private List<Page> _stackVisualization = new List<Page>();

    private bool _isPaused = false;

    private void OnEnable()
    {
        //playerInputHandler.PauseEvent += OnPause;
       // playerInputHandler.ResumeEvent += OnCancel;
    }

    private void OnDisable()
    {
        //playerInputHandler.PauseEvent -= OnPause;
        //playerInputHandler.ResumeEvent -= OnCancel;
    }

    private void OnPause()
    {
        _isPaused = !_isPaused;
        if (_isPaused == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
           // playerInputHandler.SetUI();
            if (_pageStack.Contains(menuPage))
            {
                return;
            }
            PushPage(menuPage);

        }

    }

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

        if (initialPage != null && !_pageStack.Contains(initialPage))
        {
            PushPage(initialPage);
        }
    }

    private void OnCancel()
    {
        Debug.Log("OnCancel");
        if (_rootCanvas.enabled && _rootCanvas.gameObject.activeInHierarchy)
        {
            if (_pageStack.Count > 1)
            {
                PopPage();
               // playerInputHandler.SetGameplay();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
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
        if (!_pageStack.Contains(page))
        {
            _pageStack.Push(page);
        }
        UpdateStackVisualisation();
    }

    public void PopPage()
    {
        
        if (_pageStack.Count > 0) 
        {
            var page = _pageStack.Pop();
            page.Exit(true); 

            if (_pageStack.Count > 0)
            {
                var newCurrentPage = _pageStack.Peek();
                if (newCurrentPage.exitOnNewPagePush)
                {
                    newCurrentPage.Enter(false);
                }
            }
        }
        UpdateStackVisualisation();
    }

    public void PopAllPages()
    {

        while (_pageStack.Count > 0)
        {
            PopPage();
        }
    }

    void UpdateStackVisualisation()
    {
        _stackVisualization.Clear();
        var stackArray = _pageStack.ToArray();
        for (int i = stackArray.Length - 1; i >= 0; i--)
        {
            _stackVisualization.Add(stackArray[i]);
        }
    }
}