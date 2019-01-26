// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;

namespace FluffyUnderware.DevTools
{

    public class DTSingleton<T> : MonoBehaviour, IDTSingleton where T : MonoBehaviour, IDTSingleton
    {
        static T _instance;
        static object _lock = new object();
        static bool applicationIsQuitting = false;
        bool isDuplicateInstance = false;

        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying)
                    applicationIsQuitting = false;
                if (applicationIsQuitting)
                    return null;

                if (_instance == null)
                    lock (_lock)
                        if (_instance == null)
                        {
                            Object[] objectsOfTypeT = FindObjectsOfType(typeof(T));

                            _instance = objectsOfTypeT.Length >= 1
                                ? (T)objectsOfTypeT[0]
                                : new GameObject().AddComponent<T>();
                        }

                return _instance;
            }
        }

        public virtual void Awake()
        {
            T instance = Instance;
            lock (_lock)
            {
                if (GetInstanceID() != instance.GetInstanceID())
                {
                    instance.MergeDoubleLoaded((IDTSingleton)this);
                    this.isDuplicateInstance = true;
                    this.Invoke("DestroySelf", 0);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            lock (_lock)
                if (Application.isPlaying && !isDuplicateInstance)
                {
                    applicationIsQuitting = true;
                    _instance = null;
                }
        }

        public virtual void MergeDoubleLoaded(IDTSingleton newInstance)
        {
        }

        void DestroySelf()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(this.gameObject);
            else
#endif
                Destroy(this.gameObject);
        }

    }

    public interface IDTSingleton
    {
        void MergeDoubleLoaded(IDTSingleton newInstance);
    }
}
