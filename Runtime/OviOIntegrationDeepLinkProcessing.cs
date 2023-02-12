using System;
using UnityEngine;

namespace OviO.Integration
{
    public class OviOIntegrationDeepLinkProcessing : ScriptableObject
    {
        public GameObject OviOIntegrationGameObject = new GameObject();
        private OviOIntegration _ovioIntegration;

        public OviOIntegrationDeepLinkProcessing(string devId)
        {
            if (devId == null || devId == string.Empty)
            {
                throw new ArgumentNullException(nameof(devId), "devId cannot be null or empty");
            }

            _ovioIntegration = new OviOIntegration(devId);
            OviOIntegrationGameObject.AddComponent<ProcessDeepLink>();
        }

        public void RegiserCallback(Action<TransactionData> callback)
        {
            OviOIntegrationGameObject.GetComponent<ProcessDeepLink>().Initialize(callback, _ovioIntegration);
        }
    }
}
