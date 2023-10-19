using UnityEngine;
using NprGallery.Models;

namespace NprGallery
{
    public static class RenderingProvider
    {
        public static IConfig GetRenderingConfig(RenderingTechnique technique)
        {
            switch (technique)
            {
                case RenderingTechnique.Default: return GetOriginalShading();
                case RenderingTechnique.Toon: return GetToonShading();
                case RenderingTechnique.Gooch: return GetGoochShading();
                case RenderingTechnique.DuotoneSurface: return GetDuotoneSurface();
                case RenderingTechnique.RealtimeHatching: return GetRealtimeHatching();
                default: return GetOriginalShading();
            }
        }

        private static OriginalShadingConfig GetOriginalShading()
        {
            return new OriginalShadingConfig()
            {
                Technique = RenderingTechnique.Default,
                ShaderFile = string.Empty,
                DisplayName = "Default",
            };
        }

        private static ToonShadingConfig GetToonShading()
        {
            return new ToonShadingConfig()
            {
                Technique = RenderingTechnique.Toon,
                ShaderFile = "Unlit/Toon",
                DisplayName = "Toon Shading",
                Glossiness = 16f,
                BaseColor = new Color(255f, 255f, 255f, 255f),
                AmbientColor = new Color(102f, 102f, 102f, 255f),
                SpecularColor = new Color(230f, 230f, 230f, 255f),
            };
        }


        private static GoochShadingConfig GetGoochShading()
        {
            return new GoochShadingConfig()
            {
                Technique = RenderingTechnique.Gooch,
                ShaderFile = "Lit/Gooch",
                DisplayName = "Gooch Shading",
            };
        }

        private static DuotoneConfig GetDuotoneSurface()
        {
            return new DuotoneConfig()
            {
                Technique = RenderingTechnique.DuotoneSurface,
                ShaderFile = string.Empty,
                DisplayName = "Duotone",
            };
        }

        private static RealtimeHatchingConfig GetRealtimeHatching()
        {
            return new RealtimeHatchingConfig()
            {
                Technique = RenderingTechnique.RealtimeHatching,
                ShaderFile = "Unlit/Hatching",
                DisplayName = "Hatching",
            };
        }
    }
}
