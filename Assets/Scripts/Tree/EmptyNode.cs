using Gen.Data;
using Gen.UI;
using UnityEngine;

namespace Gen.Tree
{
    public sealed class EmptyNode : MonoBehaviour, IEditableNode
    {
        public Person Person
        {
            get => new(null); set
            {
                Instantiate(TreeManager.Instance.RenderNodePrefab, transform.position, transform.rotation, TreeLayout.Instance.Anchor)
                    .Init(value);
            }
        }



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private Canvas m_Canvas;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            if (m_Canvas)
                m_Canvas.worldCamera = Camera.main;
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void OpenEditorUI() => UIPersonEditor.Instance.Caller = this;
        public void OnPersonDeletion()
        {
            // Ignored.
        }
    }
}
