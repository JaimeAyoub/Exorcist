
using UnityEngine;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    [Header("Opciones Shader para pixelear la pantalla")]
    public Material PixelationShaderMaterial;
    public Slider PixelationShaderSlider;
    [Header("Opciones de sonido")]
    public Slider SonidoSlider;
    [Header("Opciones sobre los efectos de PostProcessing")]
    private 

    void Start()
    {
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
}
