using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startlights : MonoBehaviour {
//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------
//
//                  Simple Startlights Script - Select current state in Inspector or controll it by your own script
//
//                                                          by VIS-Games 2023
//
//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------

    public enum StartlightStatus 
    {
        Off,
        Go,
        Abort,
        StartAbort,
        StartPhase1,
        StartPhase2,
        StartPhase3,
        StartPhase4,
        StartPhase5,
    }
    public StartlightStatus startlightStatus = StartlightStatus.Off;

    public enum RenderPipeline
    {
        BuiltIn,
        URP,
        HDRP,
    }
    public RenderPipeline renderPipeline = RenderPipeline.BuiltIn;

    Renderer lightRenderer;
    Vector2 uvOffset;
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
void Start()
{
    lightRenderer = GetComponent<Renderer>();
    uvOffset = new Vector2(0.0f, 0.0f);
}
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
void Update()
{

    switch(startlightStatus)
    {
        case StartlightStatus.Off:        {uvOffset.y = 8 * -0.0625f;};break;
        case StartlightStatus.Go:         {uvOffset.y = 0 * -0.0625f;};break;
        case StartlightStatus.Abort:      {uvOffset.y = 1 * -0.0625f;};break;
        case StartlightStatus.StartAbort: {uvOffset.y = 2 * -0.0625f;};break;
        case StartlightStatus.StartPhase1:{uvOffset.y = 3 * -0.0625f;};break;
        case StartlightStatus.StartPhase2:{uvOffset.y = 4 * -0.0625f;};break;
        case StartlightStatus.StartPhase3:{uvOffset.y = 5 * -0.0625f;};break;
        case StartlightStatus.StartPhase4:{uvOffset.y = 6 * -0.0625f;};break;
        case StartlightStatus.StartPhase5:{uvOffset.y = 7 * -0.0625f;};break;

    }
    if(renderPipeline == RenderPipeline.BuiltIn)
        lightRenderer.materials[1].SetTextureOffset("_MainTex", uvOffset);
    else if(renderPipeline == RenderPipeline.URP)
        lightRenderer.materials[1].SetTextureOffset("_BaseMap", uvOffset);
    else // HDRP
    {
        lightRenderer.materials[1].SetTextureOffset("_BaseColorMap", uvOffset);
        lightRenderer.materials[1].SetTextureOffset("_EmissiveColorMap", uvOffset);
    }
}
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
}
