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
    private const float SensitivityMin = 0f;
    private const float SensitivityMax = 3f;
    private const float SensitivityDefault = 1.5f; // mitad del slider
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

        if (playerMovement != null)
        {
            NormalizeSensitivitySliders();
            ApplySensitivity();
        }

        else
        {
            Debug.LogError("No chromatic aberration found");
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
        if (playerMovement == null || sensitivitySliderX == null) return;
        playerMovement.xMouseSensitivity = sensitivitySliderX.value;
    }

    public void changeSensitivityinY()
    {
        if (playerMovement == null || sensitivitySliderY == null) return;
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

    public void AnimateEffects()
    {
        if (colorAdjustments != null)
        {
        }
    }
}