Shader "Discone/SurfaceColor" {
    SubShader {
        Tags {
            "Surface" = "True"
        }

        UsePass "Custom/Incline/SURFACE"
    }
}