using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla final "Gracias por jugar": cuenta atrás y vuelve sola al menú
/// principal. Botones cableados desde la escena Gracias_por_Jugar.
/// </summary>
public class GraciasPorJugar : MonoBehaviour
{
    [Tooltip("Segundos antes de volver sola al menú principal.")]
    public float autoReturnSeconds = 30f;

    [Tooltip("Build index del menú principal.")]
    public int mainMenuBuildIndex = 0;

    private float _timer;

    private void Start()
    {
        _timer = autoReturnSeconds;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        _timer -= Time.unscaledDeltaTime;
        if (_timer <= 0f)
            GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
