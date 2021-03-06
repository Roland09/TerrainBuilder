namespace Rowlan.TerrainBuilder
{
    /// <summary>
    /// A NoiseType implementation for Perlin noise
    /// </summary>
    [System.Serializable]
    internal class PerlinNoise : NoiseType<PerlinNoise>
    {
        private static NoiseTypeDescriptor desc = new NoiseTypeDescriptor()
        {
            name = "Perlin",
            outputDir = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/NoiseLib",
            sourcePath = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/NoiseLib/Implementation/PerlinImpl.hlsl",
            supportedDimensions = NoiseDimensionFlags._1D | NoiseDimensionFlags._2D | NoiseDimensionFlags._3D,
            inputStructDefinition = null
        };

        public override NoiseTypeDescriptor GetDescription() => desc;
    }
}