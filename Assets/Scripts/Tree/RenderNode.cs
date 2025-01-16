using Gen.Data;
using Gen.UI;
using OptiLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gen.Tree
{
    public sealed class RenderNode : Node, IEditableNode
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public List<Connection> Connections => m_Connections;
        public Person Person
        {
            get => m_Person; set
            {
                if (m_Person == value) return;
                if (m_Person != null) m_Person.OnNameChanged -= HandleNameChange;
                m_Person = value;
                value.OnNameChanged += HandleNameChange;
                HandleNameChange(value.Name);
            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [Header("Tree nodes")]
        [SerializeField] private Transform m_InConnection;
        [SerializeField] private Transform m_OutConnection;
        [SerializeField] private TMP_Text m_PersonName;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:
        private readonly List<Connection> m_Connections = new();
        private Person m_Person;

        // Local Fields:
        private bool isDirty;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        protected override void Awake()
        {
            base.Awake();
            if (TryGetComponent(out BoxCollider2D collider))
            {
                m_InConnection.transform.localPosition = new Vector3(0, collider.size.y * 0.5F);
                m_OutConnection.transform.localPosition = new Vector3(0, collider.size.y * -0.5F);
            }
        }
        private void OnMouseUpAsButton()
        {
            if (!InputUtilities.IsMoved)
                UIPersonEditor.Instance.Caller = this;
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void Init(Person person)
        {
            if (person == null)
            {
                Destroy(gameObject);
                return;
            }

            Person = person;
            person.UIPosition = transform.position;
            TreeManager.Instance.AddNode(this);
            Rebuild();
        }
        public void OnPersonDeletion()
        {
            TreeManager.Instance.RemoveNode(this);
            m_Connections.ForEach((c) => Destroy(c.gameObject));
            Destroy(gameObject);
        }

        public void ForceRebuild() => Rebuild_Internal();
        public void Rebuild()
        {
            if (isDirty) return; isDirty = true;
            Lerp.Delay(Rebuild_Internal);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Implementations
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        protected override void OnMoved(Vector3 newPosition)
        {
            if (Person != null) Person.UIPosition = newPosition;
            TreeManager.Instance.SetDirty();
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        void HandleNameChange(string name) => m_PersonName.text = name;
        void Rebuild_Internal()
        {
            isDirty = false;
            int requirement = Person.Childs.Length;
            int limit = m_Connections.Count;
            if (requirement > limit)
            {
                Connection cp = TreeManager.Instance.ConnectionPrefab;
                while (requirement > limit)
                {
                    Connection instance = Instantiate(cp);
                    instance.Source = m_OutConnection;
                    m_Connections.Add(instance);
                    limit++;
                }
            }

            int conIndex = 0;
            var nodes = TreeManager.Instance.Nodes;
            foreach (var child in Person.Childs)
            {
                if (!nodes.TryGetValue(child, out RenderNode value))
                    continue;

                Connection con = m_Connections[conIndex];
                con.Target = value.m_InConnection;
                if (con.gameObject.activeSelf) con.ForceRebuild();
                else con.gameObject.SetActive(true);
                conIndex++;
            }

            while (conIndex < limit)
            {
                m_Connections[conIndex].gameObject.SetActive(false);
                conIndex++;
            }
        }
    }
}
