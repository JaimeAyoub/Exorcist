using System.Collections.Generic;
using UnityEngine;

public class LetterSpawner : MonoBehaviour
{
    private TextAsset _textAsset;
    public Queue<char> charedText;


    void Start()
    {
        charedText = new Queue<char>();
        charedText.Enqueue();
    }


    void Update()
    {
        
    }
}
