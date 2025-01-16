using CRUD;
using Gen.Networking;
using OptiLib.Animation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gen
{
    public sealed class Manager : MonoBehaviour
    {
        public static Manager Instance { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:
        public event Action OnFaded
        {
            add => OnFadedList.Add(value);
            remove => OnFadedList.Remove(value);
        }

        // Properties:
        public static (bool succeeded, string message) LastCommunicationResult { get; set; }
        public bool IsDebugging => m_IsDebugging;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private CanvasGroup m_FadeGroup;
        [SerializeField] private PointCurve m_FadeCurve = new() { curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f) };

        [Header("Debugging")]
        [SerializeField] private bool m_IsDebugging = false;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:
        private static readonly List<Action> OnFadedList = new();


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            DontDestroyOnLoad(Instance = this);

            // Setups animations
            m_FadeCurve.action += (alpha) => m_FadeGroup.alpha = alpha;

            // DB init.
            DB.Init();
        }

        private void Update()
        {
            if (!Instance.IsDebugging) return;
            Debug.Log($"{nameof(IDatabaseProcessor.DebugMessage)}: {IDatabaseProcessor.DebugMessage}");
            Debug.Log($"[{IDatabaseProcessor.DebugIncrement}] {nameof(LastCommunicationResult)}: {LastCommunicationResult}");
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void Transition(Action onFaded)
        {
            m_FadeCurve.Start(EndAction);
            void EndAction()
            {
                m_FadeCurve.StartReverse();
                onFaded?.Invoke();
            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Implementations
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>


#if UNITY_EDITOR
        private void Reset() => m_FadeCurve.CustomExecutor = this;
#endif
    }
}
