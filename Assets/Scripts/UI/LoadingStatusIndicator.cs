using OptiLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gen
{
    public sealed class LoadingStatusIndicator : MonoBehaviour, IAnimatable
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
        [SerializeField] private TMP_Text m_StatusText;
        [SerializeField] private LayoutElement m_Element;
        [SerializeField] private AnimationCurve m_RevealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Space]
        [SerializeField] private RectTransform m_RotatorBlock;
        [SerializeField] private RectTransform m_RotatorAnchor;
        [SerializeField] private AnimationCurve m_RotationCurve = AnimationCurve.Linear(0f, 0f, 2f, 360f);


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Loading Icon Animation:
        private float rotationProgress;

        // Height Animation:
        private float resizeProgress;
        private float minHeight;
        private float targetProgress;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            minHeight = m_Element.minHeight;
            m_Element.minHeight = m_RevealCurve.Evaluate(0F);
            HideLoadingAnimation();
            Hide();
        }

        private void OnEnable()
        {
            AnimationManager.Animate(this);
            LoginManager.Instance.OnLoggingIn += Show;
            LoginManager.Instance.OnLoggingIn += ShowLoadingAnimation;
            LoginManager.Instance.OnLoginSucceeded += HideLoadingAnimation;
            LoginManager.Instance.OnLoginFailed += HideLoadingAnimation;
            LoginManager.Instance.OnStatusUpdated += HandleStatusUpdate;
        }

        private void OnDisable()
        {
            AnimationManager.StopAnimating(this);
            LoginManager.Instance.OnLoggingIn -= Show;
            LoginManager.Instance.OnLoggingIn -= ShowLoadingAnimation;
            LoginManager.Instance.OnLoginSucceeded -= HideLoadingAnimation;
            LoginManager.Instance.OnLoginFailed -= HideLoadingAnimation;
            LoginManager.Instance.OnStatusUpdated -= HandleStatusUpdate;
        }

        public void UpdateAnimation(float timeDelta)
        {
            resizeProgress = Mathf.Lerp(resizeProgress, targetProgress, timeDelta);
            m_Element.minHeight = m_RevealCurve.Evaluate(resizeProgress) * minHeight;
            if (m_RotatorBlock.gameObject.activeSelf)
            {
                rotationProgress += timeDelta;
                rotationProgress %= m_RotationCurve.EndTime();

                m_RotatorAnchor.localEulerAngles = new Vector3(0, 0, m_RotationCurve.Evaluate(rotationProgress));
            }
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>



        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Show() => targetProgress = 1f;
        private void Hide() => targetProgress = 0f;
        private void ShowLoadingAnimation() => m_RotatorBlock.gameObject.SetActive(true);
        private void HideLoadingAnimation() => m_RotatorBlock.gameObject.SetActive(false);
        private void HandleStatusUpdate(string message)
        {
            if (message == null)
            {
                Hide();
                return;
            }

            Show();
            m_StatusText.text = message;
        }
    }
}