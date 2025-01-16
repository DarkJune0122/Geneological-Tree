using CRUD;
using Gen.Networking;
using OptiLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gen.Tree
{
    public sealed class TreeManager : MonoBehaviour
    {
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Singleton Instance
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public static TreeManager Instance { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:

        // Properties:
        public Dictionary<string, RenderNode> Nodes => nodes;
        public RenderNode RenderNodePrefab => m_RenderNodePrefab;
        public Connection ConnectionPrefab => m_ConnectionPrefab;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private SceneField m_MenuScene;
        [SerializeField] private CanvasGroup m_RaycastBlocker;
        [Tooltip("Delay in seconds.")]
        [SerializeField] private float m_AutoSaveDelay = 30F;
        [Space]
        [SerializeField] private Connection m_ConnectionPrefab;
        [SerializeField] private RenderNode m_RenderNodePrefab;
        [SerializeField] private EmptyNode m_DefaultNode;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:

        // Local Fields:
        private readonly Dictionary<string, RenderNode> nodes = new();
        private bool isLoading = false;
        private bool isSaving = false;
        private bool isDirty = false;
        private bool isTransitioning = false;
        private float lastSaveTime;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            Instance = this;
            lastSaveTime = Time.realtimeSinceStartup;
            LoadTree();
        }

        private void Update()
        {
            if (!isDirty) return;
            float delta = Time.realtimeSinceStartup - lastSaveTime;
            if (delta < m_AutoSaveDelay) return;
            lastSaveTime = Time.realtimeSinceStartup; // Repeating, just as a safety measure.
            SaveTree();
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public void ReturnToMenu()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            Manager.Instance.Transition(() =>
            {
                isTransitioning = false;
                SceneManager.LoadScene(m_MenuScene);
                SaveTree();
            });
        }

        private void LoadTree()
        {
            if (isLoading) return;
            isLoading = true;
            m_RaycastBlocker.gameObject.SetActive(true);

            DB.Get(LoginInfo.Active.UserID).WhenCompleted((result) =>
            {
                Manager.LastCommunicationResult = new(result.succeeded, result.message);
                m_RaycastBlocker.gameObject.SetActive(false);

                if (result.succeeded && result.result != null)
                {
                    // Attempts to sync background and UI threads.
                    Lerp.Delay(() => InstantiateTree(result.result));
                }
            });
        }

        private void InstantiateTree(Data.Tree tree)
        {
            Debug.Log("Tree loading result: " + IDatabaseProcessor.DebugMessage);
            Data.Tree.Current = tree;
            foreach (Data.Person person in tree.Nodes)
            {
                var node = Instantiate(m_RenderNodePrefab, person.UIPosition, Quaternion.identity, TreeLayout.Instance.Anchor);
                node.Init(person);
            }
        }

        public void SetDirty() => isDirty = true;
        public void SaveTree()
        {
            if (isSaving) return;
            lastSaveTime = Time.realtimeSinceStartup;
            Data.Tree tree = Data.Tree.Current;
            if (tree == null) return;

            isSaving = true;
            DB.Put(LoginInfo.Active.UserID, tree).WhenCompleted((result) =>
            {
                Debug.Log("Saving result: " + result);
                if (this != null)
                {
                    lastSaveTime = Time.realtimeSinceStartup;
                    isSaving = false;
                }
            });
        }

        public void AddNode(RenderNode node)
        {
            if (!node || node.Person == null) return;
            nodes.Add(node.Person.InternalName, node);
            if (nodes.Count >= 1) m_DefaultNode.gameObject.SetActive(false);
        }

        public void RemoveNode(RenderNode node)
        {
            if (!node || node.Person == null) return;
            nodes.Remove(node.Person.InternalName);
            if (nodes.Count <= 0)
            {
                try
                {
                    m_DefaultNode.gameObject.SetActive(true);
                }
                catch { } // Ignored - triggered when closing game: "GameObjects can not be made active when they are being destroyed."
            }
        }

        public void RebuildAll()
        {
            foreach (var node in nodes.Values)
            {
                node.Rebuild();
            }
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

    }
}
