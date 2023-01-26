# OviO Integration SDK

## Description
This package introduces Web2 gaming Integration with OviO Gaming Ltd.

## Installation
Using unity go to **Window** -> **PackageManager** -> **+** -> **Add package from Git Url**
Paste this link:
https://github.com/oviogg/ovio-gg-web2.git

## Usage
### Initialization
```
new OviOIntegration(string devId)
```

Use the devId provided to you by OviO.

### GetAmountAsync
#### <u>Parameters</u>
**url** *(string)* - the deeplink url that used to access your game

#### <u>Returns</u>

_CoinData class_

+ **Amount**  *(int)* - total number of coins that need to be transfered to the gamer.

+ **CoinName** *(string)* - the name of the currency that needs to be transfered to the gamer.

+ **IsSuccess** *(bool)* - indicates whether the call was successful or not.

+ **Message** *(string)* - on non-successful calls this will indicate the error.



This is an asynchronus function to call OviO and get the coin data regarding the gamer transactions.

### GetAmount
#### <u>Parameters</u>
**url** *(string)* - the deeplink url that used to access your game

#### <u>Returns</u>

_CoinData class_

+ **Amount**  *(int)* - total number of coins that need to be transfered to the gamer.

+ **CoinName** *(string)* - the name of the currency that needs to be transfered to the gamer.

+ **IsSuccess** *(bool)* - indicates whether the call was successful or not.

+ **Message** *(string)* - on non-successful calls this will indicate the error.


This is a synchronus function to call OviO and get the coin data regarding the gamer transactions.
