using UnityEngine;

/// <summary>
/// Shake sutil para una letra dorada de UI. Se pone en el prefab.
///
/// - Rotación: siempre funciona, aunque la letra esté dentro de un LayoutGroup.
/// - Posición: solo se aplica si 'visual' es un HIJO del objeto con el
///   LayoutElement. Si 'visual' es el propio objeto, se ignora para no pelear
///   con el layout.
///
/// Estructura recomendada del prefab si quieres shake de posición:
///   LetterGoldPrefab   (RectTransform + LayoutElement + LetterShake)
///   └── Visual         (RectTransform + Image)   <- arrastrar aquí a 'visual'
/// </summary>
public class LetterShake : MonoBehaviour
{
    [SerializeField] private RectTransform visual;      // Si es null, usa este mismo objeto
    [SerializeField] private float angle = 5f;          // Grados de rotación máxima
    [SerializeField] private float positionAmountX = 2f; // Píxeles (solo si visual es hijo)
    [SerializeField] private float positionAmountY = 2f; // Píxeles (solo si visual es hijo)
    [SerializeField] private float speed = 18f;         // Frecuencia del temblor

    private float _seed;
    private Vector2 _basePos;
    private Quaternion _baseRot;
    private bool _canMove;

    private void Awake()
    {
        if (visual == null) visual = (RectTransform)transform;
        _canMove = visual != (RectTransform)transform;
        _seed = Random.value * 100f; // cada letra con su propio ritmo
        _baseRot = visual.localRotation;
        _basePos = visual.anchoredPosition;
        enabled = false; // sin coste mientras no tiembla
    }

    public void StartShake()
    {
        _baseRot = visual.localRotation;
        _basePos = visual.anchoredPosition;
        enabled = true;
    }

    public void StopShake()
    {
        enabled = false;
        if (visual == null) return;
        visual.localRotation = _baseRot;
        if (_canMove) visual.anchoredPosition = _basePos;
    }

    private void Update()
    {
        float t = Time.time * speed + _seed;

        // PerlinNoise devuelve 0..1 -> lo pasamos a -1..1
        float nz = Mathf.PerlinNoise(t, 0f) * 2f - 1f;
        visual.localRotation = _baseRot * Quaternion.Euler(0f, 0f, nz * angle);

        if (_canMove)
        {
            float nx = Mathf.PerlinNoise(0f, t) * 2f - 1f;
            float ny = Mathf.PerlinNoise(t, t) * 2f - 1f;
            nx *= positionAmountX;
            ny *= positionAmountY;
            visual.anchoredPosition = _basePos + new Vector2(nx, ny);
        }
    }

    private void OnDisable()
    {
        // Si el objeto se desactiva, deja todo en su sitio.
        if (visual == null) return;
        visual.localRotation = _baseRot;
        if (_canMove) visual.anchoredPosition = _basePos;
    }
}
