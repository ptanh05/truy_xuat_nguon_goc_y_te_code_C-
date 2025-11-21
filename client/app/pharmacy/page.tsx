"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { QrCode, Search, Package, Truck } from "lucide-react";
import QRScanner from "@/components/QRScanner";
import RoleGuard from "@/components/RoleGuard";
import { useWallet } from "@/hooks/useWallet";
import PharmacyTransferRequests from "@/components/PharmacyTransferRequests";
import { toast } from "sonner";
import { API_BASE_URL } from "@/lib/api";

function PharmacyContent() {
  const [scanMode, setScanMode] = useState<"qr" | "manual">("qr");
  const [batchNumber, setBatchNumber] = useState("");
  const [drugData, setDrugData] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [milestones, setMilestones] = useState<any[]>([]);
  const [showTransferRequests, setShowTransferRequests] = useState(false);
  const [nftList, setNftList] = useState<any[]>([]);
  const [pendingTransferCount, setPendingTransferCount] = useState(0);

  const { account } = useWallet();

  const resolveMediaUrl = (url?: string | null) => {
    if (!url) return "";
    const trimmed = url.trim();
    if (!trimmed) return "";
    if (/^https?:\/\//i.test(trimmed)) {
      return trimmed;
    }

    let cleaned = trimmed
      .replace(/^ipfs:\/\//i, "")
      .replace(/^ipfs\//i, "")
      .replace(/^\/+/i, "");

    if (cleaned.startsWith("ipfs/")) {
      cleaned = cleaned.substring(5);
    }

    const encoded = encodeURIComponent(cleaned);
    return `${API_BASE_URL}/manufacturer/ipfs-file/${encoded}`;
  };

  // Lấy danh sách NFTs trong pharmacy khi vào trang
  const fetchNFTsInPharmacy = async () => {
    try {
      const { api } = await import("@/lib/api");
      const data = await api.get("/pharmacy");
      setNftList(Array.isArray(data) ? data : []);
    } catch (error) {
      setNftList([]);
    }
  };

  useEffect(() => {
    fetchNFTsInPharmacy();
  }, []);

  const fetchPendingTransferCount = async () => {
    if (!account) {
      setPendingTransferCount(0);
      return;
    }
    try {
      const { API_BASE_URL } = await import("@/lib/api");
      const res = await fetch(
        `${API_BASE_URL}/distributor/transfer-to-pharmacy?pharmacyAddress=${account}&status=pending`
      );
      if (res.ok) {
        const data = await res.json();
        setPendingTransferCount(Array.isArray(data) ? data.length : 0);
      } else {
        setPendingTransferCount(0);
      }
    } catch {
      setPendingTransferCount(0);
    }
  };

  useEffect(() => {
    fetchPendingTransferCount();
  }, [account]);

  // Auto-refresh mỗi 10 giây
  useEffect(() => {
    const interval = setInterval(() => {
      fetchNFTsInPharmacy();
      fetchPendingTransferCount();
      // Refresh milestones nếu đang xem drugData
      if (drugData?.batchNumber) {
        lookupDrug(drugData.batchNumber);
      }
    }, 10000);

    return () => clearInterval(interval);
  }, [drugData]);

  const handleQRScan = (result: string) => {
    setBatchNumber(result);
    lookupDrug(result);
  };

  const lookupDrug = async (batchNumberValue: string) => {
    setIsLoading(true);
    try {
      // Lấy thông tin NFT theo batch_number
      const { API_BASE_URL } = await import("@/lib/api");
      const nftRes = await fetch(
        `${API_BASE_URL}/manufacturer?batchNumber=${encodeURIComponent(batchNumberValue)}`
      );
      const nftData = await nftRes.json();
      const hasBatchNumber =
        nftData?.batchNumber ?? nftData?.batch_number ?? null;
      if (!nftRes.ok || !nftData || !hasBatchNumber) {
        setDrugData(null);
        setMilestones([]);
        toast.error("Không tìm thấy lô thuốc với số lô này");
        setIsLoading(false);
        return;
      }
      const normalizedNft = {
        id: nftData.id,
        name: nftData.name,
        batchNumber: nftData.batchNumber ?? nftData.batch_number ?? "",
        manufactureDate:
          nftData.manufactureDate ?? nftData.manufacture_date ?? "",
        expiryDate: nftData.expiryDate ?? nftData.expiry_date ?? "",
        description: nftData.description ?? "",
        formulation: nftData.formulation ?? "",
        status: nftData.status ?? "",
        manufacturerAddress:
          nftData.manufacturerAddress ?? nftData.manufacturer_address ?? "",
        distributorAddress:
          nftData.distributorAddress ?? nftData.distributor_address ?? "",
        pharmacyAddress:
          nftData.pharmacyAddress ?? nftData.pharmacy_address ?? "",
        imageUrl: nftData.imageUrl ?? nftData.image_url ?? "",
      };
      setDrugData(normalizedNft);
      // Lấy lịch sử vận chuyển
      const msRes = await fetch(
        `${API_BASE_URL}/manufacturer/milestone?nftId=${nftData.id}`
      );
      const msData = await msRes.json();
      const normalizedMilestones = Array.isArray(msData)
        ? (msData as any[])
            .map((m) => ({
              id: m.id,
              type: m.type,
              description: m.description,
              location: m.location,
              timestamp:
                m.timestamp ??
                m.timeStamp ??
                m.Timestamp ??
                m.createdAt ??
                m.created_at,
              actorAddress: (m.actorAddress ?? m.actor_address ?? "").toLowerCase(),
            }))
            .sort(
              (a, b) =>
                new Date(a.timestamp).getTime() -
                new Date(b.timestamp).getTime()
            )
        : [];
      setMilestones(normalizedMilestones);
    } catch (error) {
      toast.error("Có lỗi xảy ra khi tra cứu");
      setDrugData(null);
      setMilestones([]);
    } finally {
      setIsLoading(false);
    }
  };


  return (
    <div className="max-w-6xl mx-auto p-6">
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Kiểm tra và xác nhận lô thuốc
            </h1>
            <p className="text-gray-600">
              Quét QR hoặc nhập số lô để xác minh và nhập kho
            </p>
          </div>
          <Button
            variant="outline"
            onClick={() => setShowTransferRequests(!showTransferRequests)}
            className="flex items-center"
          >
            <Truck className="w-4 h-4 mr-2" />
            Yêu cầu chuyển lô ({pendingTransferCount})
          </Button>
        </div>
      </div>

      <div className="grid lg:grid-cols-2 gap-8">
        {/* Scanner Section */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <QrCode className="w-5 h-5 mr-2" />
              Quét mã QR hoặc nhập thủ công
            </CardTitle>
            <CardDescription>
              Sử dụng camera để quét QR trên hộp thuốc hoặc nhập Số lô
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex gap-2 mb-4">
              <Button
                variant={scanMode === "qr" ? "default" : "outline"}
                onClick={() => setScanMode("qr")}
                size="sm"
              >
                <QrCode className="w-4 h-4 mr-1" />
                Quét QR
              </Button>
              <Button
                variant={scanMode === "manual" ? "default" : "outline"}
                onClick={() => setScanMode("manual")}
                size="sm"
              >
                <Search className="w-4 h-4 mr-1" />
                Nhập thủ công
              </Button>
            </div>

            {scanMode === "qr" ? (
              <QRScanner onScan={handleQRScan} />
            ) : (
              <div className="space-y-3">
                <div>
                  <Label htmlFor="batchNumber">Số lô thuốc</Label>
                  <Input
                    id="batchNumber"
                    value={batchNumber}
                    onChange={(e) => setBatchNumber(e.target.value)}
                    placeholder="Nhập số lô thuốc (ví dụ: BN123, BN456)"
                  />
                </div>
                <Button
                  onClick={() => lookupDrug(batchNumber)}
                  disabled={!batchNumber || isLoading}
                  className="w-full"
                >
                  {isLoading ? "Đang tra cứu..." : "Tra cứu"}
                </Button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Drug Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Package className="w-5 h-5 mr-2" />
              Thông tin lô thuốc
            </CardTitle>
            <CardDescription>Chi tiết về lô thuốc được quét</CardDescription>
          </CardHeader>
          <CardContent>
            {!drugData ? (
              <div className="text-center py-8 text-gray-500">
                <QrCode className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>Quét QR hoặc nhập Số lô để xem thông tin</p>
              </div>
            ) : (
              <div>
                <div className="mb-4">
                  <div className="font-mono text-xs text-gray-500 mb-1">
                    Số lô: {drugData.batchNumber}
                  </div>
                  <div className="font-bold text-lg mb-1">{drugData.name}</div>
                  <div className="text-sm text-gray-700 mb-1">
                    ID: {drugData.id}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Ngày sản xuất: {drugData.manufactureDate}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Hạn dùng: {drugData.expiryDate}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Mô tả: {drugData.description}
                  </div>
                  {drugData.formulation && (
                    <div className="text-sm text-gray-700 mb-1">
                      Dạng bào chế: {drugData.formulation}
                    </div>
                  )}
                  <div className="text-sm text-gray-700 mb-1">
                    Trạng thái: <b>{drugData.status}</b>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Manufacturer:{" "}
                    <span className="font-mono text-xs">
                      {drugData.manufacturerAddress}
                    </span>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Distributor:{" "}
                    <span className="font-mono text-xs">
                      {drugData.distributorAddress}
                    </span>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Pharmacy:{" "}
                    <span className="font-mono text-xs">
                      {drugData.pharmacyAddress}
                    </span>
                  </div>
                  {resolveMediaUrl(drugData.imageUrl) ? (
                    <img
                      src={resolveMediaUrl(drugData.imageUrl)}
                      alt={`Ảnh lô thuốc ${drugData.name}`}
                      className="max-w-xs rounded my-2 border"
                    />
                  ) : (
                    <div className="text-xs text-gray-500 my-2">
                      Không có ảnh hiển thị
                    </div>
                  )}
                </div>
                {/* Nút xác nhận nhập kho */}
                <div className="mt-6">
                  <h4 className="font-semibold mb-2">Lịch sử vận chuyển</h4>
                  {milestones.length === 0 ? (
                    <div className="text-sm text-gray-500">
                      Chưa có mốc vận chuyển nào
                    </div>
                  ) : (
                    <div className="overflow-x-auto">
                      <table className="min-w-full text-xs border">
                        <thead>
                          <tr className="bg-gray-100">
                            <th className="px-2 py-1 border">Thời gian</th>
                            <th className="px-2 py-1 border">Loại mốc</th>
                            <th className="px-2 py-1 border">Mô tả</th>
                            <th className="px-2 py-1 border">Vị trí</th>
                            <th className="px-2 py-1 border">
                              Người thực hiện
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          {milestones.map((m: any) => (
                            <tr key={m.id}>
                              <td className="border px-2 py-1">
                                {new Date(m.timestamp).toLocaleString()}
                              </td>
                              <td className="border px-2 py-1">{m.type}</td>
                              <td className="border px-2 py-1">
                                {m.description}
                              </td>
                              <td className="border px-2 py-1">{m.location || "-"}</td>
                              <td className="border px-2 py-1 font-mono text-xs">
                                {m.actorAddress}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Danh sách NFTs trong pharmacy */}
      <div className="mt-8">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Package className="w-5 h-5 mr-2" />
              Danh sách lô thuốc trong kho ({nftList.length})
            </CardTitle>
            <CardDescription>
              Tất cả lô thuốc đã được xác nhận nhập kho
            </CardDescription>
          </CardHeader>
          <CardContent>
            {nftList.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <Package className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>Chưa có lô thuốc nào trong kho</p>
              </div>
            ) : (
              <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
                {nftList.map((nft) => (
                  <div
                    key={nft.id}
                    className="p-4 border rounded-lg hover:bg-gray-50 cursor-pointer"
                    onClick={() => {
                      if (nft.batchNumber) {
                        setBatchNumber(nft.batchNumber);
                        lookupDrug(nft.batchNumber);
                      }
                    }}
                  >
                    <div className="font-semibold mb-1">#{nft.id} - {nft.name}</div>
                    {nft.batchNumber && (
                      <div className="text-sm text-gray-600 mb-1">
                        Số lô: {nft.batchNumber}
                      </div>
                    )}
                    <div className="text-xs text-gray-500">
                      Trạng thái: {nft.status}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Yêu cầu chuyển lô từ nhà phân phối */}
      {showTransferRequests && (
        <div className="mt-8">
          <PharmacyTransferRequests
            pharmacyAddress={account || ""}
            onPendingCountChange={setPendingTransferCount}
          />
        </div>
      )}
    </div>
  );
}

export default function PharmacyPage() {
  return (
    <RoleGuard requiredRoles={["PHARMACY"]}>
      <PharmacyContent />
    </RoleGuard>
  );
}
