using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace OviO.Integration
{
    public class OviOIntegrationScripts : MonoBehaviour
    {
        private string _devId;

        public void Initialize(string devId)
        {
            _devId = devId;
        }
        private static string _baseUrl = $"https://prod.ovio.gg/api/gameIntegration";

        public void GetAmount(string url, Action<TransactionData> callback)
        {
            var transactionId = url.ToLower().Split('/').Last();

            StartCoroutine(GetPublicKeyRoutine(_devId, transactionId, callback, StartGetSignedTransactionDataResponseRoutine));
        }

        private IEnumerator GetPublicKeyRoutine(
            string devId,
            string transactionId,
            Action<TransactionData> callback,
            Action<string, string, Action<TransactionData>> getSignedTransactionDataResponseRoutine)
        {
            using (var webRequest = UnityWebRequest.Get($"{_baseUrl}/{devId}/publicKey"))
            {
                yield return webRequest.SendWebRequest();
                getSignedTransactionDataResponseRoutine.Invoke(webRequest.downloadHandler.text, transactionId, callback);
            }
        }

        private IEnumerator GetSignedTransactionDataResponseRoutine(
            string publicKey,
            string transactionId,
            Action<TransactionData> callbackFunction,
            Action<string, string, RSA, RSAPKCS1SignatureDeformatter, Action<TransactionData>> verifySignatureAndClaimTransactionFunction)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm(nameof(SHA256));
            var transactionIdEncryptedString = GetEncryptedTransactionIdString(transactionId, rsa);
            var request = new Request
            {
                TransactionId = transactionIdEncryptedString
            };

            var signTransactionBodyData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            using (var webRequest = UnityWebRequest.Put($"{_baseUrl}/{_devId}/transactionData", ""))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(signTransactionBodyData);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                yield return webRequest.SendWebRequest();

                verifySignatureAndClaimTransactionFunction.Invoke(
                    transactionId,
                    webRequest.downloadHandler.text,
                    rsa,
                    rsaDeformatter,
                    callbackFunction);
            }
        }

        private IEnumerator VerifySignatureAndClaimTransactionRoutine(
            string transactionId,
            string signedResponseString,
            RSA rsa,
            RSAPKCS1SignatureDeformatter rsaDeformatter,
            Action<TransactionData> callbackFunction)
        {
            var signedTransactionDataResponse = JsonConvert.DeserializeObject<SignedTransactionDataResponse>(signedResponseString);
            if (!signedTransactionDataResponse.Success)
            {
                callbackFunction(new TransactionData()
                {
                    IsSuccess = false,
                    Message = "An error occurred"
                });
            }

            var hashedTransactionDataResponse = Convert.FromBase64String(signedTransactionDataResponse.HashBase64String);
            var signature = Convert.FromBase64String(signedTransactionDataResponse.SignatureBase64String);

            if (rsaDeformatter.VerifySignature(hashedTransactionDataResponse, signature))
            {
                var transactionIdEncryptedString = GetEncryptedTransactionIdString(transactionId, rsa);
                var request = new Request
                {
                    TransactionId = transactionIdEncryptedString
                };

                var signTransactionBodyData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

                using (var webRequest = UnityWebRequest.Put($"{_baseUrl}/{_devId}/markClaim", ""))
                {
                    webRequest.uploadHandler = new UploadHandlerRaw(signTransactionBodyData);
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    yield return webRequest.SendWebRequest();

                    if (!bool.Parse(webRequest.downloadHandler.text))
                    {
                        callbackFunction.Invoke(new TransactionData()
                        {
                            IsSuccess = false,
                            Message = "Transaction was already claimed"
                        });
                    }
                    else
                    {
                        var transactionData = ExtractTransactionData(signedTransactionDataResponse.TransactionDataBase64String);
                        callbackFunction(new TransactionData()
                        {
                            Amount = transactionData.Amount,
                            CoinName = transactionData.CoinName,
                            IsSuccess = true
                        });
                    }
                }
            }
            else
            {
                callbackFunction.Invoke(new TransactionData()
                {
                    IsSuccess = false,
                    Message = "Invalid signature"
                });
            }

            TransactionData ExtractTransactionData(string web2IntegrationResponseString) =>
                JsonConvert.DeserializeObject<TransactionData>(
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(web2IntegrationResponseString)));
        }

        private string GetEncryptedTransactionIdString(string transactionId, RSA rsa) =>
            Convert.ToBase64String(
               rsa.Encrypt(
                   Encoding.UTF8.GetBytes(transactionId),
                   RSAEncryptionPadding.Pkcs1));

        private void StartGetSignedTransactionDataResponseRoutine(string publicKey, string transactionId, Action<TransactionData> callback)
        {
            StartCoroutine(GetSignedTransactionDataResponseRoutine(publicKey, transactionId, callback, StartVerifySignatureAndClaimTransactionRoutine));
        }

        private void StartVerifySignatureAndClaimTransactionRoutine(
            string transactionId,
            string signedResponse,
            RSA rsa,
            RSAPKCS1SignatureDeformatter rsaDeformatter,
            Action<TransactionData> callback)
        {
            StartCoroutine(VerifySignatureAndClaimTransactionRoutine(transactionId, signedResponse, rsa, rsaDeformatter, callback));
        }
    }

    public class Request
    {
        public string TransactionId { get; set; }
    }

    public class SignedTransactionDataResponse
    {
        public bool Success { get; set; }
        public string TransactionDataBase64String { get; set; }
        public string HashBase64String { get; set; }
        public string SignatureBase64String { get; set; }
    }
}