# OviO Integration SDK
## Description

This package introduces Web2 gaming Integration with OviO Gaming Ltd.
The integration includes getting information from OviO regarding the amount that needs to be updated for certain user.


**Note**: If you're adding your code to an editor file, you will need to create an .asmdef file and add an assembly reference to com.ovio.integration.
  

## Installation

Using unity go to **Window** -> **PackageManager** -> **+** -> **Add package from Git Url**

Paste this link:
[https://github.com/oviogg/ovio-gg-web2.git](https://github.com/oviogg/ovio-gg-web2.git)

## Usage

### OviOIntegration
Use this class if you handle your deeplink processing on your own.

#### Initializion
In your MonoBehaviour file which handles deeplink processing initialize the OviOIntegration class in the Awake() or Start() function.

```c#
Using OviO.Integration;

public class ExampleMonoBehaviour : MonoBehaviour {
	private OviOIntegration ovioIntegration;
	
	public void Awake()
	{
		ovioIntegration = new OviOIntegration(<YOUR_OWN_DEV_ID>);
	}
}
```

**OR**
```c#
Using OviO.Integration;

public class ExampleMonoBehaviour : MonoBehaviour {
	private OviOIntegration ovioIntegration;
	
	public void Start()
	{
		ovioIntegration = new OviOIntegration(<YOUR_OWN_DEV_ID>);
	}
}
```

**devId** is a *string* provided to you by OviO.

> ⚠️  **Important:** Do not share this devId with anybody. This is your own game unique and secret identifier.
  
  #### GetAmount
  GetAmount(*string url, Action\<TranscationData\> callback*) gets the deeplink to your application and a callback that handles the [TransactionData](#TransactionData) returned from the function.
  
  <u>Usage</u>
  In your function that handles deeplink processing, call ovioIntegration.GetAmount with a callback that handles the return value of GetAmount (the *TransactionData* class)
```c#
Using OviO.Integration;

public class ExampleMonoBehaviour : MonoBehaviour {
	private OviOIntegration ovioIntegration;
	
	public void HandleDeepLink(string link)
	{
		// Your own deeplink processing
		
		if (link.ToLower().Contains("ovio"))
        	{
            		ovioIntegration.GetAmount(link, HandleTransactionData);
        	}
	}

	private void HandleTransactionData(TransactionData transactionData)
	{
		/** Your own handling of transactionData
		 For example:
		 if (!transactionData.IsSuccess)
		 {
		     Debug.LogError($"OviOIntegration failed {transactionData.Message}");
		 }
		 else if (transactionData.CoinName != "<YOUR-GAME-COIN-NAME>")
		 {
		     // Handle not legal CoinName
		 }
		 else
		 {
			 // Transfer transactionData.Amount to gamer
		 }
		**/ 
	}
}
```

### OviOIntegrationDeepLinkProcessing
Use this class if you don't currently handle deeplink processing on your own and want OviO to handle it for you.

#### Initializion
In your MonoBehaviour file which handles deeplink processing initialize the OviOIntegrationDeepLinkProcessing class in the Awake() or Start() function.
Register your own callback to handle TransactionData that returns from transactions' integration with OviO.

```c#
Using OviO.Integration;

public class ExampleMonoBehaviour : MonoBehaviour {
	private OviOIntegrationDeepLinkProcessing ovioIntegrationDeepLinkProcessing;
	
	public void Awake()
	{
		ovioIntegrationDeepLinkProcessing = new OviOIntegrationDeepLinkProcessing(<YOUR_OWN_DEV_ID>);
		ovioIntegrationDeepLinkProcessing.RegisterCallback(HandleTransactionData);
	}

	private void HandleTransactionData(TransactionData transactionData)
	{
		/** Your own handling of transactionData
		 For example:
		 if (!transactionData.IsSuccess)
		 {
		     Debug.LogError($"OviOIntegration failed {transactionData.Message}");
		 }
		 else if (transactionData.CoinName != "<YOUR-GAME-COIN-NAME>")
		 {
		     // Handle not legal CoinName
		 }
		 else
		 {
			 // Transfer transactionData.Amount to gamer
		 }
		**/ 
	}
}
```

**OR**
```c#
Using OviO.Integration;

public class ExampleMonoBehaviour : MonoBehaviour {
	private OviOIntegrationDeepLinkProcessing ovioIntegrationDeepLinkProcessing;
	
	public void Start()
	{
		ovioIntegrationDeepLinkProcessing = new OviOIntegrationDeepLinkProcessing(<YOUR_OWN_DEV_ID>);
		ovioIntegrationDeepLinkProcessing.RegisterCallback(HandleTransactionData);
	}

	private void HandleTransactionData(TransactionData transactionData)
	{
		/** Your own handling of transactionData
		 For example:
		 if (!transactionData.IsSuccess)
		 {
		     Debug.LogError($"OviOIntegration failed {transactionData.Message}");
		 }
		 else if (transactionData.CoinName != "<YOUR-GAME-COIN-NAME>")
		 {
		     // Handle not legal CoinName
		 }
		 else
		 {
			 // Transfer transactionData.Amount to gamer
		 }
		**/ 
	}
}
```

**devId** is a *string* provided to you by OviO.

> ⚠️  **Important:** Do not share this devId with anybody. This is your own game unique and secret identifier.



<a name="TransactionData"></a>
## TransactionData 
+  **Amount**  *(int)* - total number of coins that the gamer purchased in the transaction and need to be transferred to the gamer.
+  **CoinName**  *(string)* - the name of the currency that needs to be transferred to the gamer. Can be used to check legitimacy of the transaction.
+  **IsSuccess**  *(bool)* - indicates whether the call was successful or not. 
+  **Message**  *(string)* - on non-successful calls this will indicate the error.
