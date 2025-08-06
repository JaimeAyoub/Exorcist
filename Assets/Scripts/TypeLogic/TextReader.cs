using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TextReader : MonoBehaviour
{
    [Header ("Referencias UI")]
    public TextMeshProUGUI fraseObjetivoText;
    public TMP_InputField fieldDeEscritura;
    public TextMeshProUGUI retroalimentacionText;

    [Header ("Texto a leer")]
    public TextAsset paginasBibliaText;

    // Lista de frases cargadas del archivo
    private List<string> frasesDisponibles;
    private string fraseActual;
    private int indiceFraseActual = 0;

    // Estado del sistema de typeo
    private bool enModoEscritura = false;

    // Tecla para entrar en modo escritura TODO: encontrar manera de que se bloque la tecla para poder usarla en el modo de typeo
    public KeyCode teclaActivarModo = KeyCode.T;

    void Start()
    {
        if (fraseObjetivoText == null || fieldDeEscritura == null || retroalimentacionText == null || paginasBibliaText == null)
        {
            Debug.LogError("Asegurate de asignar todas las referencias UI y el TextAsset en el Inspector");
            enabled = false;
            return;
        }

        CargarFrasesDesdeArchivo();
        SeleccionarNuevaFrase();
        DesactivarModoEscritura(); // Empieza en modo normal
    }

    void Update()
    {
        // Activar/desactivar el modo escritura
        if (Input.GetKeyDown(teclaActivarModo))
        {
            if (!enModoEscritura)
            {
                ActivarModoEscritura();
            }
            else
            {
                DesactivarModoEscritura();
            }
        }

        // Compara el texto
        if (enModoEscritura)
        {
            CompararTexto();
        }
    }

    void CargarFrasesDesdeArchivo()
    {
        if (paginasBibliaText != null)
        {
            frasesDisponibles = paginasBibliaText.text.Split('\n','\r')
                                                    .Select(s => s.Trim())
                                                    .Where(s => !string.IsNullOrEmpty(s))
                                                    .ToList();
            if (frasesDisponibles.Count == 0)
            {
                Debug.LogError("El archivo vacio o no tiene frases validas.");
            }
            else
            {
                Debug.Log($"Se cargaron {frasesDisponibles.Count} frases.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un Text Asset con texto.");
        }
    }

    void SeleccionarNuevaFrase()
    {
        if (frasesDisponibles != null && frasesDisponibles.Count > 0)
        {
            fraseActual = frasesDisponibles[indiceFraseActual];
            indiceFraseActual = (indiceFraseActual + 1) % frasesDisponibles.Count;

            fraseObjetivoText.text = fraseActual;
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

        if (textoJugador == fraseActual)
        {
            retroalimentacionText.text = "¡Correcto! Presiona '" + teclaActivarModo.ToString() + "' para la siguiente frase o salir.";
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
            //campoDeEscritura.text = displayedText; // ¡Cuidado! Esto sobrescribe lo que escribe el jugador

            // Una forma más simple es solo comparar la longitud para dar una pista visual
            if (textoJugador.Length > fraseActual.Length)
            {
                retroalimentacionText.text = "Demasiado largo. Sigue escribiendo.";
            }
            else if (fraseActual.StartsWith(textoJugador))
            {
                retroalimentacionText.text = "Sigue así!!!";
            }
            else
            {
                retroalimentacionText.text = "Error de typeo. Revisa lo que escribiste.";
            }
        }
    }

    void ActivarModoEscritura()
    {
        enModoEscritura = true;
        fieldDeEscritura.gameObject.SetActive(true);
        fraseObjetivoText.gameObject.SetActive(true);
        retroalimentacionText.gameObject.SetActive(true);

        SeleccionarNuevaFrase();
        fieldDeEscritura.ActivateInputField();
        Debug.Log("Modo escritura activado.");
    }

    void DesactivarModoEscritura()
    {
        enModoEscritura = false;
        fieldDeEscritura.gameObject.SetActive(false);
        fraseObjetivoText.gameObject.SetActive(false);
        retroalimentacionText.gameObject.SetActive(false);

        fieldDeEscritura.text = "";
        Debug.Log("Modo escritura desactivado.");
    }
}