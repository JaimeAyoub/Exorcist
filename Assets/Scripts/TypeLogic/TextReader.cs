using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TextReader : MonoBehaviour
{
    public TextAsset textAsset;
    public TMP_Text tmpTexto;
    public string text;

    public char[] textseparate;

    
    private bool textEnded;
    void Start()
    {
        textseparate = textAsset.text.ToCharArray();

        

        for (int i = 0; i < textseparate.Length; i++)
        {
            tmpTexto.text += textseparate[i];
        }

        Debug.Log(textseparate);
    }

    void LetterToTMP(char correctChar, bool isCorrectChar)
    {
        if (isCorrectChar == true)
        {
            tmpTexto.text += correctChar;
        }
    }


    void Update()
    {
        while (textEnded == true)
        {
            int index = 0;

            if(Input.inputString == textseparate[index].ToString())
            {
                index++;
                LetterToTMP(textseparate[index], true);
                break;
            }
            else
            {
                index--;
                break;
            }
        }

        //if (index >= textseparate.Length)
        //    return;
        //foreach (char c in texto.text)
        //{
        //    if (c == textseparate[index])
        //       // "color == gray" + c + "</color>";
        //    if (Input.inputString == textseparate[index].ToString())
        //    {
        //        texto.text += "<color=green>" + textseparate[index] + "</color>";


        //        Debug.Log("Tecla Presionada correctamente, siguiente tecla " + textseparate[index + 1]);
        //        index++;
        //    }
        //}
    }
}