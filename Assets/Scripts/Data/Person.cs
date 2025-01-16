using System;
using UnityEngine;

namespace Gen.Data
{
    [Serializable]
    public sealed class Person
    {
        public event Action<string> OnNameChanged;

        public string InternalName { get => m_InternalName; set => m_InternalName = value; }
        public string Name
        {
            get => m_Name;
            set
            {
                if (m_Name == value) return;
                m_Name = value;
                OnNameChanged?.Invoke(value);
            }
        }
        public string Description { get => m_Description; set => m_Description = value; }
        public string[] Childs { get => m_Childs; set => m_Childs = value; }
        public Vector2 UIPosition { get => m_UIPosition; set => m_UIPosition = value; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private string m_InternalName;
        [SerializeField] private string m_Name;
        [SerializeField] private string m_Description;
        [SerializeField] private string[] m_Childs = Array.Empty<string>();
        [SerializeField] private Vector2 m_UIPosition;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Constructors
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public Person(string internalName)
        {
            InternalName = internalName;
        }
    }
}
