using Gen.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace Gen
{
    public sealed class AppTerminal : MonoBehaviour
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
        [SerializeField] private UnityEvent<bool> m_UseLocalServerCallback;
        [SerializeField] private UnityEvent<bool> m_UseDebugServerCallBack;
        [SerializeField] private UnityEvent<bool> m_UseDebugServerChanged;


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
        private void Awake()
        {
            m_UseLocalServerCallback.Invoke(IDatabaseProcessor.UseLocalServer);
            m_UseDebugServerCallBack.Invoke(IDatabaseProcessor.UseDebugServer);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void Quit() => Application.Quit();
        public void SetDestinationServer(bool choice) => IDatabaseProcessor.UseLocalServer = choice;
        public void ToggleDestinationServer() => SetDestinationServer(!IDatabaseProcessor.UseLocalServer);
        public void SetUseDebugServer(bool choice)
        {
            if (IDatabaseProcessor.UseDebugServer == choice) return;
            IDatabaseProcessor.UseDebugServer = choice;
            m_UseDebugServerChanged.Invoke(choice);
        }

        public void ToggleUseDebugServer() => SetDestinationServer(!IDatabaseProcessor.UseDebugServer);


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
    }
}