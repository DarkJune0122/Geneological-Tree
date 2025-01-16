using Gen;
using OptiLib;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ConnectionAnimator : MonoBehaviour
{
    public const int MaxDetalization = 80;

    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Singleton Property
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    public static ConnectionAnimator Instance { get; private set; }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Public Properties
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    // Events:

    // Properties:
    public static int ConnectionCount { get; set; }
    public int Detalization => Mathf.CeilToInt(m_DetalizationCurve.Evaluate(ConnectionCount) * MaxDetalization);
    public int MinDetalization => Mathf.CeilToInt(m_DetalizationCurve.Evaluate(m_DetalizationCurve.EndTime()) * MaxDetalization);


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Serialized Fields
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    [SerializeField] private AnimationCurve m_DetalizationCurve = AnimationCurve.Constant(0F, 10F, 1.0F);
    [SerializeField] private float m_MaxUpdatesPerSecond = 60F;


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Private Fields
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    // Static Fields:

    // Encapsulated Fields:

    // Local Fields:
    private readonly Dictionary<Connection, int> Requests = new();
    private float lastTime;


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Unity Callbacks
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    private void Awake() => Instance = this;
    [Obsolete("Connections are now able to render themselves when needed, separately from this system. Just ue them directly.")]
    private void Update()
    {
        float targetDelay = 1f / m_MaxUpdatesPerSecond;
        float delta = Time.realtimeSinceStartup - lastTime;
        if (delta >= targetDelay)
        {
            /*foreach (Connection connection in Requests.Keys)
            {
                connection.UpdateConnection();
            }*/

            lastTime = Time.realtimeSinceStartup;
        }
    }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Public Methods
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    [Obsolete("Connections are now able to render themselves when needed, separately from this system. Just ue them directly.")]
    public void Animate(Connection connection)
    {
        if (Requests.TryGetValue(connection, out int totalRequests))
        {
            Requests[connection] = totalRequests + 1;
            return;
        }

        Requests[connection] = 1;
    }

    [Obsolete("Connections are now able to render themselves when needed, separately from this system. Just ue them directly.")]
    public void StopAnimating(Connection connection)
    {
        if (!Requests.TryGetValue(connection, out int totalRequests))
            return;

        totalRequests--;
        if (totalRequests <= 0)
        {
            Requests.Remove(connection);
        }
    }


    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
    /// 
    ///                                     Miscellaneous
    /// 
    /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    private struct AnimateRequest
    {
        public Connection connection;
        public int totalRequests;
    }
}