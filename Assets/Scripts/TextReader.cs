using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TextReader : MonoBehaviour
{
    public TMP_Text texto;
    public string text;

    public char[] textseparate;

    public int index = 0;

    void Start()
    {
        text = "La puta madre como corno vamos a hacer esto";
        textseparate = text.ToCharArray();
        for (int i = 0; i < textseparate.Length; i++)
        {
            texto.text += textseparate[i];
        }

        Debug.Log(textseparate);
    }


    void Update()
    {
        

        if (index >= textseparate.Length)
            return;
        foreach (char c in texto.text)
        {
            if (c == textseparate[index])
               // "color == gray" + c + "</color>";
            if (Input.inputString == textseparate[index].ToString())
            {
                texto.text += "<color=green>" + textseparate[index] + "</color>";


                Debug.Log("Tecla Presionada correctamente, siguiente tecla " + textseparate[index + 1]);
                index++;
            }
        }
    }
}