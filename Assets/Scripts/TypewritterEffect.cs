using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(AudioSource))]
public class TypewriterWithShake : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private AudioSource audioSource;

    [Header("Configuración de Velocidad")]
    [SerializeField] private float delayBetweenCharacters = 0.05f;

    [Header("Efectos de Sonido")]
    [SerializeField] private AudioClip typingSound;
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 0.5f;
    [SerializeField] private int soundPlayInterval = 1;
    [SerializeField] private bool randomizePitch = true;

    [Header("Efecto de Temblor (Shake)")]
    [Tooltip("¿Quieres que el texto tiemble constantemente?")]
    [SerializeField] private bool enableShake = true;
    [Tooltip("Qué tanto se van a mover las letras.")]
    [SerializeField] private float shakeMagnitude = 2f;
    [Tooltip("Qué tan rápido vibran las letras.")]
    [SerializeField] private float shakeSpeed = 30f;

    private Coroutine typingCoroutine;
    private int currentlyVisibleCharacters = 0;
    private bool isTextFullyRevealed = false;

    public UnityEvent OnTypewriteComplete;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {

    }

    public void PlayText(string textToType)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        textMesh.text = textToType;
        isTextFullyRevealed = false;
        currentlyVisibleCharacters = 0;
        
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        textMesh.ForceMeshUpdate();
        int totalVisibleCharacters = textMesh.textInfo.characterCount;
        textMesh.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            currentlyVisibleCharacters = i;
            textMesh.maxVisibleCharacters = i;

            if (i > 0 && i <= totalVisibleCharacters)
            {
                char previousChar = textMesh.text[i - 1];
                if (previousChar != ' ' && i % soundPlayInterval == 0)
                {
                    PlayTypingSound();
                }
            }

            yield return new WaitForSeconds(delayBetweenCharacters);
        }

        isTextFullyRevealed = true;
        if (isTextFullyRevealed && OnTypewriteComplete != null)
        {
            yield return new WaitForSeconds(1.0f);
            OnTypewriteComplete.Invoke();
        }
    }

    // Update se encarga de mover los vértices frame por frame
    private void Update()
    {
        if (!enableShake) return;

        // Necesitamos asegurarnos de que la malla del texto exista
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        // Solo animamos los caracteres que ya son visibles en pantalla
        int loopLimit = isTextFullyRevealed ? textInfo.characterCount : currentlyVisibleCharacters;

        for (int i = 0; i < loopLimit; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Si es un espacio o caracter invisible, nos lo saltamos
            if (!charInfo.isVisible) continue;

            // Conseguimos el índice del material y de los vértices de esta letra
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

            // Generar un desfase aleatorio basado en el tiempo para esta letra
            // Usamos el índice 'i' para que no todas las letras se muevan idénticas hacia el mismo lado
            float seed = Time.time * shakeSpeed + (i * 40f);
            Vector3 offset = new Vector3(
                Mathf.Sin(seed) * Random.Range(-1f, 1f),
                Mathf.Cos(seed) * Random.Range(-1f, 1f),
                0
            ) * shakeMagnitude;

            // TextMeshPro mapea 4 vértices por letra: 0(Abajo-Izquierda), 1(Arriba-Izquierda), 2(Arriba-Derecha), 3(Abajo-Derecha)
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;
        }

        // Le avisamos a TextMeshPro que actualice la geometría con los nuevos vértices modificados
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    private void PlayTypingSound()
    {
        if (audioSource != null && typingSound != null)
        {
            audioSource.pitch = randomizePitch ? Random.Range(0.85f, 1.0f) : 1.0f;
            audioSource.PlayOneShot(typingSound, soundVolume);
        }
    }
}