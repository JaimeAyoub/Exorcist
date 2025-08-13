using UnityEditor.Rendering.Fullscreen.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    public Material blendMaterial;
    
    public Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.maxValue = 255;
        slider.minValue = 1;
        slider.value = blendMaterial.GetFloat("_PixelSize");
        
        blendMaterial.SetFloat("_PixelSize", 8.0f);
    }

    // Update is called once per frame
    void Update()
    {
        blendMaterial.SetFloat("_PixelSize", slider.value);
    }
}
