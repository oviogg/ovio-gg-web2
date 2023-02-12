using System;
using UnityEngine;

namespace OviO.Integration
{
    public class OviOIntegration : ScriptableObject
    {
        public GameObject OviOIntegrationGameObject = new GameObject();

        public OviOIntegration(string devId)
        {
            if (devId == null || devId == string.Empty)
            {
                throw new ArgumentNullException(nameof(devId), "devId cannot be null or empty");
            }

            OviOIntegrationGameObject.AddComponent<OviOIntegrationScripts>();
            OviOIntegrationGameObject.GetComponent<OviOIntegrationScripts>().Initialize(devId);
        }

        public void GetAmount(string url, Action<TransactionData> callback)
        {
            OviOIntegrationGameObject.GetComponent<OviOIntegrationScripts>().GetAmount(url, callback);
        }
    }

    public class TransactionData
    {
        public int Amount { get; set; }
        public string CoinName { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}