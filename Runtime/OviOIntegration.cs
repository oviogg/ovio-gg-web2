using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OviO.Integration
{
    public class OviOIntegration
    {
        private readonly HttpClient _httpClient;

        private Uri _baseUrl = new Uri($"http://localhost:60814/api/gameIntegration/");

        private string _devId;

        public OviOIntegration(string devId)
        {
            _httpClient = new HttpClient();
            _devId = devId;
        }

        public async Task<TransactionData> GetAmountAsync(string url)
        {
            var transactionId = url.Split('/').Last();

            var rsa = RSA.Create();
            var publicKey = await GetPublicKeyAsync();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm(nameof(SHA256));

            var transactionIdEncryptedString = GetEncryptedTransactionIdString();
            var request = new Request
            {
                TransactionId = transactionIdEncryptedString
            };

            var signedTransactionDataResponse = await GetSignedTransactionDataResponseAsync();
            if (!signedTransactionDataResponse.Success) 
            {
                return new TransactionData()
                {
                    IsSuccess = false,
                    Message = "An error occurred"
                };
            }
            var hashedTransactionDataResponse = Convert.FromBase64String(signedTransactionDataResponse.HashBase64String);
            var signature = Convert.FromBase64String(signedTransactionDataResponse.SignatureBase64String);

            if (rsaDeformatter.VerifySignature(hashedTransactionDataResponse, signature))
            {
                if (!await MarkClaimedTransactionAsync())
                {
                    return new TransactionData()
                    {
                        IsSuccess = false,
                        Message = "Transaction was already claimed"
                    };
                }

                var transactionData = ExtractTransactionData(signedTransactionDataResponse.TransactionDataBase64String);

                return new TransactionData()
                {
                    Amount = transactionData.Amount,
                    CoinName = transactionData.CoinName,
                    IsSuccess = true
                };
            }

            return new TransactionData()
            {
                IsSuccess = false,
                Message = "Invalid signature"
            };

            async Task<string> GetPublicKeyAsync()
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUrl, $"{_devId}/publicKey"));
                var response = await _httpClient.SendAsync(httpRequest);

                return await response.Content.ReadAsStringAsync();
            }

            async Task<SignedTransactionDataResponse> GetSignedTransactionDataResponseAsync()
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(_baseUrl, $"{_devId}/transactionData"));

                httpRequest.Content = new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json");

                var response = await _httpClient.SendAsync(httpRequest);

                return JsonConvert.DeserializeObject<SignedTransactionDataResponse>(await response.Content.ReadAsStringAsync());
            }

            async Task<bool> MarkClaimedTransactionAsync()
            {
                using var httpClaimRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(_baseUrl, $"{_devId}/markClaim"));

                httpClaimRequest.Content =
                    new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json");

                var claimResponse = await _httpClient.SendAsync(httpClaimRequest);

                return bool.Parse(await claimResponse.Content.ReadAsStringAsync());
            }

            string GetEncryptedTransactionIdString() =>
                Convert.ToBase64String(
                        rsa.Encrypt(
                            Encoding.UTF8.GetBytes(transactionId),
                            RSAEncryptionPadding.Pkcs1));

            TransactionData ExtractTransactionData(string web2IntegrationResponseString) =>
                JsonConvert.DeserializeObject<TransactionData>(
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(web2IntegrationResponseString)));
        }

        public TransactionData GetAmount(string url) => GetAmountAsync(url).GetAwaiter().GetResult();
    }

    public class TransactionData
    {
        public int Amount { get; set; }
        public string CoinName { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
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