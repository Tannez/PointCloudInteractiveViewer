Shader "Unlit/HideObject"
{
    // Inspiration: https://www.youtube.com/watch?v=iu6Do4nU1xw 
    SubShader
    {
        // Render the mask after regular geometry, but before masked geometry and
        // transparent things.

        Tags {"Queue" = "Geometry-1"}

        // Don't draw in the RGBA channels; just the depth buffer

        ColorMask 0 
        ZWrite On
        Cull Back 
        ZTest LEqual 

        // Do nothing specific in the pass:

        Pass {}
    }
    


}

        