using CRUD;
using Gen.Networking;
using OptiLib;
using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gen
{
    public sealed class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }
        private const string LastLoginKey = "lastUsername";
        private const string LastPasswordKey = "lastPassword";


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Properties
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Events:
        public event Action<string> OnStatusUpdated;
        public event Action OnLoggingIn;
        public event Action OnLoginSucceeded;
        public event Action OnLoginFailed;

        // Properties:
        public string LastMessage { get; private set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Serialized Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        [SerializeField] private GraphicRaycaster m_Raycaster;
        [SerializeField] private TMP_InputField m_Login;
        [SerializeField] private TMP_InputField m_Password;
        [SerializeField] private Toggle m_RememberMe;
        [SerializeField] private SceneField m_MainScene;
        [SerializeField] private float m_TransitionDelay = 1.2F;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Fields
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        // Static Fields:

        // Encapsulated Fields:

        // Local Fields:
        private bool isLoggingIn;


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Unity Callbacks
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void Awake()
        {
            Instance = this;
            m_Login.SetTextWithoutNotify(PlayerPrefs.GetString(LastLoginKey));
            m_Password.SetTextWithoutNotify(PlayerPrefs.GetString(LastPasswordKey));
            m_RememberMe.SetIsOnWithoutNotify(PlayerPrefs.HasKey(LastLoginKey) && PlayerPrefs.HasKey(LastPasswordKey));

            m_Login.onValueChanged.AddListener(ResetOnEdit);
            m_Password.onValueChanged.AddListener(ResetOnEdit);
            m_RememberMe.onValueChanged.AddListener(ResetOnEdit);
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Public Methods
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void ResetMessage() => SetStatus(null);
        public void TrySignUp()
        {
            if (isLoggingIn) return;
            isLoggingIn = true;
            if (!RetrieveCredentials(out LoginInfo info))
            {
                isLoggingIn = false;
                return;
            }

            Debug.Log("Username: " + info.Username);
            Debug.Log("Login: " + info.Password);
            Debug.Log("User ID: " + info.UserID);

            OnLoggingIn?.Invoke();
            SetStatus("Signing up...");
            DB.Register(info).WhenCompleted(result =>
            {
                Manager.LastCommunicationResult = new(result.succeeded, result.message);
                isLoggingIn = false;
                if (!result.succeeded)
                {
                    OnSigningUpFailed();
                }

                if (result.result)
                {
                    Debug.Log("Username: " + info.Username);
                    Debug.Log("Login: " + info.Password);
                    Debug.Log("User ID: " + info.UserID);
                    OnSigningUpSucceeded();
                }
                else if (result.status == 400)
                {
                    OnSigningUpFailed();
                }
                else OnRequestFailure();
            });
        }

        public void TrySignIn()
        {
            if (isLoggingIn) return;
            isLoggingIn = true;
            if (!RetrieveCredentials(out LoginInfo info))
            {
                isLoggingIn = false;
                return;
            }

            OnLoggingIn?.Invoke();
            SetStatus("Signing in...");
            DB.Login(info).WhenCompleted(result =>
            {
                Manager.LastCommunicationResult = new(result.succeeded, result.message);
                isLoggingIn = false;
                if (!result.succeeded)
                {
                    OnRequestFailure();
                    return;
                }

                if (result.result.Item1)
                {
                    LoginInfo.Active = info;
                    info.UserID = result.result.Item2;
                    OnSigningInSucceeded();
                }
                else OnSigningInFailed();
            });
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Miscellaneous
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private void SetStatus(string status)
        {
            Debug.Log(status);
            OnStatusUpdated?.Invoke(LastMessage = status);
        }

        private void ResetOnEdit(string text)
        {
            if (isLoggingIn) return;
            ResetMessage();
        }
        private void ResetOnEdit(bool flag)
        {
            if (isLoggingIn) return;
            ResetMessage();
        }


        private void OnRequestFailure()
        {
            OnLoginFailed?.Invoke();
            SetStatus("Request failed. Please, try again." + (Manager.LastCommunicationResult.succeeded ? string.Empty : "\n" + Manager.LastCommunicationResult.message));
        }

        private void OnSigningUpFailed()
        {
            OnLoginFailed?.Invoke();
            SetStatus("Username is already taken!");
        }
        private void OnSigningUpSucceeded()
        {
            OnLoginSucceeded?.Invoke();
            SetStatus("Singed up successfully! Enter the system by signing in again.");
        }

        private void OnSigningInFailed()
        {
            OnLoginFailed?.Invoke();
            SetStatus("Cannot sign in! Verify you login and password.");
        }

        private void OnSigningInSucceeded()
        {
            m_Raycaster.enabled = false;
            OnLoginSucceeded?.Invoke();
            SetStatus("Singed in successfully!");
            Lerp.Delay(() => Manager.Instance.Transition(() => SceneManager.LoadScene(m_MainScene)));
        }

        private bool RetrieveCredentials([NotNullWhen(true)] out LoginInfo info)
        {
            info = null;
            if (string.IsNullOrWhiteSpace(m_Login.text))
            {
                SetStatus("Login field cannot be empty!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(m_Password.text))
            {
                SetStatus("Password field cannot be empty!");
                return false;
            }

            info = new LoginInfo()
            {
                Username = m_Login.text,
                Password = m_Password.text
            };

            if (m_RememberMe.isOn)
            {
                PlayerPrefs.SetString(LastLoginKey, info.Username);
                PlayerPrefs.SetString(LastPasswordKey, info.Password);
            }
            else
            {
                PlayerPrefs.DeleteKey(LastLoginKey);
                PlayerPrefs.DeleteKey(LastPasswordKey);
            }

            return true;
        }

        [Serializable]
        private struct LoginData
        {
            public string login;
            public string password;

            public LoginData(string login, string password)
            {
                this.login = login;
                this.password = password;
            }
        }
    }
}