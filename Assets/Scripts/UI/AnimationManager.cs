using Gen;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AnimationManager : MonoBehaviour
{
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Public Properties
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    // Events:

    // Properties:
    public static AnimationManager Instance { get; private set; }
    public static float MaxUpdatesPerSecond
    {
        get => Instance.m_MaxUpdatesPerSecond;
        set => Instance.m_MaxUpdatesPerSecond = value;
    }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Serialized Fields
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    [SerializeField] private float m_MaxUpdatesPerSecond = 90f;


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Private Fields
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    // Static Fields:
    private static readonly List<IAnimatable> animatables = new(128);

    // Encapsulated Fields:

    // Local Fields:
    private float lastTime;
    private float screenRefreshRate = 60;


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Unity Callbacks
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    private void Awake()
    {
        Instance = this;
        screenRefreshRate = (float)Math.Round(Screen.currentResolution.refreshRateRatio.value);
    }

    private void OnEnable() => lastTime = Time.realtimeSinceStartup;
    private void Update()
    {
        float targetDelay = 1f / Mathf.Min(screenRefreshRate, m_MaxUpdatesPerSecond);
        float delta = Time.realtimeSinceStartup - lastTime;
        if (delta >= targetDelay)
        {
            for (int i = animatables.Count - 1; i >= 0; i--)
            {
                animatables[i].UpdateAnimation(delta);
            }

            lastTime = Time.realtimeSinceStartup;
        }
    }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Public Methods
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    public static void Animate(IAnimatable animatable) => animatables.Add(animatable);
    public static void StopAnimating(IAnimatable animatable) => animatables.Remove(animatable);


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Miscellaneous
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
}