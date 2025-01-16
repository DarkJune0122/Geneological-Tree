using OptiLib;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Gen
{
    public class Node : MonoBehaviour
    {
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
        [SerializeField] private Canvas m_Visuals;
        [Header("Animation Settings")]
        [SerializeField] private float m_MaxAngle = 20.0F;
        [SerializeField] private float m_RotationSpeed = 3.2F;
        [Space]
        [SerializeField] private AnimationCurve m_ScalingCurve = AnimationCurve.Linear(1F, 1F, 1F, 1.1F);
        [SerializeField] private float m_ScalingSpeed = 3.2F;
        [Space]
        [SerializeField] private float m_MovementSpeed = 1.4F;



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:

        // Local Fields:
        private Vector3 initScale;

        // Dynamic:
        private Vector2 targetPosition;
        private bool isDragging;
        private bool isSelected;
        private float scaleProgress;
        private float rotationProgress;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        protected virtual void Awake()
        {
            m_Visuals.worldCamera = Camera.main;
            initScale = m_Visuals.transform.localScale;
            targetPosition = transform.position;
            RectTransform rectTransform = (RectTransform)m_Visuals.transform;
            if (TryGetComponent(out BoxCollider2D collider))
            {
                collider.size = rectTransform.rect.size;
            }
        }

        private void Update()
        {
            Rotate(Time.deltaTime);
            ScaleUp(Time.deltaTime);
            if (isDragging)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                    return;
                }

                UpdatePosition();
            }

            Vector2 position = transform.position;
            if (position == targetPosition) return;
            Vector2 delta = (targetPosition - position);
            position += (targetPosition - position) * Mathf.Lerp(0F, 1F, Time.deltaTime * m_MovementSpeed * Mathf.Max(1F, delta.magnitude));
            transform.position = position;
            OnMoved(position);
        }

        protected virtual void OnMoved(Vector3 newPosition) { }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void OnMouseEnter() => isSelected = true;
        private void OnMouseExit() => isSelected = false;
        private void OnMouseDrag()
        {
            isDragging = true;
            UpdatePosition();
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ScaleUp(float timeDelta)
        {
            float newScale = isSelected
                ? Mathf.Lerp(m_ScalingCurve.StartTime(), m_ScalingCurve.EndTime(), scaleProgress + timeDelta * m_ScalingSpeed)
                : Mathf.Lerp(m_ScalingCurve.StartTime(), m_ScalingCurve.EndTime(), scaleProgress - timeDelta * m_ScalingSpeed);
            if (scaleProgress != newScale)
            {
                m_Visuals.transform.localScale = initScale * m_ScalingCurve.Evaluate(newScale);
            }

            scaleProgress = newScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Rotate(float timeDelta)
        {
            if (isSelected)
            {
                // Determines on which distance form center rotation is at a peek (and divides '1' by this value).
                rotationProgress = Mathf.Lerp(0F, 1F, rotationProgress + timeDelta * m_RotationSpeed);

                Vector3 localScale = m_Visuals.transform.localScale;
                localScale.x *= m_Visuals.pixelRect.width;
                localScale.y *= m_Visuals.pixelRect.height;

                // Rotating towards pointer.
                Vector3 delta = InputUtilities.WorldMousePosition - m_Visuals.transform.position;
                delta.x = Mathf.Clamp(delta.x / localScale.x, -1F, 1F) * rotationProgress;
                delta.y = Mathf.Clamp(delta.y / localScale.y, -1F, 1F) * rotationProgress;
                m_Visuals.transform.localRotation = Quaternion.Euler(delta.y * m_MaxAngle, -delta.x * m_MaxAngle, 0f);
            }
            else
            {
                float newRotation = Mathf.Lerp(0F, 1F, rotationProgress - timeDelta * m_RotationSpeed);
                if (rotationProgress == newRotation) return;
                Vector3 rotation = m_Visuals.transform.localEulerAngles;
                rotation.x += Mathf.DeltaAngle(rotation.x, 0F) * (1 - newRotation);
                rotation.y += Mathf.DeltaAngle(rotation.y, 0F) * (1 - newRotation);
                rotation.z += Mathf.DeltaAngle(rotation.z, 0F) * (1 - newRotation);
                m_Visuals.transform.localEulerAngles = rotation;
                rotationProgress = newRotation;
            }
        }

        void UpdatePosition()
        {
            // Account for camera movement and zoom.
            targetPosition = TreeLayout.Instance.GetGripPosition(InputUtilities.WorldMousePosition);
        }
    }
}