using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class LetterSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private const int NumberOfCharsInScreen = 7;

    [Header("Variables para el texto")] 
    public TextAsset textAsset;
    public List<char> textToCharList;
    public Queue<char> QueueTextToScreen;
    private int _iteratorText;


    [Header("Variables para la aparición de las letras")]
    public GameObject prefabLetter;
    public Sprite[] letterSpriteArray;
    public Dictionary<char, Sprite> LetterSpritesMap;
    private List<GameObject> _letterObjects;
    public float spaceBetweenLetters;
    public VisualEffect vfxBook;
    public VisualEffect vfxHit;
    public VisualEffect vfxMiss;

    [Header("Variables para el libro")]
    public GameObject bookLocation;
    public GameObject prefabLetterInBook;
    private List<GameObject> _lettersInBook;
    [SerializeField] public int lettersInParagraph;
    public int _letterCount;
    public GameObject SpawnVFXBarra;


    public SoundData typeSD;
    private void Awake()
    {
        textToCharList = textAsset.text.ToList();
        QueueTextToScreen = new Queue<char>();
        _letterObjects = new List<GameObject>();
        _lettersInBook = new List<GameObject>();

        LetterSpritesMap = new Dictionary<char, Sprite>();
        foreach (Sprite sprite in letterSpriteArray)
        {
        
            char key = sprite.name[0]; 
            LetterSpritesMap[key] = sprite;
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
            playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
            playerInputHandler.KeyTypedEvent -= UpdateScreenText;
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
        if (!CombatManager.Instance.isCombat) return;

        while (QueueTextToScreen.Count > 0 && char.IsControl(QueueTextToScreen.Peek()))
        {
            if (_letterObjects.Count > 0)
            {
                Destroy(_letterObjects[0]);
                _letterObjects.RemoveAt(0);
            }

            QueueTextToScreen.Dequeue();
            _iteratorText++;
            AddQueueIfAvailable();
        }

        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;

        char currentChar = QueueTextToScreen.Peek();

        if (keyTyped == currentChar)
        {
            int indexForBook = _iteratorText;
            GameObject letterObj = _letterObjects[0];

            AddTextInBook(letterObj, indexForBook);

            QueueTextToScreen.Dequeue();
            _letterObjects.RemoveAt(0);

            SoundManager.Instance.CreateSound().WithSoundData(typeSD).WithRandomPitch().Play();
            _letterCount++;
            _iteratorText++;
            CombatManager.Instance.AddTime(1.0f);

            AddQueueIfAvailable();

            for (int i = 0; i < _letterObjects.Count; i++)
                _letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);
        }
        else 
        {
            if (_letterObjects.Count > 0 && keyTyped != ' ')
            {
                SpriteRenderer sp = _letterObjects[0].GetComponent<SpriteRenderer>();
                
                sp.DOKill(); 
                sp.color = Color.white; 
                sp.DOColor(Color.red, 0.125f).SetLoops(2, LoopType.Yoyo);
                
                CombatManager.Instance.SubstracTime(1.0f);
                CameraShake.Instance.CmrShake(0.55f, 0.50f);
                SpawnVFX(SpawnVFXBarra.transform.position, vfxMiss);
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
        }
    }

    private void AddTextInBook(GameObject letterToAdd, int index)
    {
        if (!CombatManager.Instance.isCombat || index >= textToCharList.Count || _letterObjects.Count == 0)
        {
            Destroy(letterToAdd);
            return;
        }

        char currentChar = textToCharList[index];

        if (_letterCount >= lettersInParagraph)
        {
            _letterCount = 0;
            Sequence seq = DOTween.Sequence();

            foreach (var letterObj in _lettersInBook.ToList())
            {
                if (CombatManager.Instance.enemy != null && letterObj != null)
                {
                    seq.Join(
                        letterObj.transform.DOMove(CombatManager.Instance.enemy.transform.position, 0.5f)
                            .SetEase(Ease.InFlash)
                            .OnComplete(() =>
                            {
                                if (letterObj != null && CombatManager.Instance.player != null)
                                {
                                    Destroy(letterObj);
                                    _lettersInBook.Remove(letterObj);
                                }
                            })
                    );
                }
            }

            seq.OnComplete(() =>
            {
                SpawnVFX(CombatManager.Instance.enemy.transform.position, vfxHit);
                CombatManager.Instance.player.GetComponentInChildren<PlayerAttack>().Attack(1);
            });
        }

        GameObject letter = Instantiate(prefabLetterInBook, bookLocation.transform);
        letter.transform.localPosition = new Vector3(_letterCount * spaceBetweenLetters, 0f, 0f);
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

    private void SpawnVFX(Vector3 position, VisualEffect vfx)
    {
        if (!vfx) return; 
        var vfxInstance = Instantiate(vfx, position, Quaternion.identity);
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
    }
}