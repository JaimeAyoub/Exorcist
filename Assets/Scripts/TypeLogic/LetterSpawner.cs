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
    public VisualEffect vfxMiss;

    [Header("Variables para la aparicion de las letras doradas en el libro")]
    public GameObject bookLocation; //Donde apareceran las letras doradas

    public GameObject prefabLetterInBook; //GameObject con el sprite renderer y shader dorado
    private List<GameObject> _lettersInBook; //Lista donde guardamos las letras que hay en el libro
    [SerializeField] public int lettersInParagraph; //LEGACY: ya no hace daño por párrafo (daño = palabras + Enter)
    public int _letterCount; //Variable para saber cuantas letras hemos escrito.
    public GameObject SpawnVFXBarra;

    [Header("Combate por palabras (Enter = daño)")]
    [Tooltip("Máximo de palabras completas que se envían con un Enter. 1 palabra = 1 hit.")]
    public int maxWordsPerSubmit = 3;
    private string _typedBuffer = ""; //Texto tecleado desde el último Enter (case-sensitive)

    [Header("UI opcional: preview con case exacto")]
    [Tooltip("Si se asigna, muestra las próximas palabras objetivo con mayúsculas exactas.")]
    public TextMeshProUGUI targetWordsText;
    [Tooltip("Si se asigna, muestra lo que el jugador lleva tecleado.")]
    public TextMeshProUGUI typedWordsText;

    [Header("Sonidos")] public SoundData letterSound;

    private void OnEnable()
    {
        // playerInputHandler.KeyTypedEvent += UpdateScreenText;
    }

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
        string raw = textAsset != null ? textAsset.text : "";
        raw = raw.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
        if (string.IsNullOrEmpty(raw)) raw = "Amen";
        textToCharList = raw.ToList();
        QueueTextToScreen = new Queue<char>();
        _queueTextIndices = new Queue<int>();
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
<<<<<<< Updated upstream
        int initialCount = Mathf.Min(NumberOfCharsInScreen, textToCharList.Count);

        for (int i = 0; i < initialCount; i++)
        {
            QueueTextToScreen.Enqueue(textToCharList[i]);
=======
        BuildDevouredSet();

        var initialCount = Mathf.Min(NumberOfCharsInScreen, textToCharList.Count);

        for (var i = 0; i < initialCount; i++)
        {
            QueueTextToScreen.Enqueue(textToCharList[i]);
            _queueTextIndices.Enqueue(i);
>>>>>>> Stashed changes
        }

        StartUpdateText();
        AutoSkipSeparators();
        AutoSkipDevoured();
        RefreshWordPreview();
    }

    private void StartUpdateText()
    {
<<<<<<< Updated upstream
        int index = 0;
        foreach (var c in QueueTextToScreen)
        {
            SpawnLetter(c, index, prefabLetter.transform.position);
=======
        var index = 0;
        var textIndex = 0;
        foreach (var c in QueueTextToScreen)
        {
            var ti = _queueTextIndices.Count > index ? _queueTextIndices.ElementAt(index) : textIndex;
            SpawnLetter(c, index, ti);
>>>>>>> Stashed changes
            index++;
            textIndex++;
        }
    }

<<<<<<< Updated upstream
    private void SpawnLetter(char c, int index, Vector3 position)
    {
        GameObject letterObj = Instantiate(prefabLetter, transform);
=======
    private void SpawnLetter(char c, int index, int textIndex = -1)
    {
        var letterObj = Instantiate(prefabLetter, transform);
        // El hueco siempre se conserva: la letra devorada ocupa su mismo espacio.
>>>>>>> Stashed changes
        letterObj.transform.localPosition = new Vector3(index * spaceBetweenLetters, 0, 0);


        var sr = letterObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.material = new Material(sr.material);

<<<<<<< Updated upstream
        if (LetterSpritesMap.TryGetValue(char.ToUpper(c), out Sprite sprite))
=======
        var isDevoured = textIndex >= 0 && _devouredIndices.Contains(textIndex);
        if (isDevoured)
        {
            ApplyDevouredVisualWorld(letterObj, sr, c);
            _letterObjects.Add(letterObj);
            return;
        }

        if (sr != null && LetterSpritesMap.TryGetValue(c, out var sprite))
>>>>>>> Stashed changes
        {
            sr.sprite = sprite;
            sr.material.SetTexture("_LetterText", sprite.texture);
        }

        _letterObjects.Add(letterObj);
    }

    /// <summary>
    /// Letra exacta tecleada (vía Keyboard.onTextInput). Comparación
    /// case-sensitive contra el texto esperado. Espacios, comas, puntos, etc.
    /// NO hay que teclearlos: se auto-avanzan como separadores (los textos
    /// no se modifican) y pulsar esas teclas no hace nada.
    /// Acumula en el libro sin hacer daño; el daño se envía con Enter.
    /// </summary>
    public void HandleTypedChar(char keyTyped)
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.isCombat) return;
        if (IsAutoSeparator(keyTyped)) return; // Separadores automáticos: ignorar.
        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;
        if (CountCompleteWords(_typedBuffer) >= maxWordsPerSubmit) return;

        AutoSkipSeparators();
        AutoSkipDevoured();
        if (QueueTextToScreen.Count == 0 || _letterObjects.Count == 0) return;

        char currentChar = QueueTextToScreen.Peek();

        if (keyTyped == currentChar) // tecla correcta (case-sensitive)
        {
<<<<<<< Updated upstream
            int indexForBook = _iteratorText;
            GameObject letterObj = _letterObjects[0];
=======
            var indexForBook = _queueTextIndices.Count > 0 ? _queueTextIndices.Peek() : _iteratorText;
            var letterObj = _letterObjects[0];
>>>>>>> Stashed changes

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

            for (int i = 0; i < _letterObjects.Count; i++)
                _letterObjects[i].transform.localPosition = new Vector3(i * spaceBetweenLetters, 0, 0);

            RefreshWordPreview();
        }
        else // tecla incorrecta (incluye case incorrecto)
        {
            SpriteRenderer sp = _letterObjects[0].GetComponent<SpriteRenderer>();
            sp.DOColor(Color.red, 0.125f).SetLoops(2, LoopType.Yoyo);
            CameraShake.Instance.CmrShake(0.55f, 0.50f);
            SpawnVFX(SpawnVFXBarra.transform.position, vfxMiss);
        }
    }

    /// <summary>
    /// Separadores que el jugador NO teclea: se auto-avanzan (espacios, comas,
    /// puntos...). Los textos no se modifican; solo cambia la validación.
    /// </summary>
    private static bool IsAutoSeparator(char c)
    {
        return c == ' ' || c == ',' || c == '.' || c == ';' || c == ':';
    }

    /// <summary>
    /// Consume automáticamente los separadores del texto esperado.
    /// Se destruye su letra en pantalla sin pasar por el libro (no hay sprites
    /// de separador) y se añade el caracter al buffer para delimitar palabras.
    /// El buffer siempre es text[cursor - buffer.Length .. cursor].
    /// </summary>
    private void AutoSkipSeparators()
    {
        while (QueueTextToScreen.Count > 0 && IsAutoSeparator(QueueTextToScreen.Peek()))
        {
<<<<<<< Updated upstream
            char sep = QueueTextToScreen.Dequeue();
=======
            var sep = QueueTextToScreen.Dequeue();
            if (_queueTextIndices.Count > 0) _queueTextIndices.Dequeue();
>>>>>>> Stashed changes
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

    /// <summary>Mundo (SpriteRenderer): '_' si hay sprite, si no fallback tipo guión.</summary>
    private void ApplyDevouredVisualWorld(GameObject letterObj, SpriteRenderer sr, char realChar)
    {
        if (sr == null) return;
        if (TryGetDevouredSprite(realChar, out var dash))
        {
            sr.sprite = dash;
            sr.material.SetTexture("_LetterText", dash.texture);
            sr.color = Color.white;
            return;
        }
        // Fallback sin asset '_': se oculta la letra real y se deja una barra (escala Y aplastada + tinte).
        if (LetterSpritesMap.TryGetValue(realChar, out var real))
        {
            sr.sprite = real;
            sr.material.SetTexture("_LetterText", real.texture);
        }
        sr.color = devouredTint;
        letterObj.transform.localScale = new Vector3(1f, 0.22f, 1f);
    }

    /// <summary>Libro (UI Image): '_' si hay sprite, si no fallback tipo guión.</summary>
    private void ApplyDevouredVisualBook(Image img)
    {
        if (img == null) return;
        if (TryGetDevouredSprite('_', out var dash))
        {
            img.sprite = dash;
            if (img.material != null && img.material.HasProperty("_LetterTexture"))
                img.material.SetTexture("_LetterTexture", dash.texture);
            img.color = Color.white;
            return;
        }
        img.color = devouredTint;
        var rt = img.GetComponent<RectTransform>();
        if (rt != null) rt.localScale = new Vector3(1f, 0.3f, 1f);
    }

    private bool TryGetDevouredSprite(char fallbackReal, out Sprite dash)
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
    /// Enter/Return: envía las palabras COMPLETAS del buffer.
    /// 1 palabra = 1 hit, 2 palabras = 2 hits, 3 palabras = 3 hits.
    /// La última palabra a medias NO se envía: queda en el buffer.
    /// </summary>
    public void HandleSubmit()
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.isCombat) return;

        int complete = CountCompleteWords(_typedBuffer);
        // Si la oración se acabó, la última palabra (sin espacio final
        // posible) cuenta como completa.
        if (_iteratorText >= textToCharList.Count
            && _typedBuffer.Length > 0
            && _typedBuffer[_typedBuffer.Length - 1] != ' ')
            complete++;
        int hits = Mathf.Min(complete, maxWordsPerSubmit);
        if (hits <= 0) return; // Nada completo todavía: se conserva el buffer.

        // Prefijo consumido; los espacios automáticos no tienen letra en el
        // libro, así que solo vuelan las letras no-espacio del prefijo.
        string consumed = RemoveSubmittedWords(hits);
        int bookLettersToFly = consumed.Count(c => c != ' ');
        FlyBookToEnemy(hits, bookLettersToFly);
        RefreshWordPreview();
    }

    /// <summary>Palabras completas en el buffer: tokens separados por espacio,
    /// sin contar una posible última palabra a medias (sin espacio final).</summary>
    private int CountCompleteWords(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        var tokens = s.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return 0;
        return s[s.Length - 1] == ' ' ? tokens.Length : tokens.Length - 1;
    }

    /// <summary>
    /// Elimina del buffer las primeras <paramref name="wordCount"/> palabras
    /// completas (con sus espacios separadores). Devuelve el prefijo consumido.
    /// </summary>
    private string RemoveSubmittedWords(int wordCount)
    {
        string s = _typedBuffer;
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
    /// Vuela al enemigo solo las letras doradas de las palabras enviadas
    /// y aplica el daño (1 por palabra). El resto del libro se recoloca.
    /// </summary>
    private void FlyBookToEnemy(int hits, int bookLettersToFly)
    {
        var enemy = CombatManager.Instance.enemy;
        Debug.Log($"[Typeo] Enter: {hits} palabra(s) -> {hits} hit(s). Enemigo: {(enemy != null ? enemy.name : "NULL")}");

        int toFly = Mathf.Min(bookLettersToFly, _lettersInBook.Count);
        var flying = _lettersInBook.GetRange(0, toFly);
        _lettersInBook.RemoveRange(0, toFly);
        _letterCount = Mathf.Max(0, _letterCount - toFly);
        for (int i = 0; i < _lettersInBook.Count; i++)
            if (_lettersInBook[i] != null)
                _lettersInBook[i].transform.localPosition =
                    new Vector3(i * spaceBetweenLetters, 0f, 0f);

        // Sin letras que volar (desync defensivo): aplicar daño directo.
        if (flying.Count == 0)
        {
            DealDamageToEnemy(hits);
            return;
        }

        Sequence seq = DOTween.Sequence();
        foreach (var letters in flying)
        {
            if (enemy != null && letters != null)
            {
                seq.Join(
                    letters.transform.DOMove(enemy.transform.position, 0.5f)
                        .SetEase(Ease.InFlash)
                        .OnComplete(() => { if (letters != null) Destroy(letters); })
                );
            }
            else if (letters != null)
            {
                Destroy(letters);
            }
        }

        seq.OnComplete(() => DealDamageToEnemy(hits));
    }

<<<<<<< Updated upstream
=======
    private void Detach(GameObject go)
    {
        if (go == null) return;
        var parent = letterTyped != null ? letterTyped.transform
            : (letterTypedContainer != null ? letterTypedContainer.transform : null);
        if (parent == null) { Destroy(go); return; }
        go.transform.SetParent(parent, true);
        go.transform.SetAsLastSibling(); // dibujar encima del resto
    }

>>>>>>> Stashed changes
    /// <summary>
    /// Aplica el daño al enemigo con chequeos y logs. Si falta PlayerAttack,
    /// daña directamente el EnemyHealthBase para no perder el hit.
    /// </summary>
    private void DealDamageToEnemy(int hits)
    {
        var cm = CombatManager.Instance;
        if (cm == null || cm.enemy == null || cm.player == null)
        {
            Debug.LogError($"[Typeo] Daño cancelado: enemy={(cm != null && cm.enemy != null ? "ok" : "NULL")}, player={(cm != null && cm.player != null ? "ok" : "NULL")}");
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
        int idx = _iteratorText;
        while (idx < textToCharList.Count && textToCharList[idx] == ' ') idx++;
        int start = idx, words = 0;
        while (idx < textToCharList.Count && words < wordCount)
        {
            if (textToCharList[idx] == ' ')
            {
                while (idx < textToCharList.Count && textToCharList[idx] == ' ') idx++;
                if (idx < textToCharList.Count) words++;
            }
<<<<<<< Updated upstream
            else idx++;
        }
        int len = Mathf.Min(idx, textToCharList.Count) - start;
        return len > 0 ? new string(textToCharList.GetRange(start, len).ToArray()) : "";
=======
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
>>>>>>> Stashed changes
    }


    private void AddQueueIfAvailable()
    {
        int nextIndex = _iteratorText + NumberOfCharsInScreen - 1;

        if (nextIndex < textToCharList.Count)
        {
            char nextChar = textToCharList[nextIndex];
            QueueTextToScreen.Enqueue(nextChar);
<<<<<<< Updated upstream
            SpawnLetter(nextChar, _letterObjects.Count, prefabLetter.transform.position);
=======
            _queueTextIndices.Enqueue(nextIndex);
            SpawnLetter(nextChar, _letterObjects.Count, nextIndex);
>>>>>>> Stashed changes
            Debug.Log($"Se agregó la letra: {nextChar}");
        }
    }


    private void AddTextInBook(GameObject letterToAdd, int index, bool isDevoured = false)
    {
        // Las devoradas ya salieron de _letterObjects, así que no se exige _letterObjects.Count > 0 en ese caso.
        if (!CombatManager.Instance.isCombat || index >= textToCharList.Count || (!isDevoured && _letterObjects.Count == 0))
        {
            Destroy(letterToAdd);
            return;
        }

<<<<<<< Updated upstream
        char currentChar = textToCharList[index];
=======
        var currentChar = textToCharList[index];
        isDevoured = isDevoured || _devouredIndices.Contains(index);
>>>>>>> Stashed changes

        // NOTA: el daño ya NO es por párrafo. Las letras solo se acumulan en el
        // libro y vuelan al enemigo en FlyBookToEnemy() al pulsar Enter.

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

        // Gula: el eco en el libro muestra '_' pero ocupa el mismo hueco del LayoutGroup.
        if (isDevoured) ApplyDevouredVisualBook(img);

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
        _queueTextIndices?.Clear();
        _letterObjects.Clear();
        _lettersInBook.Clear();
        _devouredIndices.Clear();
        GulaActive = false;
        _iteratorText = 0;
        _letterCount = 0;
        _typedBuffer = "";
        RefreshWordPreview();
    }
}