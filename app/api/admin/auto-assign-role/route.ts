import { NextRequest, NextResponse } from 'next/server';
import { ethers } from "ethers";
import pharmaNFTAbi from "@/lib/pharmaNFT-abi.json";

const PHARMA_NFT_ADDRESS = process.env.NEXT_PUBLIC_PHARMA_NFT_ADDRESS;
const PHARMADNA_RPC = process.env.PHARMADNA_RPC || "https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io";
const OWNER_PRIVATE_KEY = process.env.OWNER_PRIVATE_KEY;

export async function POST(req: NextRequest) {
  const { address } = await req.json();
  if (!address) return NextResponse.json({ error: "Thiếu địa chỉ" }, { status: 400 });
  try {
    if (!OWNER_PRIVATE_KEY) throw new Error("OWNER_PRIVATE_KEY not set");
    const provider = new ethers.JsonRpcProvider(PHARMADNA_RPC);
    if (!PHARMA_NFT_ADDRESS || PHARMA_NFT_ADDRESS === '0x' || PHARMA_NFT_ADDRESS.length < 10) {
      throw new Error("PHARMA_NFT_ADDRESS is not configured");
    }
    const code = await provider.getCode(PHARMA_NFT_ADDRESS);
    if (!code || code === '0x') {
      throw new Error(`No contract code at PHARMA_NFT_ADDRESS: ${PHARMA_NFT_ADDRESS}`);
    }
    const ownerWallet = new ethers.Wallet(OWNER_PRIVATE_KEY, provider);
    const contract = new ethers.Contract(PHARMA_NFT_ADDRESS, pharmaNFTAbi.abi || pharmaNFTAbi, ownerWallet);
    // Gán role Manufacturer (1)
    const tx = await contract.assignRole(address, 1);
    await tx.wait();
    return NextResponse.json({ success: true, txHash: tx.hash });
  } catch (err: any) {
    return NextResponse.json({ error: err.message }, { status: 500 });
  }
} 