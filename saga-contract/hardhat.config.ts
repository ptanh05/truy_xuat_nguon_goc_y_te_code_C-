import { HardhatUserConfig } from "hardhat/config";
import "@nomicfoundation/hardhat-toolbox";
import * as dotenv from "dotenv";

dotenv.config({ path: __dirname + "/.env" });

const { DEPLOYER_PRIVATE_KEY, PHARMADNA_RPC, PHARMADNA_CHAIN_ID } = process.env;

const networks: HardhatUserConfig["networks"] = {};

if (PHARMADNA_RPC) {
  networks.pharmadna = {
    url: PHARMADNA_RPC,
    chainId: PHARMADNA_CHAIN_ID ? Number(PHARMADNA_CHAIN_ID) : undefined,
    accounts: DEPLOYER_PRIVATE_KEY ? [DEPLOYER_PRIVATE_KEY] : []
  };
}

const config: HardhatUserConfig = {
  solidity: {
    compilers: [
      { version: "0.8.20" },
      { version: "0.8.28" }
    ]
  },
  networks
};

export default config;
