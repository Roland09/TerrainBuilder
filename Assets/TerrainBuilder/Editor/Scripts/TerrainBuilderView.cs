using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using static Rowlan.TerrainBuilder.TerrainBuilderSettings;

namespace Rowlan.TerrainBuilder
{
    internal class TerrainBuilderView : BindableElement, TerrainBuilderSettings.IChangeListener
    {
        private TerrainBuilderSettings noiseSettings;

        #region GUI
        EnumField targetTerrainField;
        EnumField continuousUpdateField;
        #endregion GUI

        /// <summary>
        /// Flag to indicate that the terrain should be recreated
        /// </summary>
        private bool shouldUpdateTerrain = false;

        public TerrainBuilderView(VisualElement root, TerrainBuilderSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;

            // target terrain
            targetTerrainField = root.Query<EnumField>(UxmlConstants.TARGET_TERRAIN_ENUM);
            targetTerrainField.Init( noiseSettings.targetTerrain);
            targetTerrainField.RegisterValueChangedCallback((evt) => noiseSettings.targetTerrain = (TargetTerrain)(evt.newValue as System.Enum));

            // continuous enum
            continuousUpdateField = root.Query<EnumField>(UxmlConstants.UPDATE_STRATEGY_ENUM);
            continuousUpdateField.Init( noiseSettings.continuousUpdate);
            continuousUpdateField.RegisterValueChangedCallback((evt) => noiseSettings.continuousUpdate = (ContinuousUpdate) (evt.newValue as System.Enum));

            // create terrain button
            Button createTerrainButton = root.Query<Button>(UxmlConstants.CREATE_TERRAIN_BUTTON);
            createTerrainButton.clickable.clicked += () => ButtonClicked();

            AddUpdateListener();
        }

        private void ButtonClicked()
        {
            CreateTerrain();
        }

        private void CreateTerrain()
        {
            // TODO: remove
            Debug.Log("Creating terrain " + Time.time);

            // get the active terrain and ensure a terrain exists
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
                return;

            // get all terrains
            Terrain[] terrains = UnityEngine.Object.FindObjectsOfType<Terrain>().ToArray();

            // get the grouping ID from the active/selected Terrain
            int groupingID = terrain.groupingID; 

            // get map of terrains of the same group in order to retrieve tile indexes
            UnityEngine.TerrainUtils.TerrainMap map = UnityEngine.TerrainUtils.TerrainMap.CreateFromPlacement(terrain, (t) => { return t.groupingID == groupingID; });

            // apply noise to the individual tiles
            foreach (KeyValuePair<UnityEngine.TerrainUtils.TerrainTileCoord, Terrain> pair in map.terrainTiles)
            {
                if (!IsInFilter(pair.Value.transform))
                    continue;

                // get the tile coordinates
                int tileX = pair.Key.tileX;
                int tileZ = pair.Key.tileZ;

                TerrainData terrainData = pair.Value.terrainData;

                int sizeX = terrainData.heightmapResolution;
                int sizeY = terrainData.heightmapResolution;

                // create a clone of the noise settings for every tile and modify it accordingly
                NoiseSettings tileNoiseSettings = ScriptableObject.CreateInstance<NoiseSettings>();
                tileNoiseSettings.Copy(noiseSettings);

                // problem: the terrain tiles didn't align properly, there were always gaps
                // tried to find out the difference by simply aligning the tiles manually, got those numbers:
                // scale 1 => correct: 0.998
                // scale 10 => correct: 9.998
                // => multiplying  the scale with that number worked for the other scales
                // need to find out why there's this difference; leaving it as it is for now, no time to evaluate the cause
                float magicNumber = 0.998f;

                //cs.transformSettings.translation.z += ff.value;
                tileNoiseSettings.transformSettings.translation.x += tileNoiseSettings.transformSettings.scale.x * tileX * magicNumber;
                tileNoiseSettings.transformSettings.translation.z += tileNoiseSettings.transformSettings.scale.z * tileZ * magicNumber;

                // create heightmap using the noise
                ApplyToTerrain(terrainData, tileNoiseSettings, sizeX, sizeY);

            }
        }

        /// <summary>
        /// Filter by target terrain selection
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private bool IsInFilter( Transform transform )
        {
            switch(noiseSettings.targetTerrain)
            {
                case TargetTerrain.Active:
                    return transform.gameObject == Terrain.activeTerrain.gameObject;

                case TargetTerrain.All:
                    return true;

                case TargetTerrain.Selected:
                    return Selection.gameObjects.Contains(transform.gameObject);

                default:
                    throw new Exception( $"Unsupported target terrain {noiseSettings.targetTerrain}");
            }
        }

        private void ApplyToTerrain(TerrainData terrainData, NoiseSettings noiseSettings, int width, int height)
        {
            RenderTexture prev = RenderTexture.active;
            {
                RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, GraphicsFormat.R16_UNorm);
                RenderTexture.active = rt;

                NoiseUtils.Blit2D(noiseSettings, rt);

                TerrainBuilderHelper.CopyTextureToTerrainHeight(terrainData, rt, Vector2Int.zero, rt.width, 1, 0f, 1f);

                RenderTexture.ReleaseTemporary(rt);
            }
            RenderTexture.active = prev;
        }

        public void OnEventRaised()
        {
            if (noiseSettings.continuousUpdate != ContinuousUpdate.Manually)
            {
                shouldUpdateTerrain = true;
            }
        }

        private void AddUpdateListener()
        {
            EditorApplication.update -= UpdateTerrain;
            EditorApplication.update += UpdateTerrain;
        }

        private void RemoveUpdateListener()
        {
            EditorApplication.update -= UpdateTerrain;
        }

        public void OnDisable()
        {
            RemoveUpdateListener();
        }

        public void UpdateTerrain()
        {
            switch(noiseSettings.continuousUpdate)
            {
                case ContinuousUpdate.Manually:
                    // nothing to do
                    break;

                case ContinuousUpdate.OnSettingsChange:
                    if(shouldUpdateTerrain)
                    {
                        CreateTerrain();
                        shouldUpdateTerrain = false;
                    }
                    break;

                case ContinuousUpdate.OnEditorUpdateEvent:
                    // TODO: this really is too many updates, but it's super smooth if the terrain is small enough; leaving it for now as it is
                    CreateTerrain();
                    shouldUpdateTerrain = false;
                    break;
            }
        }
    }
}
