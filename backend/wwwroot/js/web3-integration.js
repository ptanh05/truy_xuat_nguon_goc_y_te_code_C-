// Web3 Integration for PharmaDNA
class PharmaDNAWeb3 {
  constructor() {
    this.web3 = null
    this.account = null
    this.contractAddress = null
  }

  async connectWallet() {
    try {
      if (typeof window.ethereum !== "undefined") {
        const accounts = await window.ethereum.request({
          method: "eth_requestAccounts",
        })
        this.account = accounts[0]
        localStorage.setItem("walletAddress", this.account)
        console.log("Wallet connected:", this.account)
        return this.account
      } else {
        alert("Vui lòng cài đặt MetaMask hoặc ví Web3 khác")
        return null
      }
    } catch (error) {
      console.error("Error connecting wallet:", error)
      return null
    }
  }

  async disconnectWallet() {
    this.account = null
    localStorage.removeItem("walletAddress")
    console.log("Wallet disconnected")
  }

  getConnectedAccount() {
    return this.account || localStorage.getItem("walletAddress")
  }

  async switchToPharmaDNANetwork() {
    try {
      const chainId = "0x" + (2759821881746000).toString(16)
      await window.ethereum.request({
        method: "wallet_switchEthereumChain",
        params: [{ chainId: chainId }],
      })
    } catch (error) {
      if (error.code === 4902) {
        // Network not added, add it
        await this.addPharmaDNANetwork()
      }
    }
  }

  async addPharmaDNANetwork() {
    try {
      await window.ethereum.request({
        method: "wallet_addEthereumChain",
        params: [
          {
            chainId: "0x" + (2759821881746000).toString(16),
            chainName: "PharmaDNA Chainlet",
            rpcUrls: ["https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io"],
            nativeCurrency: {
              name: "Saga",
              symbol: "SAGA",
              decimals: 18,
            },
          },
        ],
      })
    } catch (error) {
      console.error("Error adding network:", error)
    }
  }

  async signMessage(message) {
    try {
      const signature = await window.ethereum.request({
        method: "personal_sign",
        params: [message, this.account],
      })
      return signature
    } catch (error) {
      console.error("Error signing message:", error)
      return null
    }
  }

  async verifySignature(message, signature) {
    try {
      const response = await fetch("/api/auth/verify-signature", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          message: message,
          signature: signature,
          address: this.account,
        }),
      })
      return await response.json()
    } catch (error) {
      console.error("Error verifying signature:", error)
      return null
    }
  }
}

// Initialize Web3 instance
const pharmaDNAWeb3 = new PharmaDNAWeb3()

// Setup wallet connection button
document.addEventListener("DOMContentLoaded", () => {
  const connectBtn = document.getElementById("walletConnect")
  if (connectBtn) {
    connectBtn.addEventListener("click", async (e) => {
      e.preventDefault()
      const account = await pharmaDNAWeb3.connectWallet()
      if (account) {
        await pharmaDNAWeb3.switchToPharmaDNANetwork()
        connectBtn.textContent = account.substring(0, 6) + "..." + account.substring(38)
        connectBtn.classList.add("active")
      }
    })
  }

  const connectWalletBtn = document.getElementById("connectWalletBtn")
  if (connectWalletBtn) {
    connectWalletBtn.addEventListener("click", async () => {
      const account = await pharmaDNAWeb3.connectWallet()
      if (account) {
        await pharmaDNAWeb3.switchToPharmaDNANetwork()
        alert("Ví đã kết nối: " + account)
        window.location.reload()
      }
    })
  }
})

// Listen for account changes
if (typeof window.ethereum !== "undefined") {
  window.ethereum.on("accountsChanged", (accounts) => {
    if (accounts.length > 0) {
      pharmaDNAWeb3.account = accounts[0]
      localStorage.setItem("walletAddress", accounts[0])
      console.log("Account changed:", accounts[0])
    } else {
      pharmaDNAWeb3.disconnectWallet()
    }
  })

  window.ethereum.on("chainChanged", (chainId) => {
    console.log("Chain changed:", chainId)
    window.location.reload()
  })
}
