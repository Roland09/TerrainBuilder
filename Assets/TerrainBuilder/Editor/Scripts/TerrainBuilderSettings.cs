using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.TerrainTools
{
    /// <summary>
    /// Wrapper for the NoiseSettings which adds an event handling mechanism
    /// </summary>
    internal class TerrainBuilderSettings : NoiseSettings
    {
        public enum ContinuousUpdate
        {
            Manually,
            OnSettingsChange,
            OnEditorUpdateEvent
        }

        public ContinuousUpdate continuousUpdate = ContinuousUpdate.Manually;

        public void OnValidate()
        {
            // delay call, we aren't allowed to raise events in OnValidate()
            EditorApplication.delayCall += RaiseEvent;
        }

        public interface IChangeListener
        {
            public void OnEventRaised();
        }

        private List<IChangeListener> listeners = new List<IChangeListener>();

        public void RaiseEvent()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].OnEventRaised();
        }

        public void RegisterListener(IChangeListener listener)
        { listeners.Add(listener); }

        public void UnregisterListener(IChangeListener listener)
        { listeners.Remove(listener); }

        public void UnregisterAllListeners()
        { listeners.Clear(); }
    }
}
