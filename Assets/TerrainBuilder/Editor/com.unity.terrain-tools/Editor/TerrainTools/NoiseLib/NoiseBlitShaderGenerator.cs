using UnityEngine;

namespace Rowlan.TerrainBuilder
{
    internal class NoiseBlitShaderGenerator : NoiseShaderGenerator<NoiseBlitShaderGenerator>
    {
        private static ShaderGeneratorDescriptor m_desc = new ShaderGeneratorDescriptor()
        {
            name = "NoiseBlit",
            shaderCategory = "Hidden/TerrainTools/Noise/NoiseBlit",
            outputDir = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/Generated/",
            templatePath = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/NoiseLib/Templates/Blit.noisehlsltemplate"
        };

        public override ShaderGeneratorDescriptor GetDescription() => m_desc;
    }
}