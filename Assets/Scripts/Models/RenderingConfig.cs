using UnityEngine;

namespace NprGallery.Models
{
    public interface IConfig
    {
        /// <summary>Identifier</summary>
        RenderingTechnique Technique { get; set; }
        /// <summary>Rendering name which display on the pistol</summary>
        string DisplayName { get; set; }
        /// <summary>Shader file's path</summary>
        string ShaderFile { get; set; }
    }

    public class OriginalShadingConfig : IConfig
    {
        public RenderingTechnique Technique { get; set; }
        public string DisplayName { get; set; }
        public string ShaderFile { get; set; }
    }

    public class ToonShadingConfig : IConfig
    {
        public RenderingTechnique Technique { get; set; }
        public string DisplayName { get; set; }
        public string ShaderFile { get; set; }

        // shader  properties
        public float Glossiness { get; set; }
        public Color BaseColor { get; set; }
        public Color AmbientColor { get; set; }
        public Color SpecularColor { get; set; }
    }

    public class DuotoneConfig : IConfig
    {
        public RenderingTechnique Technique { get; set; }
        public string DisplayName { get; set; }
        public string ShaderFile { get; set; }
    }

    public class GoochShadingConfig : IConfig
    {
        public RenderingTechnique Technique { get; set; }
        public string DisplayName { get; set; }
        public string ShaderFile { get; set; }
    }

    public class RealtimeHatchingConfig : IConfig
    {
        public RenderingTechnique Technique { get; set; }
        public string DisplayName { get; set; }
        public string ShaderFile { get; set; }
    }
}
