using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class TextReader : MonoBehaviour
{
    
    public static TextReader instance;
    [Header("Referencias UI")] public TextMeshProUGUI fraseObjetivoText;
    public TMP_InputField fieldDeEscritura;
    public TextMeshProUGUI retroalimentacionText;

    [Header("Texto a leer")] public TextAsset paginasBibliaText;

    // Lista de frases cargadas del archivo
    private List<string> _frasesDisponibles;
    private string _fraseActual;
    private int _indiceFraseActual = 0;

    // Estado del sistema de typeo
    private bool _enModoEscritura = false;

    //Referencia del player para poder llamar sus funciones de hacer y recibir daño
    private GameObject _player;

    //Variables para la logica del player
    private bool _canSubstracTime = false;
    private bool _canAddTime = true;
    
    //Cosas para testear el sistema de combate, esto no es final
    private int _correctWords = 0;

    // Tecla para entrar en modo escritura TODO: encontrar manera de que se bloque la tecla para poder usarla en el modo de typeo
    public KeyCode teclaActivarModo = KeyCode.T;

    void Start()
    {
        if (fraseObjetivoText == null || fieldDeEscritura == null || retroalimentacionText == null ||
            paginasBibliaText == null)
        {
            Debug.LogError("Asegurate de asignar todas las referencias UI y el TextAsset en el Inspector");
            enabled = false;
            return;
        }

        _player = GameObject.FindGameObjectWithTag("Player");

        CargarFrasesDesdeArchivo();
        SeleccionarNuevaFrase();
        DesactivarModoEscritura(); // Empieza en modo normal
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Activar/desactivar el modo escritura
        if (Input.GetKeyDown(teclaActivarModo))
        {
            if (!_enModoEscritura)
            {
                ActivarModoEscritura();
            }
            else if(CombatManager.instance.isCombat == false)
            {
                DesactivarModoEscritura();
            }
        }

        // Compara el texto
        if (_enModoEscritura)
        {
            if (Input.anyKeyDown)
            {
                _canAddTime = true;
                _canSubstracTime = true;
                if (_correctWords == 1)
                {
                    _player.GetComponent<PlayerAttack>().Attack(1);
                    _correctWords = 0;
                }
                CompararTexto();
            }
        }
    }

    void CargarFrasesDesdeArchivo()
    {
        if (paginasBibliaText != null)
        {
            _frasesDisponibles = paginasBibliaText.text.Split('\n', '\r')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
            if (_frasesDisponibles.Count == 0)
            {
                Debug.LogError("El archivo vacio o no tiene frases validas.");
            }
            else
            {
                Debug.Log($"Se cargaron {_frasesDisponibles.Count} frases.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un Text Asset con texto.");
        }
    }

    void SeleccionarNuevaFrase()
    {
        if (_frasesDisponibles != null && _frasesDisponibles.Count > 0)
        {
            _fraseActual = _frasesDisponibles[_indiceFraseActual];
            _indiceFraseActual = (_indiceFraseActual + 1) % _frasesDisponibles.Count;

            fraseObjetivoText.text = _fraseActual;
            retroalimentacionText.text = "";
            fieldDeEscritura.text = "";
            fieldDeEscritura.ActivateInputField();
        }
        else
        {
            fraseObjetivoText.text = "No hay texto disponible.";
        }
    }

    void CompararTexto()
    {
        string textoJugador = fieldDeEscritura.text;

        if (textoJugador == _fraseActual)
        {
            retroalimentacionText.text = "�Correcto! Presiona '" + teclaActivarModo.ToString() +
                                         "' para la siguiente frase o salir.";
            fieldDeEscritura.DeactivateInputField();
            // TODO: Logica para puntos, terminar combate etc...
        }
        else
        {
            //string displayedText = "";
            //for (int i = 0; i < textoJugador.Length; i++)
            //{
            //    if (i < fraseActual.Length && textoJugador[i] == fraseActual[i])
            //    {
            //        displayedText += "<color=green>" + textoJugador[i] + "</color>";
            //    }
            //    else
            //    {
            //        displayedText += "<color=red>" + textoJugador[i] + "</color>";
            //    }
            //}
            //campoDeEscritura.text = displayedText; // �Cuidado! Esto sobrescribe lo que escribe el jugador

            // Una forma m�s simple es solo comparar la longitud para dar una pista visual
            if (textoJugador.Length > _fraseActual.Length)
            {
                retroalimentacionText.text = "Demasiado largo. Sigue escribiendo.";
            }
            else if (_fraseActual.StartsWith(textoJugador))
            {
                retroalimentacionText.text = "Sigue as�!!!";
                _correctWords += 1;
                if (_canAddTime && CombatManager.instance.currentTime <CombatManager.instance.MaxTime)
                {
                    CombatManager.instance.AddTime(0.5f);
                    _canAddTime = false;
                }
               

            }
            else
            {
                retroalimentacionText.text = "Error de typeo. Revisa lo que escribiste.";
                if (_canSubstracTime)
                {
                    CombatManager.instance.SubstracTime(0.5f);
                    Debug.Log("Se quito tiempo");
                    _canSubstracTime = false;
                }
            }
        }
    }

    public void ActivarModoEscritura()
    {
        _enModoEscritura = true;
        fieldDeEscritura.gameObject.SetActive(true);
        fraseObjetivoText.gameObject.SetActive(true);
        retroalimentacionText.gameObject.SetActive(true);

        SeleccionarNuevaFrase();
        fieldDeEscritura.ActivateInputField();
        Debug.Log("Modo escritura activado.");
    }

    public void DesactivarModoEscritura()
    {
        _enModoEscritura = false;
        fieldDeEscritura.gameObject.SetActive(false);
        fraseObjetivoText.gameObject.SetActive(false);
        retroalimentacionText.gameObject.SetActive(false);

        fieldDeEscritura.text = "";
        Debug.Log("Modo escritura desactivado.");
    }
}