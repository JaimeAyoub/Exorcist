using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LetterSpawner : MonoBehaviour
{
    private InputSystem_Actions inputActions;


    private int numberOfCharsInScreen = 7;

    public TextAsset _textAsset;
    public Queue<char> textToScreen;
    public List<char> charedText;
    public TextMeshProUGUI text;
    private int iteratorText = 1;

    private void Awake()
    {
      inputActions = new InputSystem_Actions();
      charedText = new List<char>();
      textToScreen = new Queue<char> ();
    }

    void Start()
    {
        inputActions.Enable();
      foreach(char cha in _textAsset.text.ToCharArray().Where(c => c!= '\r' || c != '\n'))
      {
        charedText.Add(cha);
      }
        FillCharQueue();
        inputActions.Player.Typing.performed += (InputAction.CallbackContext context) => Debug.Log(context.control.name);
        
    }
    void Update()
    {
        //UpdateTextFromQueue();
    }

    void FillCharQueue()
    {
        for(int i = 0; i < numberOfCharsInScreen; i++)
        {
            textToScreen.Enqueue(charedText[i]);
        }
    }

    void UpdateQueue(char chara, bool isCorrect)
    {

        if(isCorrect)
        {
            textToScreen.Dequeue();
            textToScreen.Enqueue(charedText[numberOfCharsInScreen + iteratorText]);
            iteratorText++;
        }
        else
        {
            return;
        }
              
    }

    void UpdateTextFromQueue()
    {
        text.text += textToScreen.Dequeue().ToString();
    }

}
