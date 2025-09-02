using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class LetterSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private const int NumberOfCharsInScreen = 7;

    public TextAsset textAsset; // Texto que se leerá
    public List<char> textToCharList; // Lista de caracteres del texto
    public Queue<char> QueueTextToScreen; // Letras en pantalla
    private int _iteratorText; // Posición actual en el texto

    public GameObject prefabLetter; // Prefab de letra
    public Sprite[] letterSpriteArray; // Sprites de letras
    public Dictionary<char, Sprite> LetterSpritesMap; // Diccionario de sprites
    private List<GameObject> _letterObjects; // Prefabs en pantalla
    public float spaceBetweenLetters;
    public VisualEffect visualEffect;

    public List<GameObject> lettersInBook;
    public GameObject bookLocation;
    public GameObject prefabLetterInBook;
    [SerializeField] private float _seperatorInY = 0;
    [SerializeField] private int lettersInParagraph = 5;
    [SerializeField] private int _letterCount;

    private void OnEnable()
    {
        playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    private void Awake()
    {
        textToCharList = textAsset.text.ToList();
        QueueTextToScreen = new Queue<char>();
        _letterObjects = new List<GameObject>();
        lettersInBook = new List<GameObject>();

        LetterSpritesMap = new Dictionary<char, Sprite>();
        foreach (Sprite sprite in letterSpriteArray)
        {
            char key = char.ToUpper(sprite.name[0]);
            LetterSpritesMap[key] = sprite;
        }
    }

    void Start()
    {
        FillCharQueue();
    }

    private void FillCharQueue()
    {
        int initialCount = Mathf.Min(NumberOfCharsInScreen, textToCharList.Count);

        for (int i = 0; i < initialCount; i++)
        {
            QueueTextToScreen.Enqueue(textToCharList[i]);
        }

        StartUpdateText();
    }

    private void StartUpdateText()
    {
        int index = 0;
        foreach (var c in QueueTextToScreen)
        {
            SpawnLetter(c, index);
            index++;
        }
    }

    private void SpawnLetter(char c, int index)
    {
        GameObject letterObj = Instantiate(prefabLetter, transform);
        letterObj.transform.localPosition = new Vector3(index * spaceBetweenLetters, 0, 0);


        var sr = letterObj.GetComponent<SpriteRenderer>();
        sr.material = new Material(sr.material);

        if (LetterSpritesMap.TryGetValue(char.ToUpper(c), out Sprite sprite))
        {
            sr.sprite = sprite;
            sr.material.SetTexture("_LetterText", sprite.texture);
        }

        _letterObjects.Add(letterObj);
    }

    private void UpdateScreenText(char keyTyped)
    {
        while (QueueTextToScreen.Count > 0 && !char.IsLetterOrDigit(QueueTextToScreen.Peek()) &&
               QueueTextToScreen.Peek() != ' ')
        {
            // Auto-avanza en punutaciones
            QueueTextToScreen.Dequeue();
            Destroy(_letterObjects[0]);
            _letterObjects.RemoveAt(0);

            _iteratorText++;
            AddQueueIfAvailable();
        }

        if (QueueTextToScreen.Count == 0) return;

        char currentChar = QueueTextToScreen.Peek();

        if (char.ToUpper(keyTyped) == char.ToUpper(currentChar))
        {
            QueueTextToScreen.Dequeue();
            AddTextInBook(_letterObjects[0]);

            _letterObjects.RemoveAt(0);
            _letterCount++;
            _iteratorText++;

            AddQueueIfAvailable();

            for (int i = 0; i < _letterObjects.Count; i++)
            {
                _letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);
            }

            if (QueueTextToScreen.Count > 0)
                Debug.Log($"{keyTyped} correcto, siguiente letra: {QueueTextToScreen.Peek()}");
            else
                Debug.Log($"{keyTyped} correcto, fin del texto");
        }
        else
        {
            Debug.Log($"{keyTyped} NO es correcto, se esperaba: {currentChar}");
        }
    }

    private void AddQueueIfAvailable()
    {
        int nextIndex = _iteratorText + NumberOfCharsInScreen - 1;

        if (nextIndex < textToCharList.Count)
        {
            char nextChar = textToCharList[nextIndex];
            QueueTextToScreen.Enqueue(nextChar);
            SpawnLetter(nextChar, _letterObjects.Count);
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }

    private void AnimateText(GameObject letterObj, GameObject finalPosition)
    {
        letterObj.transform.DOMove(finalPosition.transform.position, 0.5f)
            .OnComplete(() =>
            {
                SpawnVFX(finalPosition.transform.position);
                Destroy(letterObj);
            });
    }

    private void AddTextInBook(GameObject letterToAdd)
    {
        if (_iteratorText >= textToCharList.Count) return;

        if (_letterCount >= lettersInParagraph)
        {
            _seperatorInY -= 0.75f;
            _letterCount = 0;
        }

        char currentChar = textToCharList[_iteratorText];
        GameObject letter = Instantiate(prefabLetterInBook, bookLocation.transform);

        letter.transform.localPosition = new Vector3(
            _letterCount * spaceBetweenLetters,
            _seperatorInY,
            0f
        );
        letter.SetActive(false);

        var sr = letter.GetComponent<SpriteRenderer>();
        if (LetterSpritesMap.TryGetValue(char.ToUpper(currentChar), out Sprite sprite))
        {
            sr.sprite = sprite;


            if (sr.material.HasProperty("_LetterTexture"))
            {
                sr.material.SetTexture("_LetterTexture", sprite.texture);
            }
        }

        letterToAdd.transform.DOMove(letter.transform.position, 0.5f)
            .OnComplete(() =>
            {
                SpawnVFX(letter.transform.position); 
                letter.SetActive(true); 
                Destroy(letterToAdd); 
            });

        lettersInBook.Add(letter);
    }

    private void SpawnVFX(Vector3 postion)
    {
        if (!visualEffect) return;
        var vfxInstance = Instantiate(visualEffect, postion, Quaternion.identity);
        vfxInstance.SendEvent("Play");
        Destroy(vfxInstance.gameObject, 2f);
    }
}