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
    public List<char> listTextToScreen;
    public List<char> listCharedText;
    public TextMeshProUGUI text;
    private int iteratorText = 0;

    private string TypedKey { get; set; }

    private void OnEnable()
    {
        playerInputHandler.KeyTypedEvent += (key) => UpdateScreenText(key);
    }

    private void Awake()
    {
      listCharedText = new List<char>();
      listTextToScreen = new List<char> ();
    }

    void Start()
    {
      foreach(char cha in _textAsset.text.ToCharArray().Where(c => c!= '\r' || c != '\n'))
      {
        listCharedText.Add(cha);
      }
        FillCharQueue();
    }
    void Update()
    {
 
    }

    void PrintLetter(string str)
    {
        TypedKey = str;
    }

    void FillCharQueue()
    {
        for(int i = 0; i <= numberOfCharsInScreen; i++)
        {
            // SHADER PARA LETRAS EN GRIS
            listTextToScreen.Add(listCharedText[i]);
        }
        StartUpdateText();
    }

    void UpdateScreenText(char keyTyped)
    {
        if(keyTyped == listTextToScreen[iteratorText])
        {
            // Efecto de tecla correcta
            iteratorText++;
            Debug.Log(keyTyped + " Es la tecla correcta!!");
        }
        else
        {
            // Efecto de tecla incorrecta
            Debug.Log(keyTyped + " No es la tecla correcta!!");
        }
    }

    void StartUpdateText()
    {
        for(int i = 0; i <= listTextToScreen.Count; i++)
        {
            text.text += listTextToScreen[i];
        }
    }

}
