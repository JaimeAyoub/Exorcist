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

    [Header("Variables para el texto")] public TextAsset textAsset; // Texto que se leerá
    public List<char> textToCharList; // Lista de caracteres del texto
    public Queue<char> QueueTextToScreen; // Letras en pantalla
    private int _iteratorText; // Posición actual en el texto


    [Header("Variables para el la aparicion de las letras")]
    public GameObject prefabLetter; // Prefab de letra

    public Sprite[] letterSpriteArray; // Sprites de letras
    public Dictionary<char, Sprite> LetterSpritesMap; // Diccionario de sprites
    private List<GameObject> _letterObjects; // Prefabs en pantalla
    public float spaceBetweenLetters; //Variable para la separacion entre letras
    public VisualEffect vfxBook; //El efecto que quieres que aparezca
    public VisualEffect vfxHit; //El efecto que quieres que aparezca

    [Header("Variables para la aparicion de las letras doradas en el libro")]
    public GameObject bookLocation; //Donde apareceran las letras doradas

    public GameObject prefabLetterInBook; //GameObject con el sprite renderer y shader dorado
    private List<GameObject> _lettersInBook; //Lista donde guardamos las letras que hay en el libro
    private float _seperatorInY = 0; //Variable para pasar parrafo
    [SerializeField] public int lettersInParagraph; //Variable para poner cuantas letras quieres por parrafo
    public int _letterCount; //Variable para saber cuantas letras hemos escrito.

    private void OnEnable()
    {
        // playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    private void Awake()
    {
        textToCharList = textAsset.text.ToList();
        QueueTextToScreen = new Queue<char>();
        _letterObjects = new List<GameObject>();
        _lettersInBook = new List<GameObject>();

        LetterSpritesMap = new Dictionary<char, Sprite>();
        foreach (Sprite sprite in letterSpriteArray)
        {
            char key = char.ToUpper(sprite.name[0]);
            LetterSpritesMap[key] = sprite;
        }
    }

    void Start()
    {
        // FillCharQueue();
    }

    public void FillCharQueue()
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
            SpawnLetter(c, index, prefabLetter.transform.position);
            index++;
        }
    }

    private void SpawnLetter(char c, int index, Vector3 position)
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

    public void UpdateScreenText(char keyTyped)
    {
        if (!CombatManager.instance.isCombat) return;

        // Avanzar punteaciones
        while (QueueTextToScreen.Count > 0 && !char.IsLetterOrDigit(QueueTextToScreen.Peek()) &&
               QueueTextToScreen.Peek() != ' ')
        {
            if (_letterObjects.Count > 0)
            {
                Destroy(_letterObjects[0]);
                _letterObjects.RemoveAt(0);
            }

            if (QueueTextToScreen.Count > 0)
                QueueTextToScreen.Dequeue();

            _iteratorText++;
            AddQueueIfAvailable();
        }

        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;

        char currentChar = QueueTextToScreen.Peek();

        if (char.ToUpper(keyTyped) == char.ToUpper(currentChar)) // tecla correcta
        {
            int indexForBook = _iteratorText;
            GameObject letterObj = _letterObjects[0];

            AddTextInBook(letterObj, indexForBook);

            if (QueueTextToScreen.Count > 0)
                QueueTextToScreen.Dequeue();

            if (_letterObjects.Count > 0)
                _letterObjects.RemoveAt(0);

            AudioManager.instance.PlayBGM(SoundType.TECLAS, 0.5f);
            _letterCount++;
            _iteratorText++;
            CombatManager.instance.AddTime(1.0f);

            AddQueueIfAvailable();

            for (int i = 0; i < _letterObjects.Count; i++)
                _letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);
        }
        else // tecla incorrecta
        {
            if (_letterObjects.Count > 0 && keyTyped != ' ')
            {
                SpriteRenderer sp = _letterObjects[0].GetComponent<SpriteRenderer>();
                sp.DOColor(Color.red, 0.125f).SetLoops(2, LoopType.Yoyo);
                CombatManager.instance.SubstracTime(1.0f);
            }
        }
    }


    private void AddQueueIfAvailable()
    {
        int nextIndex = _iteratorText + NumberOfCharsInScreen - 1;

        if (nextIndex < textToCharList.Count)
        {
            char nextChar = textToCharList[nextIndex];
            QueueTextToScreen.Enqueue(nextChar);
            SpawnLetter(nextChar, _letterObjects.Count, prefabLetter.transform.position);
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }


    private void AddTextInBook(GameObject letterToAdd, int index)
    {
        if (!CombatManager.instance.isCombat || index >= textToCharList.Count || _letterObjects.Count == 0)
        {
            Destroy(letterToAdd);
            return;
        }

        char currentChar = textToCharList[index];


        if (_letterCount >= lettersInParagraph)
        {
            _letterCount = 0;

            Sequence seq = DOTween.Sequence();

            foreach (var letters in _lettersInBook.ToList())
            {
                if (CombatManager.instance.enemy != null && letters != null)
                {
                    seq.Join(
                        letters.transform.DOMove(
                                CombatManager.instance.enemy.transform.position,
                                0.5f)
                            .SetEase(Ease.InFlash)
                            .OnComplete(() =>
                            {
                                if (letters != null && CombatManager.instance.player != null)
                                {
                                    Destroy(letters);
                                    _lettersInBook.Remove(letters);
                                }
                            })
                    );
                }
            }

            seq.OnComplete(() =>
            {
                SpawnVFX(CombatManager.instance.enemy.transform.position, vfxHit);
                CombatManager.instance.player
                    .GetComponentInChildren<PlayerAttack>()
                    .Attack(1);
            });
        }

        GameObject letter = Instantiate(prefabLetterInBook, bookLocation.transform);

        letter.transform.localPosition = new Vector3(
            _letterCount * spaceBetweenLetters,
            0f,
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

        if (letterToAdd != null && letter != null)
        {
            letterToAdd.transform.DOMove(letter.transform.position, 0.5f)
                .OnComplete(() =>
                {
                    if (letterToAdd != null) Destroy(letterToAdd);
                    if (letter != null) letter.SetActive(true);
                    SpawnVFX(letter.transform.position, vfxBook);
                });
        }

        _lettersInBook.Add(letter);
    }

    private void SpawnVFX(Vector3 postion, VisualEffect vfx)
    {
        if (!vfxBook) return;
        var vfxInstance = Instantiate(vfx, postion, Quaternion.identity);
        vfxInstance.SendEvent("Play");
        Destroy(vfxInstance.gameObject, 2f);
    }

    public void EmptyAll()
    {
        foreach (var go in _letterObjects)
        {
            if (go != null)
            {
                go.transform.DOKill();
                Destroy(go);
            }
        }


        foreach (var go in _lettersInBook)
        {
            if (go != null)
            {
                go.transform.DOKill();
                Destroy(go);
            }
        }

        QueueTextToScreen.Clear();
        _letterObjects.Clear();
        _lettersInBook.Clear();
        _iteratorText = 0;
        _letterCount = 0;
        _seperatorInY = 0f;
    }
}