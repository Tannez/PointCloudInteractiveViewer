Shader "CustomRenderTexture/StencilMask"
{
    // Shader Code provided in https://www.youtube.com/watch?v=y-SEiDTbszk
    // intended for URP, so might not work

    Properties 
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 0
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry-1" }

        Pass
        {
            Blend Zero One
            ZWrite Off

            Stencil
            {
                Ref[_StencilID]
                Comp Always
                Pass Replace
            }
        }
    }
}
