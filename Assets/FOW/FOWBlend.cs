using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FOWBlend : MonoBehaviour
{
    [SerializeField]
    private Texture sourceTexture;

    public bool clear;
    
    [SerializeField] private RenderTexture destRenderTexture;
    [SerializeField] private Material mat;
    
    
    private void Awake()
    {
        Graphics.Blit( sourceTexture, destRenderTexture );
    }
    

    private void Update()
    {
        //blit the render texture onto the persistent texture with a blend shader to store what has been seen
        Graphics.Blit( sourceTexture, destRenderTexture, mat );
        
        if ( clear )
        {
            ClearTexture();
            clear = false;
        }
    }

    private void ClearTexture()
    {
        GL.Clear( true, true, Color.black );
    }
    
}
