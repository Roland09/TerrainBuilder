using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.TerrainTools.TerrainBuilderSettings;

namespace UnityEditor.TerrainTools
{
    internal class TerrainBuilderView : BindableElement, TerrainBuilderSettings.IChangeListener
    {
        private TerrainBuilderSettings noiseSettings;

        #region GUI
        EnumField continuousUpdateField;
        #endregion GUI

        /// <summary>
        /// Flag to indicate that the terrain should be recreated
        /// </summary>
        private bool shouldUpdateTerrain = false;

        public TerrainBuilderView(TerrainBuilderSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;

            // load stylesheet
            styleSheets.Add(Resources.Load<StyleSheet>(Styles.USS_FILE));


            // toolbar
            VisualElement toolbar = new VisualElement();
            toolbar.AddToClassList(Styles.TOOLBAR);

            // continuous toggle
            continuousUpdateField = new EnumField("Continuous Update", noiseSettings.continuousUpdate);
            continuousUpdateField.RegisterValueChangedCallback((evt) => noiseSettings.continuousUpdate = (ContinuousUpdate) (evt.newValue as System.Enum));
            continuousUpdateField.AddToClassList(Styles.TOOLBAR_CONTINUOUS_UPDATE_ENUMFIELD);
            toolbar.Add(continuousUpdateField);

            // create terrain button
            Button createTerrainButton = new Button() { text = "Create Terrain" };
            createTerrainButton.AddToClassList(Styles.TOOLBAR_CREATE_TERRAIN_BUTTON);
            createTerrainButton.clickable.clicked += () => ButtonClicked();
            toolbar.Add(createTerrainButton);

            Label warningLabel = new Label("Warning! This will change your terrain. Create a Backup!");
            warningLabel.AddToClassList(Styles.TOOLBAR_WARNING_LABEL);
            toolbar.Add(warningLabel);

            Add(toolbar);

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

            Terrain[] terrains = UnityEngine.Object.FindObjectsOfType<Terrain>().ToArray();

            foreach (var terrain in terrains)
            {
                // TODO: multi-tile terrain; currently all terrains get the same noise
                int sizeX = terrain.terrainData.heightmapResolution;
                int sizeY = terrain.terrainData.heightmapResolution;

                ApplyToTerrain(terrain.terrainData, noiseSettings, sizeX, sizeY);

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
