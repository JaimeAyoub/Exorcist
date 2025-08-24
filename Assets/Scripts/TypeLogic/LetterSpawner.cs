using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class LetterSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private const int NumberOfCharsInScreen = 7;

    [FormerlySerializedAs("_textAsset")] public TextAsset textAsset;
    public List<char> listTextToScreen;
    public List<char> listCharedText;
    public TextMeshProUGUI text;
    private int _iteratorText = 0;

    private string TypedKey { get; set; }

    private void OnEnable()
    {
        playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    private void Awake()
    {
      listCharedText = new List<char>();
      listTextToScreen = new List<char> ();
    }

    void Start()
    {
      foreach(var cha in textAsset.text.Where(c => true))
      {
        listCharedText.Add(cha);
      }
      FillCharQueue();
    }

    private void FillCharQueue()
    {
        for(var i = 0; i < NumberOfCharsInScreen; i++)
        {
            // SHADER PARA LETRAS EN GRIS
            listTextToScreen.Add(listCharedText[i]);
        }
        StartUpdateText();
    }

    private void UpdateScreenText(char keyTyped)
    {
        Debug.Log(keyTyped + " letter spawner");
        if( keyTyped == listTextToScreen[_iteratorText])
        {
            // Efecto de tecla correcta
            _iteratorText++;
            Debug.Log(keyTyped + " Es la tecla correcta!! primer if");
        }
        else if(keyTyped == listTextToScreen[_iteratorText])
        {
            // Efecto de tecla incorrecta
            _iteratorText++;
            Debug.Log(keyTyped + " Es la tecla correcta!! segundo if");
        }
        else
        {
            // Efecto de tecla incorrecta
            Debug.Log(keyTyped + " No es la tecla correcta!! tercer if");
        }
    }

    private void StartUpdateText()
    {
        for(var i = 0; i < listTextToScreen.Count; i++)
        {
            text.text += listTextToScreen[i];
        }
    }

}
