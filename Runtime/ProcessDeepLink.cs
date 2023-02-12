using System;
using UnityEngine;

namespace OviO.Integration
{
    public class ProcessDeepLink : MonoBehaviour
    {
        public static ProcessDeepLink Instance { get; private set; }
        public string deeplinkURL;

        private Action<TransactionData> _callback;
        private OviOIntegration _ovioIntegration;

        public void Initialize(Action<TransactionData> callback, OviOIntegration ovioIntegration)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback), "callback cannot be null");
            }

            _callback = callback;
            _ovioIntegration = ovioIntegration;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Application.deepLinkActivated += OnDeepLinkActivated;
                if (!String.IsNullOrEmpty(Application.absoluteURL))
                {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    OnDeepLinkActivated(Application.absoluteURL);
                }
                // Initialize DeepLink Manager global variable.
                else deeplinkURL = "[none]";
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDeepLinkActivated(string link)
        {
            deeplinkURL = link;

            if (link.ToLower().Contains("ovio"))
            {
                HandleLaunchURL(link.ToLower());
            }
        }

        void HandleLaunchURL(string link)
        {
            _ovioIntegration.GetAmount(link, _callback);
        }
    }
}