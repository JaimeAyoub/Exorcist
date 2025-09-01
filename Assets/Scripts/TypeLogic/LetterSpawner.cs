using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private List<GameObject> letterObjects; // Prefabs en pantalla
    public float spaceBetweenLetters;

    private void OnEnable()
    {
        playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    private void Awake()
    {
        textToCharList = textAsset.text.ToList();
        QueueTextToScreen = new Queue<char>();
        letterObjects = new List<GameObject>();

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
            sr.material.SetTexture("_LetterTexture", sprite.texture);
        }

        letterObjects.Add(letterObj);
    }

    private void UpdateScreenText(char keyTyped)
    {
        while (QueueTextToScreen.Count > 0 && !char.IsLetterOrDigit(QueueTextToScreen.Peek())&& QueueTextToScreen.Peek() != ' ')
        {
            // Auto-avanza en espacios o comas sin que el jugador escriba nada
            QueueTextToScreen.Dequeue();
            Destroy(letterObjects[0]);
            letterObjects.RemoveAt(0);
            _iteratorText++;
            AddQueueIfAvailable();
        }
        if (QueueTextToScreen.Count == 0) return;

        char currentChar = QueueTextToScreen.Peek();

        if (char.ToUpper(keyTyped) == char.ToUpper(currentChar))
        {
            // Tecla correcta: quitar de la cola y destruir prefab
            QueueTextToScreen.Dequeue();
            Destroy(letterObjects[0]);
            letterObjects.RemoveAt(0);

            _iteratorText++; 

            AddQueueIfAvailable();
            
            for (int i = 0; i < letterObjects.Count; i++)
            {
                letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);
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
            SpawnLetter(nextChar, letterObjects.Count); 
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }
}
