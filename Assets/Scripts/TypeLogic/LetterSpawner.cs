using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LetterSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private int numberOfCharsInScreen = 7;

    public TextAsset _textAsset;
    public Queue<char> textToScreen;
    public List<char> charedText;
    public TextMeshProUGUI text;
    private int iteratorText = 1;

    private string TypedKey { get; set; }

    private void OnEnable()
    {
        playerInputHandler.KeyTypedEvent += (key) => PrintLetter(key);
    }

    private void Awake()
    {
      charedText = new List<char>();
      textToScreen = new Queue<char> ();
    }

    void Start()
    {

      foreach(char cha in _textAsset.text.ToCharArray().Where(c => c!= '\r' || c != '\n'))
      {
        charedText.Add(cha);
      }
        FillCharQueue();
        
        
    }
    void Update()
    {
        //UpdateTextFromQueue();
    }

    void PrintLetter(string str)
    {
        TypedKey = str;
        Debug.Log(TypedKey);
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
