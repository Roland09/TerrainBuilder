namespace Rowlan.TerrainBuilder
{
    /// <summary>
    /// A NoiseType implementation for Value noise
    /// </summary>
    [System.Serializable]
    internal class ValueNoise : NoiseType<ValueNoise>
    {
        private static NoiseTypeDescriptor desc = new NoiseTypeDescriptor()
        {
            name = "Value",
            outputDir = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/NoiseLib",
            sourcePath = "Packages/com.rowlan.terrainbuilder/Editor/com.unity.terrain-tools/Shaders/NoiseLib/Implementation/ValueImpl.hlsl",
            supportedDimensions = NoiseDimensionFlags._1D | NoiseDimensionFlags._2D | NoiseDimensionFlags._3D,
            inputStructDefinition = null
        };

        public override NoiseTypeDescriptor GetDescription() => desc;
    }
}