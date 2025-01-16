using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gen
{
    public sealed class CameraController : MonoBehaviour
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public bool ReverseZoomDirection
        {
            get => PlayerPrefs.GetInt("reverse-zoom") == 0;
            set => PlayerPrefs.SetInt("reverse-zoom", value ? 0 : 1);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private Camera m_Camera;
        [SerializeField] private Transform m_Anchor;
        [SerializeField] private float m_MinZoom = 3;
        [SerializeField] private float m_MaxZoom = 10;
        [Tooltip("In FoV units per second.")]
        [SerializeField] private float m_MinZoomChangeSpeed = 8;
        [Tooltip("In FoV units per second.")]
        [SerializeField] private float m_BaseZoomChangeSpeed = 12;
        [SerializeField] private float m_ScrollZoomScale = 4f;
        [SerializeField] private UnityEvent<bool> m_OnReverseZoomInitEvent;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:
        private static readonly Vector3 DragMask = new(1, 1, 0);

        // Encapsulated Fields:

        // Local Fields:
        private Vector3 lastPosition;
        private float targetZoom;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void OnEnable()
        {
            lastPosition = Input.mousePosition;
            targetZoom = m_Camera.orthographicSize;
            m_OnReverseZoomInitEvent.Invoke(ReverseZoomDirection);
        }

        private void Update()
        {
            // Note: Works only in orthographic camera mode.
            if (!m_Camera.orthographic)
                throw new NotSupportedException("Camera Controller do not support orthographic camera mode.");

            UpdateMouse();
            UpdateFoV();
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>



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
        void UpdateMouse()
        {
            if (Input.GetKeyDown(Settings.DragKey))
            {
                lastPosition = Input.mousePosition;
            }

            if (Input.GetKey(Settings.DragKey))
            {
                Vector3 delta = m_Camera.ScreenToWorldPoint(Input.mousePosition) - m_Camera.ScreenToWorldPoint(lastPosition);
                Debug.Log($"{nameof(delta)}: {delta}");
                delta.x *= DragMask.x;
                delta.y *= DragMask.y;
                delta.z *= DragMask.z;
                m_Anchor.Translate(-delta, Space.World);
            }

            lastPosition = Input.mousePosition;
        }

        void UpdateFoV()
        {
            float axis = Input.GetAxis("Mouse ScrollWheel");
            if (!Mathf.Approximately(axis, 0F))
            {
                targetZoom = Mathf.Clamp(targetZoom + m_ScrollZoomScale * (ReverseZoomDirection ? axis : -axis), m_MinZoom, m_MaxZoom);
            }

            // FoV movement itself:
            if (!Mathf.Approximately(targetZoom, m_Camera.orthographicSize))
            {
                float delta = targetZoom - m_Camera.orthographicSize;
                m_Camera.orthographicSize += delta * Mathf.Lerp(0f, 1f, Mathf.Max(
                        a: Time.deltaTime * m_MinZoomChangeSpeed,
                        b: Time.deltaTime * m_BaseZoomChangeSpeed * Mathf.Max(1F, delta)));
            }
        }

#if UNITY_EDITOR
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Editor Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Reset()
        {
            TryGetComponent(out m_Camera);
            TryGetComponent(out m_Anchor);
        }
#endif
    }
}