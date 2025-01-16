using OptiLib.Animation;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gen.UI
{
    public sealed class MessagePopup : MonoBehaviour
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Singleton Implementation
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static MessagePopup Instance { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private TMP_Text m_MessageText;
        [SerializeField] private PointCurve m_MovementCurve;
        [SerializeField] private AnimationCurve m_AlphaCurve;
        [SerializeField] private float m_FastSpeed = 1.6f;
        [SerializeField] private Vector2 m_Delta;

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:

        // Local Fields:
        private readonly Queue<string> messages = new();
        private RectTransform anchor;
        private Vector2 initPosition;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            Instance = this;
            anchor = (RectTransform)transform;
            initPosition = anchor.anchoredPosition;
            m_MovementCurve.action = (movement) =>
            {
                anchor.anchoredPosition = initPosition + m_Delta * movement;
                m_Group.alpha = m_AlphaCurve.Evaluate(m_MovementCurve.time / m_MovementCurve.Duration);
            };
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void Show(string message, bool allowRepeating = false)
        {
            if (!allowRepeating && messages.Contains(message)) return;
            messages.Enqueue(message);
            ShowNext();
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
        void ShowNext()
        {
            if (!m_MovementCurve.IsIdle)
            {
                m_MovementCurve.speed = m_FastSpeed;
                return;
            }

            if (messages.TryPeek(out string message)) return;
            m_MovementCurve.speed = messages.Count <= 1 ? 1F : m_FastSpeed;
            m_MovementCurve.Start(HandleAnimationEnd);
            m_MessageText.text = message;

            // Local:
            void HandleAnimationEnd()
            {
                // Finalizing current message.
                // Finalizing here, so message can be checked for repeats within a queue.
                _ = messages.Dequeue();
                ShowNext();
            }
        }
    }
}
