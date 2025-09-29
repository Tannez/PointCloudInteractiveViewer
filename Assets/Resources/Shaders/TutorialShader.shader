Shader "Unlit/TutorialShader"
{
    // https://www.youtube.com/watch?v=OrWBSN0yasQ 

    Properties // Variables that affect the material/shader
    {
        _Color("Test Color", color) = (1,1,1,1) //Vector 4 with 4 variables (RGBA)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" } // Rendering properties 
        LOD 100 // Level of Detail - use based on hardware capabilities - better hardware = more advanced shader code

        Pass
        {
            CGPROGRAM
            #pragma vertex vert // Runs on every single vertex
            #pragma fragment frag // Runs on every single pixel on screen

            #include "UnityCG.cginc" // Imports file with shader functions within Unity

            struct appdata // Mesh Data - data from the gameobject
            {
                float4 vertex : POSITION; //Position of each vertex within the gameobject
            };

            struct v2f // Pass data from vertex data to fragment -> draws things on screen
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) // takes vertex data from appdata
            {
                v2f o; // o = output going to fragment
                o.vertex = UnityObjectToClipPos(v.vertex); // Moves vertex data in the right position of the camera
                return o;
            }

            // Difference between datatypes (fixed,half,float) is 
            // the precision and range:
            // fixed: low precision + low range -> used for color (4 bits) 
            // half: has more precision
            // float: most precise

            fixed4 frag (v2f i) : SV_Target
            {
                half4 someValue = half4(1,1,1,1);
                fixed4 col = 1;
                return col;
            }
            ENDCG
        }
    }
}
