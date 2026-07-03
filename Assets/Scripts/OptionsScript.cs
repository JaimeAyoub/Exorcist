using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class OptionsScript : UnityUtils.Singleton<OptionsScript>
{
    [Header("Opciones Shader para pixelear la pantalla")]
    public Material PixelationShaderMaterial;

    public Slider PixelationShaderSlider;
    [Header("Opciones de sonido")] public Slider SonidoSlider;

    [Header("Opciones sobre los efectos de PostProcessing")]
    public VolumeProfile volumeProfile;

    public Slider chromaticAberrationSlider;
    public ChromaticAberration _chromaticAberration;
    private FilmGrain _filmGrain;
    public Slider filmGrainSlider;
    [Header("Opcion para sensibilidad ")] public PlayerMovement playerMovement;
    public Slider sensitivitySliderX;
    public Slider sensitivitySliderY;
    [SerializeField] private Image panelToFade;

    void Start()
    {
        if (volumeProfile.TryGet(out _chromaticAberration))
        {
            _chromaticAberration.intensity.value = chromaticAberrationSlider.value;
        }

        if (playerMovement != null)
        {
            playerMovement.xMouseSensitivity = sensitivitySliderX.value * 20;
            playerMovement.yMouseSensitivity = sensitivitySliderY.value * 20;
        }

        else
        {
            Debug.LogError("No chromatic aberration found");
        }

        if (volumeProfile.TryGet(out _filmGrain))
        {
            _filmGrain.intensity.value = filmGrainSlider.value;
        }
        else
        {
            Debug.LogError("No Grain found");
        }

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
        Color currentColor = panelToFade.color;
        currentColor.a = 0.0f;
        panelToFade.color = currentColor;
    }

    public void ChangeSensitivityinX()
    {
        playerMovement.xMouseSensitivity = sensitivitySliderX.value * 20;
    }

    public void changeSensitivityinY()
    {
        playerMovement.yMouseSensitivity = sensitivitySliderY.value * 20;
    }
}