using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityUtils;

public class OptionsScript : Singleton<OptionsScript>
{
    [Header("Opciones Shader para pixelear la pantalla")]
    public Material PixelationShaderMaterial;

    public Slider PixelationShaderSlider;
    [Header("Opciones de sonido")] public Slider SonidoSlider;

    [Header("Opciones sobre los efectos de PostProcessing")]
    public VolumeProfile volumeProfile;

    public Slider chromaticAberrationSlider;

    public Slider filmGrainSlider;
    public Slider colorAdjustSlider;


    [Header("Opcion para sensibilidad ")] public PlayerMovement playerMovement;
    public Slider sensitivitySliderX;
    public Slider sensitivitySliderY;

    // Mapeo único: sensibilidad = valor del slider, sin factores distintos
    // entre Start y los callbacks (ese x20 solo en Start era el bug).
    // Se guarda en PlayerPrefs para que valga igual en menú y en nivel.
    private const float SensitivityMin = 0f;
    private const float SensitivityMax = 3f;
    private const float SensitivityDefault = 1.5f; // mitad del slider
    private const string PrefSensX = "sensX";
    private const string PrefSensY = "sensY";
    [SerializeField] private Image panelToFade;

    [Header("Efectos de postprocesado")] public ChromaticAberration _chromaticAberration;
    public FilmGrain _filmGrain;
    public ColorAdjustments colorAdjustments;


    private void Start()
    {
        if (volumeProfile.TryGet(out _chromaticAberration))
        {
            //_chromaticAberration.intensity.value = chromaticAberrationSlider.value;
        }

        // Los sliders se normalizan SIEMPRE (aunque no haya player en esta
        // escena, ej. menú principal); solo el Apply necesita playerMovement.
        NormalizeSensitivitySliders();
        LoadSensitivityPrefs();
        ApplySensitivity();

        if (playerMovement == null)
        {
            Debug.Log("OptionsScript sin playerMovement en esta escena (ej. menú): solo se configuran sliders.");
        }

        if (volumeProfile.TryGet(out _filmGrain))
            //_filmGrain.intensity.value = filmGrainSlider.value;


            PixelationShaderSlider.maxValue = 8;
        PixelationShaderSlider.minValue = 3;
        PixelationShaderSlider.value = PixelationShaderMaterial.GetFloat("_PixelSize");
    }


    public void ChangeShader()
    {
        PixelationShaderMaterial.SetFloat("_PixelSize", PixelationShaderSlider.value);
    }

    public void ChangeSound()
    {
        if (AudioManager.instance == null || AudioManager.instance.audioSource == null) return;
        AudioManager.instance.audioSource.volume = SonidoSlider.value;
    }

    public void ChangeChromaticAberration()
    {
        _chromaticAberration.intensity.value = chromaticAberrationSlider.value;
    }

    public void ChangeGrain()
    {
        _filmGrain.intensity.value = filmGrainSlider.value;
    }

    public void ChangeAlphaPanel()
    {
        var currentColor = panelToFade.color;
        currentColor.a = 0.0f;
        panelToFade.color = currentColor;
    }

    public void ChangeSensitivityinX()
    {
        if (sensitivitySliderX == null) return;
        PlayerPrefs.SetFloat(PrefSensX, sensitivitySliderX.value);
        PlayerPrefs.Save();
        if (playerMovement == null) return;
        playerMovement.xMouseSensitivity = sensitivitySliderX.value;
    }

    public void changeSensitivityinY()
    {
        if (sensitivitySliderY == null) return;
        PlayerPrefs.SetFloat(PrefSensY, sensitivitySliderY.value);
        PlayerPrefs.Save();
        if (playerMovement == null) return;
        playerMovement.yMouseSensitivity = sensitivitySliderY.value;
    }

    /// <summary>Rango único + valor medio por defecto, para que el slider
    /// nazca a la mitad sin depender de cómo quedó configurado en escena.</summary>
    private void NormalizeSensitivitySliders()
    {
        if (sensitivitySliderX != null)
        {
            sensitivitySliderX.minValue = SensitivityMin;
            sensitivitySliderX.maxValue = SensitivityMax;
            if (sensitivitySliderX.value <= Mathf.Epsilon)
                sensitivitySliderX.value = SensitivityDefault;
        }

        if (sensitivitySliderY != null)
        {
            sensitivitySliderY.minValue = SensitivityMin;
            sensitivitySliderY.maxValue = SensitivityMax;
            if (sensitivitySliderY.value <= Mathf.Epsilon)
                sensitivitySliderY.value = SensitivityDefault;
        }
    }

    private void ApplySensitivity()
    {
        if (playerMovement == null) return;
        if (sensitivitySliderX != null)
            playerMovement.xMouseSensitivity = sensitivitySliderX.value;
        if (sensitivitySliderY != null)
            playerMovement.yMouseSensitivity = sensitivitySliderY.value;
    }

    /// <summary>Si el jugador ya ajustó antes, los sliders nacen con su valor.</summary>
    private void LoadSensitivityPrefs()
    {
        if (sensitivitySliderX != null && PlayerPrefs.HasKey(PrefSensX))
            sensitivitySliderX.value = PlayerPrefs.GetFloat(PrefSensX, SensitivityDefault);
        if (sensitivitySliderY != null && PlayerPrefs.HasKey(PrefSensY))
            sensitivitySliderY.value = PlayerPrefs.GetFloat(PrefSensY, SensitivityDefault);
    }

    public void AnimateEffects()
    {
        if (colorAdjustments != null)
        {
        }
    }
}