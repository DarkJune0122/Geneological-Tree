using UnityEngine;

namespace Gen
{
    public sealed class TreeLayout : MonoBehaviour
    {
        public static TreeLayout Instance { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public Transform Anchor => m_Anchor;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private Transform m_Anchor;
        [SerializeField] private Vector2 m_GridScale = Vector2.one;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:

        // Local Fields:


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake() => Instance = this;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public Vector3 GetGripPosition(Vector3 mousePos)
        {
            return new Vector3(
                x: Mathf.Round(mousePos.x / m_GridScale.x) * m_GridScale.x,
                y: Mathf.Round(mousePos.y / m_GridScale.y) * m_GridScale.x);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>



#if UNITY_EDITOR
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Editor Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Reset() => m_Anchor = transform;
#endif
    }
}