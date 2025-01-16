using OptiLib;
using UnityEngine;


namespace Gen
{
    public sealed class Connection : MonoBehaviour
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public Transform Source { get; set; }
        public Transform Target { get; set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private LineRenderer m_Renderer;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:
        private static Vector3[] StaticSet;

        // Encapsulated Fields:

        // Local Fields:
        private float lastSourceX, lastSourceY;
        private float lastTargetX, lastTargetY;
        private bool isDirty;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake() => ConnectionAnimator.ConnectionCount++;
        private void OnDestroy() => ConnectionAnimator.ConnectionCount--;
        private void OnEnable() => Rebuild();
        private void FixedUpdate()
        {
            if (isDirty) return;
            if (!Source || !Target) return;

            Vector2 a = Source.position;
            Vector2 b = Target.position;
            if (lastSourceX != a.x || lastSourceY != a.y || lastTargetX != b.x || lastTargetY != b.y)
            {
                Rebuild();
            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void ForceRebuild() => Rebuild_Internal();
        public void Rebuild()
        {
            if (isDirty) return; isDirty = true;
            Lerp.Delay(Rebuild_Internal);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        void Rebuild_Internal()
        {
            // Dirty reset:
            isDirty = false;
            if (!Source || !Target) return;
            const float FixedCurveAltitude = 5;
            StaticSet ??= new Vector3[ConnectionAnimator.MaxDetalization + 2];
            Vector3 source = Source.position.Set_Z(FixedCurveAltitude);
            Vector3 target = Target.position.Set_Z(FixedCurveAltitude);
            lastSourceX = source.x;
            lastSourceY = source.y;
            lastTargetX = target.x;
            lastTargetY = target.y;

            // Rebuilding:
            int detalization = ConnectionAnimator.Instance.Detalization;
            StaticSet[0] = source;
            int i = 1;
            for (int length = ConnectionAnimator.Instance.MinDetalization + 2; i < length; i++)
            {
                float time = (float)i / detalization;
                Vector3 point = Bezier.Get(source, target, time).Set_Z(FixedCurveAltitude);
                StaticSet[i] = point;
            }

            m_Renderer.positionCount = StaticSet.Length;
            m_Renderer.SetPositions(StaticSet);
            for (int limit = detalization + 2; i < limit; i++)
            {
                float time = (float)i / detalization;
                Vector3 point = Bezier.Get(source, target, time).Set_Z(FixedCurveAltitude);
                m_Renderer.SetPosition(i, point);
            }
        }

        /// <summary>
        /// Simplified version of Bezier curve functions for this specific game.
        /// </summary>
        /// <remarks>
        /// Assumes that 'from' point always looks down and 'to' point look up - no exceptions, only visual errors otherwise!
        /// </remarks>
        private static class Bezier
        {
            // Temp: Make curve visually better - smoother in slopes. (introduce 'control' again)
            public static Vector2 Get(Vector2 from, Vector2 to, float time)
            {
                Vector2 center = from + (to - from) * 0.5F;
                time = Mathf.Clamp01(time) * 2F;

                // 3 Points -> 2 points -> 1 point (result):
                if (time < 1F)
                {
                    float aY = Mathf.LerpUnclamped(from.y, center.y, time);
                    float bX = Mathf.LerpUnclamped(from.x, center.x, time);
                    return new(Mathf.LerpUnclamped(from.x, bX, time), Mathf.LerpUnclamped(aY, center.y, time));
                }
                else
                {
                    time -= 1F;
                    float aX = Mathf.LerpUnclamped(center.x, to.x, time);
                    float bY = Mathf.LerpUnclamped(center.y, to.y, time);
                    return new(Mathf.LerpUnclamped(aX, to.x, time), Mathf.LerpUnclamped(center.y, bY, time));
                }
            }
        }
    }
}