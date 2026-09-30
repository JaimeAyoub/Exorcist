using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NormalMapsAnimation : MonoBehaviour
{
    [SerializeField] private List<SpriteNormalPair> pairs;
    [SerializeField] private string normalMapProperty = "_NormalMap";
    private Sprite lastSprite;
    private Dictionary<Sprite, Texture2D> lookup;
    private Material materialInstance;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        materialInstance = sr.material; // instancia propia, no el asset compartido

        lookup = new Dictionary<Sprite, Texture2D>();
        foreach (var pair in pairs)
            if (pair.colorSprite != null && pair.normalMap != null)
                lookup[pair.colorSprite] = pair.normalMap;
    }

    private void LateUpdate()
    {
        // Se detecta el cambio de frame comparando contra el sprite actual del Animator/SpriteRenderer.
        if (sr.sprite == lastSprite) return;
        lastSprite = sr.sprite;

        if (sr.sprite != null && lookup.TryGetValue(sr.sprite, out var normalTex))
            materialInstance.SetTexture(normalMapProperty, normalTex);
    }

    [Serializable]
    public struct SpriteNormalPair
    {
        public Sprite colorSprite; // el frame de la hoja de color
        public Texture2D normalMap; // el recorte correspondiente de la hoja de normales
    }
}