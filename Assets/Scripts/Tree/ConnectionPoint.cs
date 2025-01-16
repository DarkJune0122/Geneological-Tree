using Gen.Data;
using Gen.Tree;
using Gen.UI;
using OptiLib;
using OptiLib.Animation;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gen.Assets.Scripts.Tree
{
    public sealed class ConnectionPoint : MonoBehaviour, IEditableNode
    {

        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public Person Person
        {
            get => new(null); set
            {
                const float Offset = 5F;

                // Creates new connection between records.
                var node = Instantiate(TreeManager.Instance.RenderNodePrefab, transform.position + Vector3.zero.Set_Y(m_IsFrontalConnection ? -Offset : Offset), transform.rotation, TreeLayout.Instance.Anchor);
                if (m_IsFrontalConnection)
                {
                    string[] array = m_Host.Person.Childs;
                    Extentions.Add(ref array, value.InternalName);
                    m_Host.Person.Childs = array;
                    m_Host.Rebuild();
                }
                else
                {
                    string[] array = value.Childs;
                    Extentions.Add(ref array, m_Host.Person.InternalName);
                    value.Childs = array;
                }

                node.Init(value);
            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [Header("Connection Management")]
        [SerializeField] private Graphic[] m_Base;
        [SerializeField] private RenderNode m_Host;
        [SerializeField] private bool m_IsFrontalConnection;
        [SerializeField] private PointCurve m_ColorCurve;
        [SerializeField] private Gradient m_SelectedGradient;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:
        private static event Action<ConnectionPoint> OnPinChanged;
        private static ConnectionPoint LastPin
        {
            get => lastPin; set
            {
                if (lastPin == value) return;
                lastPin = value;

                OnPinChanged?.Invoke(value);
            }
        }

        // Encapsulated Fields:

        // Local Fields:
        private static ConnectionPoint lastPin;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            m_ColorCurve.action = (time) => Array.ForEach(m_Base, (item) => item.color = m_SelectedGradient.Evaluate(time));
            OnPinChanged += HighlightIfSelected;
        }

        private void OnDestroy() => OnPinChanged -= HighlightIfSelected;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void CreateConnection()
        {
            if (LastPin != null || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                Link();
                return;
            }

            LastPin = null;
            UIPersonEditor.Instance.Caller = this;
        }

        public void OnPersonDeletion()
        {
            // Ignored.
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
        void HighlightIfSelected(ConnectionPoint link)
        {
            if (link != null) m_ColorCurve.Start();
            else if (!Mathf.Approximately(m_ColorCurve.time, 0f))
            {
                m_ColorCurve.StartReverse();
            }
        }

        void Link()
        {
            if (LastPin == null)
            {
                LastPin = this;
                return;
            }

            if (LastPin.m_IsFrontalConnection == m_IsFrontalConnection)
            {
                if (MessagePopup.Instance) MessagePopup.Instance.Show("Cannot connect links on the same side!");
                LastPin = null;
                return;
            }

            // Adds child node as a child on a data side.
            ConnectionPoint frontal = m_IsFrontalConnection ? this : LastPin;
            ConnectionPoint child = m_IsFrontalConnection ? LastPin : this;
            string[] array = frontal.m_Host.Person.Childs;
            LastPin = null;

            try
            {
                // Also, removes connection if one already exist:
                string childName = child.m_Host.Person.InternalName;
                int index = Extentions.IndexOfEquatible(array, childName);
                if (index != -1)
                {
                    Extentions.RemoveAt(ref array, index);
                }
                else
                {
                    Extentions.Add(ref array, childName);
                }
            }
            catch (Exception ex)
            {
                // Some of my methods can fail, as there was too little testing fur such library.
                Debug.LogException(ex);
            }

            frontal.m_Host.Person.Childs = array;
            frontal.m_Host.Rebuild();
        }
    }
}
