@echo off
REM Script để deploy PharmaNFT contract lên PharmaDNAVN chainlet
echo 🚀 Deploying PharmaNFT contract to PharmaDNAVN chainlet...

REM Kiểm tra xem đã compile chưa
if not exist "artifacts" (
    echo 📦 Compiling contracts...
    npx hardhat compile
)

REM Deploy contract
echo 🔨 Deploying contract...
npx hardhat run scripts/deployPharmaNFT.ts --network pharmadna

echo ✅ Deployment completed!
echo.
echo 📋 Next steps:
echo 1. Copy the deployed contract address
echo 2. Update PHARMA_NFT_ADDRESS in your .env file
echo 3. Restart your Next.js application
echo.
echo 🌐 PharmaDNAVN Chainlet Details:
echo Chain ID: 2763717455037000 (0x9d1961d2ac248)
echo RPC: https://pharmadnavn-2763717455037000-1.jsonrpc.sagarpc.io
echo Explorer: https://pharmadnavn-2763717455037000-1.sagaexplorer.io
pause
