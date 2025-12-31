Shader "Custom/Stencil Mask"
{
    Properties
    {
        [IntegerRange] _Stencil("Stencil ID", Range(0, 255)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }

        Pass
        {
            Blend Zero One
            ZWrite Off

            Stencil
            {
                Ref [_Stencil]
                Comp Always
                Pass Replace
            }
        }
    }
}
