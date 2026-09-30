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

    [Header("Gula: se come una letra por palabra")]
    [Tooltip("Si el enemigo actual es Gula, devora una letra aleatoria por palabra. Esa letra no hay que teclearla: se auto-avanza y se muestra como '_' ocupando su espacio.")]
    public bool devourOneLetterPerWord = true;
    [Tooltip("Palabras más cortas que esto no son devoradas (para no hacerlas imposibles de leer).")]
    public int minWordLengthToDevour = 3;
    [Tooltip("Sprite de '_' para la letra devorada. Si se deja vacío se busca un sprite '_' en letterSpriteArray y, como fallback, se aplasta/tiñe el sprite real a modo de guión.")]
    public Sprite devouredSprite;
    [Tooltip("Sonido opcional al devorar/auto-avanzar la letra comida.")]
    public SoundData gulaDevourSound;
    [Tooltip("Tinte aplicado a la letra devorada cuando no hay sprite '_' disponible.")]
    public Color devouredTint = new Color(0.25f, 0.25f, 0.25f, 1f);

    // Índices globales (en textToCharList) devorados por Gula + cola paralela de índices para saber qué letra en pantalla está devorada.
    private readonly HashSet<int> _devouredIndices = new HashSet<int>();
    private Queue<int> _queueTextIndices;
    public bool GulaActive { get; private set; }

    private void Awake()
    {
        // Normalizar: saltos de línea/tabs -> espacio, sin espacios en bordes.
        // Se conserva case y puntuación: la validación es case-sensitive exacta.
        var raw = textAsset != null ? textAsset.text : "";
        raw = raw.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
        if (string.IsNullOrEmpty(raw)) raw = "Amen";
        textToCharList = raw.ToList();
        QueueTextToScreen = new Queue<char>();
        _queueTextIndices = new Queue<int>();
        _letterObjects = new List<GameObject>();
        _lettersInBook = new List<GameObject>();

        LetterSpritesMap = new Dictionary<char, Sprite>();
        foreach (var sprite in letterSpriteArray)
        {
            if (sprite == null) continue;
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
        BuildDevouredSet();

        var initialCount = Mathf.Min(NumberOfCharsInScreen, textToCharList.Count);

        for (var i = 0; i < initialCount; i++)
        {
            QueueTextToScreen.Enqueue(textToCharList[i]);
            _queueTextIndices.Enqueue(i);
        }

        StartUpdateText();
        AutoSkipSeparators();
        AutoSkipDevoured();
        RefreshWordPreview();
    }

    private void StartUpdateText()
    {
        var index = 0;
        var textIndex = 0;
        foreach (var c in QueueTextToScreen)
        {
            var ti = _queueTextIndices.Count > index ? _queueTextIndices.ElementAt(index) : textIndex;
            SpawnLetter(c, index, ti);
            index++;
            textIndex++;
        }
    }

    private void SpawnLetter(char c, int index, int textIndex = -1)
    {
        var letterObj = Instantiate(prefabLetter, transform);
        // El hueco siempre se conserva: la letra devorada ocupa su mismo espacio.
        letterObj.transform.localPosition = new Vector3(index * spaceBetweenLetters, 0, 0);

        var sr = letterObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.material = new Material(sr.material);

        var isDevoured = textIndex >= 0 && _devouredIndices.Contains(textIndex);
        if (isDevoured)
        {
            ApplyDevouredVisualWorld(letterObj, sr, c);
            _letterObjects.Add(letterObj);
            return;
        }

        if (sr != null && LetterSpritesMap.TryGetValue(c, out var sprite))
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
        AutoSkipDevoured();
        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;

        var currentChar = QueueTextToScreen.Peek();

        if (keyTyped == currentChar) // tecla correcta (case-sensitive)
        {
            var indexForBook = _queueTextIndices.Count > 0 ? _queueTextIndices.Peek() : _iteratorText;
            var letterObj = _letterObjects[0];

            AddTextInBook(letterObj, indexForBook);

            QueueTextToScreen.Dequeue();
            if (_queueTextIndices.Count > 0) _queueTextIndices.Dequeue();
            _letterObjects.RemoveAt(0);

            SoundManager.Instance.CreateSound().WithSoundData(letterSound).Play();

            _typedBuffer += keyTyped;
            _letterCount++;
            _iteratorText++;


            AddQueueIfAvailable();
            AutoSkipSeparators();
            AutoSkipDevoured();

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
            if (_queueTextIndices.Count > 0) _queueTextIndices.Dequeue();
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

    // ---------- Gula: letra devorada ----------

    /// <summary>
    /// Combate actual contra Gula: se acepta tag "Gula", nombre con "Gula"
    /// o componente GulaHealth (no depende solo del tag por si falta en TagManager).
    /// </summary>
    public bool IsGulaCombat()
    {
        var cm = CombatManager.Instance;
        if (cm == null || cm.enemy == null) return false;
        var enemy = cm.enemy;
        if (enemy.GetComponent<GulaHealth>() != null) return true;
        if (enemy.GetComponentInChildren<GulaHealth>() != null) return true;
        // Comparación directa (no CompareTag) para no lanzar excepción si el tag aún no existe.
        try
        {
            if (enemy.tag == "Gula") return true;
        }
        catch (UnityException) { /* tag sin definir: se ignora, manda el componente */ }
        if (enemy.name.Contains("Gula")) return true;
        return false;
    }

    /// <summary>
    /// Elige una letra aleatoria por palabra para que Gula se la coma.
    /// Se guarda por índice global en textToCharList. Las palabras cortas se respetan.
    /// </summary>
    private void BuildDevouredSet()
    {
        _devouredIndices.Clear();
        GulaActive = devourOneLetterPerWord && IsGulaCombat();
        if (!GulaActive || textToCharList == null) return;

        var i = 0;
        while (i < textToCharList.Count)
        {
            if (IsAutoSeparator(textToCharList[i])) { i++; continue; }
            var start = i;
            while (i < textToCharList.Count && !IsAutoSeparator(textToCharList[i])) i++;
            var len = i - start;
            if (len < Mathf.Max(1, minWordLengthToDevour)) continue;

            // Interior aleatorio para que la palabra siga legible (para len==3, la del medio).
            int pick;
            if (len <= 3) pick = start + len / 2;
            else pick = Random.Range(start + 1, i - 1);
            _devouredIndices.Add(pick);
            Debug.Log($"[Gula] Devorada letra '{textToCharList[pick]}' índice {pick} de palabra len {len}");
        }
    }

    private bool IsDevouredFront()
    {
        return GulaActive
               && QueueTextToScreen.Count > 0
               && _queueTextIndices.Count > 0
               && _devouredIndices.Contains(_queueTextIndices.Peek());
    }

    /// <summary>
    /// Consume sin input las letras devoradas que lleguen al frente.
    /// Deja su hueco (misma posición) y crea su eco dorado como '_' en el libro,
    /// sumando el caracter real al buffer para que la palabra siga completándose con Enter.
    /// </summary>
    private void AutoSkipDevoured()
    {
        if (!GulaActive) return;
        var guard = 0;
        while (IsDevouredFront() && guard++ < NumberOfCharsInScreen + 4)
        {
            var realChar = QueueTextToScreen.Dequeue();
            var devouredIndex = _queueTextIndices.Count > 0 ? _queueTextIndices.Dequeue() : _iteratorText;
            var letterObj = _letterObjects.Count > 0 ? _letterObjects[0] : null;
            if (_letterObjects.Count > 0) _letterObjects.RemoveAt(0);

            // Eco en el libro con visual de '_' pero cuenta como letra real para daño/palabras.
            AddTextInBook(letterObj, devouredIndex, true);

            _typedBuffer += realChar;
            _letterCount++;
            _iteratorText++;

            if (gulaDevourSound != null && gulaDevourSound.clip != null)
                SoundManager.Instance.CreateSound().WithSoundData(gulaDevourSound).WithRandomPitch().Play();

            AddQueueIfAvailable();

            for (var k = 0; k < _letterObjects.Count; k++)
                _letterObjects[k].transform.localPosition = new Vector3(k * spaceBetweenLetters, 0, 0);

            // Las devoradas nunca son separadores, pero por si encadenan con espacios:
            AutoSkipSeparators();
        }
        RefreshWordPreview();
    }

    /// <summary>Mundo (SpriteRenderer): sprite guión de Gula, con el hueco conservado.</summary>
    private void ApplyDevouredVisualWorld(GameObject letterObj, SpriteRenderer sr, char realChar)
    {
        if (sr == null) return;
        letterObj.transform.localScale = Vector3.one;
        if (TryGetDevouredSprite(out var dash))
        {
            sr.sprite = dash;
            sr.material.SetTexture("_LetterText", dash.texture);
            sr.color = Color.white;
            return;
        }
        // Sin sprite de guión (no debería pasar): hueco vacío, nunca la letra real.
        Debug.LogWarning("[Gula] No hay sprite de guión en letterSpriteArray ni en devouredSprite.");
        sr.sprite = null;
        sr.color = devouredTint;
    }

    /// <summary>Libro (UI Image): sprite guión de Gula, con el hueco conservado.</summary>
    private void ApplyDevouredVisualBook(Image img)
    {
        if (img == null) return;
        var rt = img.GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one;
        if (TryGetDevouredSprite(out var dash))
        {
            img.sprite = dash;
            if (img.material != null && img.material.HasProperty("_LetterTexture"))
                img.material.SetTexture("_LetterTexture", dash.texture);
            img.color = Color.white;
            return;
        }
        img.sprite = null;
        img.color = devouredTint;
    }

    private bool TryGetDevouredSprite(out Sprite dash)
    {
        if (devouredSprite != null) { dash = devouredSprite; return true; }
        if (LetterSpritesMap != null)
        {
            if (LetterSpritesMap.TryGetValue('_', out dash) && dash != null) return true;
            if (LetterSpritesMap.TryGetValue('-', out dash) && dash != null) return true;
        }
        dash = null;
        return false;
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
        var parent = letterTyped != null ? letterTyped.transform
            : (letterTypedContainer != null ? letterTypedContainer.transform : null);
        if (parent == null) { Destroy(go); return; }
        go.transform.SetParent(parent, true);
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

    /// <summary>Próximas N palabras esperadas desde el cursor (case exacto). Gula muestra '_' en devoradas.</summary>
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
        if (len <= 0) return "";
        var chars = textToCharList.GetRange(start, len).ToArray();
        if (GulaActive)
            for (var k = 0; k < chars.Length; k++)
                if (_devouredIndices.Contains(start + k) && !IsAutoSeparator(chars[k]))
                    chars[k] = '_';
        return new string(chars);
    }


    private void AddQueueIfAvailable()
    {
        var nextIndex = _iteratorText + NumberOfCharsInScreen - 1;

        if (nextIndex < textToCharList.Count)
        {
            var nextChar = textToCharList[nextIndex];
            QueueTextToScreen.Enqueue(nextChar);
            _queueTextIndices.Enqueue(nextIndex);
            SpawnLetter(nextChar, _letterObjects.Count, nextIndex);
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }


    private void AddTextInBook(GameObject letterToAdd, int index, bool isDevoured = false)
    {
        // Las devoradas ya salieron de _letterObjects, así que no se exige _letterObjects.Count > 0 en ese caso.
        if (!CombatManager.Instance.isCombat || index >= textToCharList.Count || (!isDevoured && _letterObjects.Count == 0))
        {
            if (letterToAdd != null) Destroy(letterToAdd);
            return;
        }

        var currentChar = textToCharList[index];
        isDevoured = isDevoured || _devouredIndices.Contains(index);

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

        // Gula: el eco en el libro muestra '_' pero ocupa el mismo hueco del LayoutGroup.
        if (isDevoured) ApplyDevouredVisualBook(img);

        _lettersInBook.Add(letter);

        // El hueco solo es válido tras recalcular el layout.
        if (letterTypedContainer != null)
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
        _queueTextIndices?.Clear();
        _letterObjects.Clear();
        _lettersInBook.Clear();
        _devouredIndices.Clear();
        GulaActive = false;
        _iteratorText = 0;
        _letterCount = 0;
        _typedBuffer = "";
        lastWordsTyped = 0;
        RefreshWordPreview();
    }
}