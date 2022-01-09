using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Rowlan.TerrainBuilder
{
    /// <summary>
    /// An EditorWindow that enables the editing and previewing of NoiseSettings Assets
    /// </summary>
    internal class TerrainBuilderWindow : EditorWindow
    {
        /// <summary>
        /// Open a NoiseWindow with no source asset to load from
        /// </summary>
        [MenuItem("Window/Rowlan/Experimental/Terrain Builder")]
        public static TerrainBuilderWindow Open()
        {
            #region Rowlan Changes
            // we use our own scriptable object to detect changes
            NoiseSettings noise = ScriptableObject.CreateInstance<TerrainBuilderSettings>();
            #endregion Rowlan Changes

            return Open(noise);
        }

        /// <summary>
        /// Open a NoiseWindow that applies changes to a provided NoiseSettings asset and loads from a provided source NoiseSettings asset
        /// </summary>
        public static TerrainBuilderWindow Open(NoiseSettings noise, NoiseSettings sourceAsset = null)
        {
            if (HasOpenInstances<TerrainBuilderWindow>())
            {
                OnClose(GetWindow<TerrainBuilderWindow>());
            }

            TerrainBuilderWindow wnd = GetWindow<TerrainBuilderWindow>();
            wnd.titleContent = EditorGUIUtility.TrTextContent("Terrain Builder");
            wnd.rootVisualElement.Clear();
            var view = new NoiseEditorView(noise, sourceAsset);
            wnd.rootVisualElement.Add(view);

            wnd.noiseEditorView = view;
            wnd.m_noiseAsset = noise;
            wnd.minSize = new Vector2(550, 300);
            wnd.rootVisualElement.Bind(new SerializedObject(wnd.m_noiseAsset));
            wnd.rootVisualElement.viewDataKey = "TerrainBuilderWindow";

            #region Rowlan change
            AddTerrainBuilder(wnd, view);
            #endregion Rowlan change

            wnd.Show();
            wnd.Focus();

            return wnd;
        }

        static void OnClose(TerrainBuilderWindow wnd)
        {
            if (wnd == null) return;

            #region Rowlan Changes
            
            if(wnd.m_noiseAsset != null)
            {
                (wnd.m_noiseAsset as TerrainBuilderSettings).UnregisterAllListeners();
            }

            if (wnd.terrainBuilderView != null)
            {
                wnd.terrainBuilderView.OnDisable();
            }

            #endregion Rowlan Changes

            wnd.onDisableCallback?.Invoke();
            wnd.onDisableCallback = null;
            wnd.noiseEditorView?.OnClose();
        }

        private NoiseSettings m_noiseAsset;

        public NoiseEditorView noiseEditorView {
            get; private set;
        }

        void OnDisable()
        {
            OnClose(this);
        }

        public event Action onDisableCallback;

        #region Rowlan Changes

        public TerrainBuilderView terrainBuilderView
        {
            get; private set;
        }

        static void AddTerrainBuilder(TerrainBuilderWindow wnd, NoiseEditorView noiseEditorView)
        {
            noiseEditorView.style.position = Position.Relative;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlConstants.TERRAIN_BUILDER_WINDOW_UXML);
            VisualElement root = visualTree.Instantiate();

            // reparent the noise view
            wnd.rootVisualElement.Clear();
            wnd.rootVisualElement.Add(root);
            root.Q<VisualElement>(UxmlConstants.CONTENT_PANEL).Add(noiseEditorView);

            NoiseSettings settings = noiseEditorView.noiseUpdateTarget;
            
            TerrainBuilderView terrainBuilderView = new TerrainBuilderView(root, settings as TerrainBuilderSettings);
            wnd.terrainBuilderView = terrainBuilderView;

            (settings as TerrainBuilderSettings).RegisterListener(terrainBuilderView);
        }


        #endregion Rowlan Changes
    }
}