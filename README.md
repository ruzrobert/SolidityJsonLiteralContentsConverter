# SolidityJsonLiteralContentsConverter
This console app converts metadata json file from Remix IDE to literal contents enabled json compatible with Etherscan / Polygonscan

Hello Github!

To verify a smart contract on Etherscan / Polygonscan we need a Standard-Json-Input json file, with literal contents for each script.

At the moment, it seems that Remix IDE isn't creating that json in the needed format.

I've created this simple console app, which converts the Remix IDE json file to a json file thats needed for the verification on the Etherscan.

Just download the YourContract_metadata.json file from Remix, paste the file path to this console app, and it will generate a proper json file in the same directory.

At the moment, dependent contracts are being searched in the IPFS. If a file isn't available, you'll be asked to copy your contract file contents and press the Enter button twice.

Good luck!
