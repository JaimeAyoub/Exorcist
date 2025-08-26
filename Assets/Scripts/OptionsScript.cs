
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    [Header("Opciones Shader para pixelear la pantalla")]
    public Material PixelationShaderMaterial;
    public Slider PixelationShaderSlider;
    [Header("Opciones de sonido")]
    public Slider SonidoSlider;
    [Header("Opciones sobre los efectos de PostProcessing")]
    public VolumeProfile volumeProfile;
    public Slider chromaticAberrationSlider;
    private ChromaticAberration _chromaticAberration;
    private FilmGrain _filmGrain;
    public  Slider filmGrainSlider;

    [SerializeField] private Image panelToFade;

    void Start()
    {
        if (volumeProfile.TryGet(out _chromaticAberration))
        {
            _chromaticAberration.intensity.value = chromaticAberrationSlider.value;
        }

        else
        {
            Debug.LogError("No chromatic aberration found");
        }
        if (volumeProfile.TryGet(out _filmGrain))
        {
            _filmGrain.intensity.value =  filmGrainSlider.value;
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
        panelToFade.color = currentColor;;
    }
}
