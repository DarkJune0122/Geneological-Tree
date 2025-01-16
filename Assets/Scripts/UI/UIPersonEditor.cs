using Gen.Data;
using Gen.Tree;
using OptiLib.Animation;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Gen.UI
{
    public sealed class UIPersonEditor : MonoBehaviour
    {
        public static UIPersonEditor Instance { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public IEditableNode Caller
        {
            get => caller; set
            {
                if (caller == value) return;
                if (value == null) return;
                if (value.Person == null) return;
                caller = value;
                Person = value.Person;
            }
        }

        public Person Person
        {
            get => person; set
            {
                if (person == value) return;
                person = value;
                if (value == null)
                {
                    Hide();
                }
                else
                {
                    Show();
                    m_PersonName.text = person.Name;
                    m_Description.text = person.Description;
                    m_OnDeleteAccessible.Invoke(person.InternalName != null);
                }

            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private PointCurve m_AppearanceAlpha = new() { curve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1f) };
        [Space]
        [SerializeField] private RectTransform m_PopupAnchor;
        [SerializeField] private CanvasGroup m_PopupGroup;
        [SerializeField] private PointCurve m_PopupAlpha = new() { curve = AnimationCurve.Constant(0f, 1f, 1f) };
        [SerializeField] private AnimationCurve m_PopupPosition = AnimationCurve.Constant(0f, 1f, 0f);
        [SerializeField] private Vector2 m_PopupOffset;
        [Header("Person Data modification")]
        [SerializeField] private TMP_InputField m_PersonName;
        [SerializeField] private TMP_InputField m_Description;
        [SerializeField] private UnityEvent<bool> m_OnDeleteAccessible;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:
        private IEditableNode caller;
        private Person person;

        // Local Fields:
        private Vector2 initPopupPosition;
        private string editName;
        private string editDescription;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            Instance = this;
            m_AppearanceAlpha.action = (evaluate) => m_Group.alpha = evaluate;
            m_PopupAlpha.action = (alpha) =>
            {
                m_PopupGroup.alpha = alpha;
                m_PopupAnchor.anchoredPosition = initPopupPosition + m_PopupOffset
                * m_PopupPosition.Evaluate(m_PopupAlpha.time / m_PopupAlpha.Duration);
            };
            m_PersonName.onValueChanged.AddListener(HandleNameChange);
            m_Description.onValueChanged.AddListener(HandleDescriptionChange);
            initPopupPosition = m_PopupAnchor.anchoredPosition;
            m_Group.alpha = 0F;
            m_Group.blocksRaycasts = false;
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void Show()
        {
            if (m_Group.blocksRaycasts) return;
            m_Group.blocksRaycasts = true;
            m_AppearanceAlpha.Start();
        }

        public void Hide()
        {
            if (!m_Group.blocksRaycasts) return;
            m_Group.blocksRaycasts = false;
            caller = null;
            person = null;
            m_AppearanceAlpha.StartReverse();
        }

        public void Submit()
        {
            if (caller != null)
            {
                TreeManager.Instance.SetDirty();
                Person.InternalName = Guid.NewGuid().ToString();
                Person.Name = editName;
                Person.Description = editDescription;
                Data.Tree.Current.Nodes.Add(Person);
                caller.Person = Person;
            }

            Hide();
        }

        public void Discard() => Hide();
        public void ShowPopup()
        {
            if (m_PopupAlpha.IsIdle)
                m_PopupAlpha.Start();
        }

        public void Delete()
        {
            if (Person != null && Person.InternalName != null)
            {
                string[] array;
                string ownName = Person.InternalName;
                foreach (var node in Data.Tree.Current.Nodes)
                {
                    int index = System.Array.IndexOf(node.Childs, ownName);
                    if (index == -1) continue;

                    array = node.Childs;
                    OptiLib.Extentions.RemoveAt(ref array, index);
                    node.Childs = array;
                }

                Data.Tree.Current.Nodes.Remove(Person);
                caller.OnPersonDeletion();

                TreeManager.Instance.RebuildAll();
            }

            Hide();
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
        void HandleNameChange(string name) => editName = name;
        void HandleDescriptionChange(string description) => editDescription = description;


#if UNITY_EDITOR
        private void Reset()
        {
            TryGetComponent(out m_Group);
            if (m_AppearanceAlpha != null)
                m_AppearanceAlpha.CustomExecutor = this;
            if (m_PopupAlpha != null)
                m_PopupAlpha.CustomExecutor = this;
        }
#endif
    }
}
