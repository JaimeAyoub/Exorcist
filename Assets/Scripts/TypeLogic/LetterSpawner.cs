using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityUtils;
using Random = UnityEngine.Random;

public class LetterSpawner : MonoBehaviour
{
    private const int NumberOfCharsInScreen = 7;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    [Header("Variables para el texto")] public TextAsset textAsset; // Texto que se leerá
    public List<char> textToCharList; // Lista de caracteres del texto
    public RectTransform letterTypedContainer;


    [Header("Variables para el la aparicion de las letras")]
    public GameObject prefabLetter; // Prefab de letra

    public Sprite[] letterSpriteArray; // Sprites de letras
    public float spaceBetweenLetters; //Variable para la separacion entre letras
    public VisualEffect vfxBook; //El efecto que quieres que aparezca
    public VisualEffect vfxHit; //El efecto que quieres que aparezca
    public VisualEffect vfxMiss;

    [Header("Variables para la aparicion de las letras doradas en el libro")]
    public Canvas letterTyped;

    public GameObject prefabLetterInBook; //GameObject con el sprite renderer y shader dorado
    [SerializeField] public int lettersInParagraph; //LEGACY: ya no hace daño por párrafo (daño = palabras + Enter)
    public int _letterCount; //Variable para saber cuantas letras hemos escrito.
    public GameObject SpawnVFXBarra;

    [Header("Combate por palabras (Enter = daño)")]
    [Tooltip("Máximo de palabras completas que se envían con un Enter. 1 palabra = 1 hit.")]
    public int maxWordsPerSubmit = 3;

    [Header("UI opcional: preview con case exacto")]
    [Tooltip("Si se asigna, muestra las próximas palabras objetivo con mayúsculas exactas.")]
    public TextMeshProUGUI targetWordsText;

    [Tooltip("Si se asigna, muestra lo que el jugador lleva tecleado.")]
    public TextMeshProUGUI typedWordsText;

    [Header("Sonidos")] public SoundData letterSound;
    public SoundData noMoreletters;
    public List<SoundData> letterTypedSound;
    public Dictionary<char, Sprite> LetterSpritesMap; // Diccionario de sprites
    public Queue<char> QueueTextToScreen; // Letras en pantalla
    private int _iteratorText; // Posición actual en el texto
    private List<GameObject> _letterObjects; // Prefabs en pantalla
    private List<GameObject> _lettersInBook; //Lista donde guardamos las letras que hay en el libro
    private string _typedBuffer = ""; //Texto tecleado desde el último Enter (case-sensitive)
    private int lastWordsTyped;

    private void Awake()
    {
        // Normalizar: saltos de línea/tabs -> espacio, sin espacios en bordes.
        // Se conserva case y puntuación: la validación es case-sensitive exacta.
        var raw = textAsset != null ? textAsset.text : "";
        raw = raw.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
        if (string.IsNullOrEmpty(raw)) raw = "Amen";
        textToCharList = raw.ToList();
        QueueTextToScreen = new Queue<char>();
        _letterObjects = new List<GameObject>();
        _lettersInBook = new List<GameObject>();

        LetterSpritesMap = new Dictionary<char, Sprite>();
        foreach (var sprite in letterSpriteArray)
        {
            var key = sprite.name[0];
            LetterSpritesMap[key] = sprite;
        }
    }

    private void Start()
    {
        // FillCharQueue();
    }


    private void OnEnable()
    {
        // playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

    public void FillCharQueue()
    {
        var initialCount = Mathf.Min(NumberOfCharsInScreen, textToCharList.Count);

        for (var i = 0; i < initialCount; i++) QueueTextToScreen.Enqueue(textToCharList[i]);

        StartUpdateText();
        AutoSkipSeparators();
        RefreshWordPreview();
    }

    private void StartUpdateText()
    {
        var index = 0;
        foreach (var c in QueueTextToScreen)
        {
            SpawnLetter(c, index);
            index++;
        }
    }

    private void SpawnLetter(char c, int index)
    {
        var letterObj = Instantiate(prefabLetter, transform);
        letterObj.transform.localPosition = new Vector3(index * spaceBetweenLetters, 0, 0);

        var sr = letterObj.GetComponent<SpriteRenderer>();
        sr.material = new Material(sr.material);

        if (LetterSpritesMap.TryGetValue(c, out var sprite))
        {
            sr.sprite = sprite;
            sr.material.SetTexture("_LetterText", sprite.texture);
        }
        else
        {
            Debug.LogWarning("No hay sprite para: " + c);
        }

        _letterObjects.Add(letterObj);
    }

    /// <summary>
    ///     Letra exacta tecleada (vía Keyboard.onTextInput). Comparación
    ///     case-sensitive contra el texto esperado. Espacios, comas, puntos, etc.
    ///     NO hay que teclearlos: se auto-avanzan como separadores (los textos
    ///     no se modifican) y pulsar esas teclas no hace nada.
    ///     Acumula en el libro sin hacer daño; el daño se envía con Enter.
    /// </summary>
    public void HandleTypedChar(char keyTyped)
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.isCombat) return;
        if (IsAutoSeparator(keyTyped)) return;
        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;
        if (CountCompleteWords(_typedBuffer) >= maxWordsPerSubmit)

        {
            SoundManager.Instance.CreateSound().WithSoundData(noMoreletters).WithRandomPitch().Play();
            foreach (var letter in _lettersInBook)
            {
                var letterRectTransform = letter.GetComponentInChildren<RectTransform>();
                if (letterRectTransform != null)
                {
                    letterRectTransform.DOKill();
                    letterRectTransform.localRotation =
                        Quaternion.identity;
                    letterRectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    letterRectTransform.DOScaleX(2.5f, 0.05f).SetLoops(2, LoopType.Yoyo);
                    letterRectTransform.DOScaleY(1.5f, 0.2f);
                    letterRectTransform.DOShakeRotation(0.1f, 45f, 5);
                }
            }


            return;
        }

        AutoSkipSeparators();
        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;

        var currentChar = QueueTextToScreen.Peek();

        if (keyTyped == currentChar) // tecla correcta (case-sensitive)
        {
            var indexForBook = _iteratorText;
            var letterObj = _letterObjects[0];

            AddTextInBook(letterObj, indexForBook);

            QueueTextToScreen.Dequeue();
            _letterObjects.RemoveAt(0);

            SoundManager.Instance.CreateSound().WithSoundData(letterSound).Play();

            _typedBuffer += keyTyped;
            _letterCount++;
            _iteratorText++;


            AddQueueIfAvailable();
            AutoSkipSeparators();

            for (var i = 0; i < _letterObjects.Count; i++)
                _letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);

            RefreshWordPreview();
            var currentWords = CountCompleteWords(_typedBuffer);
            if (lastWordsTyped != currentWords)
            {
                lastWordsTyped = currentWords;

                // Pulso dorado: cada vez que se completa una palabra nueva (1, 2 o 3)
                foreach (var letter in _lettersInBook)
                {
                    var letterImage = letter.GetComponentInChildren<Image>();
                    if (letterImage != null && letterImage.material.HasProperty("_Lerpvalue"))
                        letterImage.material.DOFloat(1.0f, "_Lerpvalue", 1.5f);
                }

                // Remate extra: solo al llegar al máximo de palabras por envío
                if (currentWords >= maxWordsPerSubmit)
                    foreach (var letter in _lettersInBook)
                    {
                        var letterRectTransform = letter.GetComponentInChildren<RectTransform>();
                        if (letterRectTransform != null)
                        {
                            letterRectTransform.DOScaleX(1.05f, 0.1f);
                            letterRectTransform.DOScaleY(1.3f, 0.1f);
                            letterRectTransform.DOShakeRotation(0.2f);
                        }
                    }
            }
        }
        else // tecla incorrecta (incluye case incorrecto)
        {
            var sp = _letterObjects[0].GetComponent<SpriteRenderer>();
            sp.DOColor(Color.red, 0.125f).SetLoops(2, LoopType.Yoyo);
            CameraShake.Instance.CmrShake(0.55f, 0.50f);
            SpawnVFX(SpawnVFXBarra.transform.position, vfxMiss);
        }
    }

    /// <summary>
    ///     Separadores que el jugador NO teclea: se auto-avanzan (espacios, comas,
    ///     puntos...). Los textos no se modifican; solo cambia la validación.
    /// </summary>
    private static bool IsAutoSeparator(char c)
    {
        return c == ' ' || c == ',' || c == '.' || c == ';' || c == ':';
    }

    /// <summary>
    ///     Consume automáticamente los separadores del texto esperado.
    ///     Se destruye su letra en pantalla sin pasar por el libro (no hay sprites
    ///     de separador) y se añade el caracter al buffer para delimitar palabras.
    ///     El buffer siempre es text[cursor - buffer.Length .. cursor].
    /// </summary>
    private void AutoSkipSeparators()
    {
        while (QueueTextToScreen.Count > 0 && IsAutoSeparator(QueueTextToScreen.Peek()))
        {
            var sep = QueueTextToScreen.Dequeue();
            if (_letterObjects.Count > 0)
            {
                Destroy(_letterObjects[0]);
                _letterObjects.RemoveAt(0);
            }

            _typedBuffer += sep;
            _iteratorText++;
            AddQueueIfAvailable();
        }
    }

    /// <summary>
    ///     Enter/Return: envía las palabras COMPLETAS del buffer.
    ///     1 palabra = 1 hit, 2 palabras = 2 hits, 3 palabras = 3 hits.
    ///     La última palabra a medias NO se envía: queda en el buffer.
    /// </summary>
    public void HandleSubmit()
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.isCombat) return;

        var complete = CountCompleteWords(_typedBuffer);
        // Si la oración se acabó, la última palabra (sin espacio final
        // posible) cuenta como completa.
        if (_iteratorText >= textToCharList.Count
            && _typedBuffer.Length > 0
            && _typedBuffer[_typedBuffer.Length - 1] != ' ')
            complete++;
        var hits = Mathf.Min(complete, maxWordsPerSubmit);
        if (hits <= 0) return; // Nada completo todavía: se conserva el buffer.

        // Prefijo consumido; los espacios automáticos no tienen letra en el
        // libro, así que solo vuelan las letras no-espacio del prefijo.
        var consumed = RemoveSubmittedWords(hits);
        var bookLettersToFly = consumed.Count(c => c != ' ');
        FlyBookToEnemy(hits, bookLettersToFly);
        lastWordsTyped = 0;
        RefreshWordPreview();
    }

    /// <summary>
    ///     Palabras completas en el buffer: tokens separados por espacio,
    ///     sin contar una posible última palabra a medias (sin espacio final).
    /// </summary>
    private int CountCompleteWords(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        var tokens = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return 0;

        return s[s.Length - 1] == ' ' ? tokens.Length : tokens.Length - 1;
    }

    /// <summary>
    ///     Elimina del buffer las primeras <paramref name="wordCount" /> palabras
    ///     completas (con sus espacios separadores). Devuelve el prefijo consumido.
    /// </summary>
    private string RemoveSubmittedWords(int wordCount)
    {
        var s = _typedBuffer;
        int idx = 0, words = 0;
        while (words < wordCount && idx < s.Length)
        {
            while (idx < s.Length && s[idx] == ' ') idx++;
            while (idx < s.Length && s[idx] != ' ') idx++;
            if (idx < s.Length && s[idx] == ' ') idx++;
            words++;
        }

        _typedBuffer = s.Substring(idx);
        return s.Substring(0, idx);
    }

    /// <summary>
    ///     Vuela al enemigo solo las letras doradas de las palabras enviadas
    ///     y aplica el daño (1 por palabra). El resto del libro se recoloca.
    /// </summary>
    private void FlyBookToEnemy(int hits, int bookLettersToFly)
    {
        var enemy = CombatManager.Instance.enemy;
        Debug.Log(
            $"[Typeo] Enter: {hits} palabra(s) -> {hits} hit(s). Enemigo: {(enemy != null ? enemy.name : "NULL")}");

        var toFly = Mathf.Min(bookLettersToFly, _lettersInBook.Count);
        var flying = _lettersInBook.GetRange(0, toFly);
        _lettersInBook.RemoveRange(0, toFly);
        _letterCount = Mathf.Max(0, _letterCount - toFly);

        // Sin letras que volar (desync defensivo): aplicar daño directo.
        if (flying.Count == 0)
        {
            DealDamageToEnemy(hits);
            return;
        }

        // Fuera del LayoutGroup: las que quedan se reacomodan solas.
        foreach (var l in flying)
        {
            if (l == null) continue;
            if (l.TryGetComponent(out LetterShake shake)) shake.StopShake();
            Detach(l);
        }

        // Overlay: el enemigo está en mundo, las letras en píxeles de pantalla.
        var enemyScreen = enemy != null
            ? Camera.main.WorldToScreenPoint(enemy.transform.position)
            : Vector3.zero;

        var seq = DOTween.Sequence();
        foreach (var letters in flying)
            if (enemy != null && letters != null)
                seq.Join(
                    letters.transform.DOMove(enemyScreen, 0.5f)
                        .SetEase(Ease.InFlash)
                        .OnComplete(() =>
                        {
                            if (letters != null) Destroy(letters);
                        })
                );
            else if (letters != null) Destroy(letters);


        seq.OnComplete(() => DealDamageToEnemy(hits));
    }

    private void Detach(GameObject go)
    {
        if (go == null) return;
        go.transform.SetParent(letterTyped.transform, true);
        go.transform.SetAsLastSibling(); // dibujar encima del resto
    }

    /// <summary>
    ///     Aplica el daño al enemigo con chequeos y logs. Si falta PlayerAttack,
    ///     daña directamente el EnemyHealthBase para no perder el hit.
    /// </summary>
    private void DealDamageToEnemy(int hits)
    {
        var cm = CombatManager.Instance;
        if (cm == null || cm.enemy == null || cm.player == null)
        {
            Debug.LogError(
                $"[Typeo] Daño cancelado: enemy={(cm != null && cm.enemy != null ? "ok" : "NULL")}, player={(cm != null && cm.player != null ? "ok" : "NULL")}");
            return;
        }

        if (hits <= 0) return;

        var health = cm.enemy.GetComponent<EnemyHealthBase>();
        if (health == null)
        {
            Debug.LogError($"[Typeo] El enemigo '{cm.enemy.name}' no tiene EnemyHealthBase. Daño perdido.");
            return;
        }

        Debug.Log($"[Typeo] Daño {hits} a '{cm.enemy.name}' (vida antes: {health.currentHealth})");
        SpawnVFX(cm.enemy.transform.position, vfxHit);

        var attack = cm.player.GetComponentInChildren<PlayerAttack>();
        if (attack != null && attack.target == cm.enemy)
            attack.Attack(hits);
        else
            health.TakeDamage(hits); // Fallback directo si PlayerAttack falla
    }

    private void RefreshWordPreview()
    {
        if (typedWordsText != null) typedWordsText.text = _typedBuffer;
        if (targetWordsText != null) targetWordsText.text = GetUpcomingWords(3);
    }

    /// <summary>Próximas N palabras esperadas desde el cursor (case exacto).</summary>
    private string GetUpcomingWords(int wordCount)
    {
        if (textToCharList == null || _iteratorText >= textToCharList.Count) return "";
        var idx = _iteratorText;
        while (idx < textToCharList.Count && textToCharList[idx] == ' ') idx++;
        int start = idx, words = 0;
        while (idx < textToCharList.Count && words < wordCount)
            if (textToCharList[idx] == ' ')
            {
                while (idx < textToCharList.Count && textToCharList[idx] == ' ') idx++;
                if (idx < textToCharList.Count) words++;
            }
            else
            {
                idx++;
            }

        var len = Mathf.Min(idx, textToCharList.Count) - start;
        return len > 0 ? new string(textToCharList.GetRange(start, len).ToArray()) : "";
    }


    private void AddQueueIfAvailable()
    {
        var nextIndex = _iteratorText + NumberOfCharsInScreen - 1;

        if (nextIndex < textToCharList.Count)
        {
            var nextChar = textToCharList[nextIndex];
            QueueTextToScreen.Enqueue(nextChar);
            SpawnLetter(nextChar, _letterObjects.Count);
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }


    private void AddTextInBook(GameObject letterToAdd, int index)
    {
        if (!CombatManager.Instance.isCombat || index >= textToCharList.Count || _letterObjects.Count == 0)
        {
            if (letterToAdd != null) Destroy(letterToAdd);
            return;
        }

        var currentChar = textToCharList[index];

        // Letra dorada UI dentro del cuadro. NO se usa SetActive(false): un
        // objeto inactivo lo ignora el LayoutGroup. Se oculta con Image.enabled.
        var letter = Instantiate(prefabLetterInBook, letterTypedContainer);
        var img = PrepareImage(letter);
        img.enabled = false;
        if (!letterTypedSound.IsNullOrEmpty())

        {
            var randomSoundindex = Random.Range(0, letterTypedSound.Count);
            SoundManager.Instance.CreateSound().WithSoundData(letterTypedSound[randomSoundindex]).WithRandomPitch()
                .Play();
        }

        if (LetterSpritesMap.TryGetValue(currentChar, out var sprite))
        {
            img.sprite = sprite;
            if (img.material.HasProperty("_LetterTexture"))
                img.material.SetTexture("_LetterTexture", sprite.texture);
        }

        _lettersInBook.Add(letter);

        // El hueco solo es válido tras recalcular el layout.
        LayoutRebuilder.ForceRebuildLayoutImmediate(letterTypedContainer);

        if (letterToAdd == null)
        {
            img.enabled = true;
            return;
        }

        var cam = Camera.main;
        var startWorld = letterToAdd.transform.position;
        // Profundidad de la letra respecto a la cámara (para convertir pantalla -> mundo).
        var depth = cam.WorldToScreenPoint(startWorld).z;
        var lastTarget = startWorld;

        DOVirtual.Float(0f, 1f, 0.1f, t =>
            {
                if (letterToAdd == null || letter == null) return;
                var slotScreen = letter.transform.position; // Overlay: píxeles
                lastTarget = cam.ScreenToWorldPoint(new Vector3(slotScreen.x, slotScreen.y, depth));
                letterToAdd.transform.position = Vector3.Lerp(startWorld, lastTarget, t);
            })
            .SetTarget(letterToAdd.transform)
            .OnComplete(() =>
            {
                if (letterToAdd != null) Destroy(letterToAdd);
                if (letter != null)
                {
                    img.enabled = true;


                    if (letter.TryGetComponent(out LetterShake shake)) shake.StartShake();
                    img.GetComponent<RectTransform>().DOScale(1.5f, 0.05f).SetLoops(2, LoopType.Yoyo);
                }
            });
    }

    private Image PrepareImage(GameObject go)
    {
        var img = go.GetComponentInChildren<Image>();
        img.material = new Material(img.material); // instancia propia
        img.raycastTarget = false;
        return img;
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
            if (go != null)
            {
                go.transform.DOKill();
                Destroy(go);
            }


        foreach (var go in _lettersInBook)
            if (go != null)
            {
                go.transform.DOKill();
                Destroy(go);
            }

        QueueTextToScreen.Clear();
        _letterObjects.Clear();
        _lettersInBook.Clear();
        _iteratorText = 0;
        _letterCount = 0;
        _typedBuffer = "";
        lastWordsTyped = 0;
        RefreshWordPreview();
    }
}