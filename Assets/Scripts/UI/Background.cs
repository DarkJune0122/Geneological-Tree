using Gen;
using System.Collections.Generic;
using UnityEngine;

public sealed class Background : MonoBehaviour, IAnimatable
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
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Transform m_PropAnchor;
    [SerializeField] private Transform m_Prop;
    [Header("Stylization")]
    [SerializeField] private Vector2 m_GridScale = Vector2.one;
    [SerializeField] private float m_PropScapeProximity = 2.4f;
    [SerializeField] private float m_PropScalingDeadZone = 0.6f;
    [SerializeField] private Vector2 m_MinScale = new(0.2f, 0.2f);
    [SerializeField] private Vector2 m_MaxScale = new(0.25f, 0.25f);


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Private Fields
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    // Static Fields:
    private const float PropSize = 1.0F;

    // Encapsulated Fields:

    // Local Fields:
    private readonly List<Transform> props = new(256);
    private Vector3 lastPosition;


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Unity Callbacks
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    private void OnEnable() => AnimationManager.Animate(this);
    private void OnDisable() => AnimationManager.StopAnimating(this);


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Public Methods
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    public void UpdateAnimation(float timeDelta)
    {
        RecalculatePositions();
        RecalculateScale();
    }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Miscellaneous
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    private void RecalculatePositions()
    {
        // TODO: Optimize A LOT.
        // Note: Can be optimizing by updating positions only when rebuilding is actually needed.
        Vector3 cameraPosition = m_Camera.transform.position;
        if (lastPosition == cameraPosition) return;

        float height = 2f * m_Camera.orthographicSize;
        float width = height * m_Camera.aspect;
        Vector2 min = new(cameraPosition.x - width * 0.5F - PropSize, cameraPosition.y - height * 0.5F - PropSize);
        Vector2 max = new(cameraPosition.x + width * 0.5F + PropSize, cameraPosition.y + height * 0.5F + PropSize);
        min.x = Mathf.Ceil(min.x / m_GridScale.x) * m_GridScale.x;
        min.y = Mathf.Ceil(min.y / m_GridScale.y) * m_GridScale.y;
        max.x = Mathf.Ceil(max.x / m_GridScale.x) * m_GridScale.x;
        max.y = Mathf.Ceil(max.y / m_GridScale.y) * m_GridScale.y;

        int totalX = (int)(Mathf.Abs(max.x - min.x) / m_GridScale.x);
        int totalY = (int)(Mathf.Abs(max.y - min.y) / m_GridScale.y);
        int propCount = totalX * totalY;
        if (propCount > props.Count)
        {
            for (int i = 0; i < propCount; i++)
            {
                Transform prop = Instantiate(m_Prop, m_PropAnchor);
                props.Add(prop);
            }
        }

        // Layout:
        for (int i = 0; i < propCount; i++)
        {
            Transform prop = props[i];
            prop.gameObject.SetActive(true);
            int x = i % totalX;
            int y = i / totalX;
            prop.position = new Vector3(min.x + x * m_GridScale.x, min.y + y * m_GridScale.y, m_PropAnchor.position.z);
        }

        for (int i = propCount, limit = props.Count; i < limit; i++)
        {
            props[i].gameObject.SetActive(false);
        }
    }

    private void RecalculateScale()
    {
        Vector3 source = InputUtilities.WorldMousePosition;
        foreach (var prop in props)
        {
            float distance = Vector2.Distance(source, prop.position);
            if (distance > m_PropScapeProximity)
            {
                prop.localScale = m_MinScale;
                continue;
            }

            prop.localScale = Vector3.Lerp(m_MinScale, m_MaxScale, 1 - (distance - m_PropScalingDeadZone) / (m_PropScapeProximity - m_PropScalingDeadZone));
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
        TryGetComponent(out m_PropAnchor);
        m_Camera = Camera.main;
    }
#endif
}