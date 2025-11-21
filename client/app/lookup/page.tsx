"use client";

import { useState } from "react";
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
import {
  QrCode,
  Search,
  Shield,
  AlertTriangle,
  CheckCircle,
  MapPin,
} from "lucide-react";
import QRScanner from "@/components/QRScanner";
import { toast } from "sonner";

const statusConfigs: Record<
  string,
  { title: string; description: string; color: string; Icon: typeof CheckCircle }
> = {
  authentic: {
    title: "Thuốc chính hãng ✓",
    description: "Lô thuốc đã được xác thực trên blockchain.",
    color: "bg-green-100 text-green-800 border-green-200",
    Icon: CheckCircle,
  },
  warning: {
    title: "Thuốc cảnh báo ⚠️",
    description: "Hãy liên hệ nhà sản xuất hoặc nhà thuốc để kiểm tra thêm.",
    color: "bg-yellow-100 text-yellow-800 border-yellow-200",
    Icon: AlertTriangle,
  },
  not_found: {
    title: "Không xác minh được",
    description: "Không tìm thấy thông tin lô thuốc trong hệ thống.",
    color: "bg-red-100 text-red-800 border-red-200",
    Icon: AlertTriangle,
  },
  default: {
    title: "Đang xác minh",
    description: "Kết quả sẽ hiển thị ngay sau khi quét.",
    color: "bg-gray-100 text-gray-800 border-gray-200",
    Icon: Shield,
  },
};

const resolveMediaUrl = (url?: string | null) => {
  if (!url) return "";
  const trimmed = url.trim();
  if (!trimmed) return "";
  if (/^https?:\/\//i.test(trimmed)) {
    return trimmed;
  }

  let cleaned = trimmed.replace(/^ipfs:\/\//i, "").replace(/^ipfs\//i, "");
  if (cleaned.startsWith("ipfs/")) {
    cleaned = cleaned.substring(5);
  }

  const encoded = encodeURIComponent(cleaned);
  return `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:5196/api"}/manufacturer/ipfs-file/${encoded}`;
};

export default function LookupPage() {
  const [scanMode, setScanMode] = useState<"qr" | "manual">("qr");
  const [batchName, setBatchName] = useState("");
  const [drugData, setDrugData] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [milestones, setMilestones] = useState<any[]>([]);

  const handleQRScan = (result: string) => {
    setBatchName(result);
    lookupDrug(result);
  };

  const lookupDrug = async (query: string) => {
    const searchValue = query.trim();
    if (!searchValue) {
      toast.error("Vui lòng nhập thông tin lô thuốc");
      return;
    }

    setIsLoading(true);
    setDrugData(null);
    setMilestones([]);
    
    try {
      const { API_BASE_URL } = await import("@/lib/api");

      const fetchByParam = async (param: "batchNumber" | "name" | "id" | "nftId") => {
        try {
          const url = `${API_BASE_URL}/manufacturer?${param}=${encodeURIComponent(searchValue)}`;
          console.log(`[Lookup] Fetching by ${param}:`, url);
          
          const res = await fetch(url);
          
          if (!res.ok) {
            // If 404, return null (not found)
            if (res.status === 404) {
              console.log(`[Lookup] Not found (404) for ${param}`);
              return null;
            }
            // For other errors, log and return null
            console.warn(`[Lookup] API error for ${param}:`, res.status, res.statusText);
            return null;
          }
          
          const data = await res.json();
          console.log(`[Lookup] Response for ${param}:`, data);
          
          // Check if data is valid and not empty
          if (!data) {
            console.log(`[Lookup] No data returned for ${param}`);
            return null;
          }
          
          // Check if it's an empty object (backend returns {} when not found)
          if (typeof data === 'object' && !Array.isArray(data) && Object.keys(data).length === 0) {
            console.log(`[Lookup] Empty object returned for ${param}`);
            return null;
          }
          
          // Check if it's an array with empty items
          if (Array.isArray(data) && data.length === 0) {
            console.log(`[Lookup] Empty array returned for ${param}`);
            return null;
          }
          
          // Check if it has at least one identifying field
          const hasId = data.id || data.Id;
          const hasName = data.name || data.Name;
          const hasBatchNumber = data.batchNumber || data.BatchNumber || data.batch_number;
          
          if (!hasId && !hasName && !hasBatchNumber) {
            console.log(`[Lookup] Data returned but no identifying fields for ${param}:`, data);
            return null;
          }
          
          console.log(`[Lookup] Valid data found for ${param}`);
          return data;
        } catch (err) {
          console.error(`[Lookup] Error fetching by ${param}:`, err);
          return null;
        }
      };

      // Try to fetch by different parameters in order
      let nftData = null;
      
      // First, try by ID (if it's a number)
      if (/^\d+$/.test(searchValue)) {
        nftData = await fetchByParam("id");
        if (!nftData) {
          nftData = await fetchByParam("nftId");
        }
      }
      
      // Then try by batchNumber
      if (!nftData) {
        nftData = await fetchByParam("batchNumber");
      }
      
      // Finally try by name
      if (!nftData) {
        nftData = await fetchByParam("name");
      }

      // Check if we have valid data
      const hasValidData =
        nftData &&
        (nftData.batchNumber ||
          nftData.batch_number ||
          nftData.name ||
          nftData.id ||
          (typeof nftData === 'object' && Object.keys(nftData).length > 0));

      if (!hasValidData) {
        setDrugData(null);
        setMilestones([]);
        toast.error("Không tìm thấy lô thuốc với thông tin đã nhập");
        return;
      }

      // Normalize status - if not provided, default to "authentic" if we have valid data
      // Handle both camelCase and PascalCase from backend
      const rawStatus = nftData.status ?? nftData.Status ?? nftData.metadata?.status ?? "";
      let normalizedStatus = rawStatus;
      if (!normalizedStatus && hasValidData) {
        normalizedStatus = "authentic";
      }
      normalizedStatus = normalizedStatus.toLowerCase().trim();

      const normalizedNft = {
        id: nftData.id ?? nftData.Id ?? 0,
        name: nftData.name ?? nftData.Name ?? nftData.metadata?.name ?? "",
        batchNumber: nftData.batchNumber ?? nftData.BatchNumber ?? nftData.batch_number ?? "",
        manufactureDate:
          nftData.manufactureDate ??
          nftData.manufacture_date ??
          nftData.metadata?.manufactureDate ??
          "",
        expiryDate:
          nftData.expiryDate ??
          nftData.expiry_date ??
          nftData.metadata?.expiryDate ??
          "",
        description:
          nftData.description ?? nftData.metadata?.description ?? "",
        formulation: nftData.formulation ?? nftData.metadata?.formulation ?? "",
        status: normalizedStatus,
        manufacturerAddress:
          nftData.manufacturerAddress ??
          nftData.manufacturer_address ??
          "",
        distributorAddress:
          nftData.distributorAddress ?? nftData.distributor_address ?? "",
        pharmacyAddress:
          nftData.pharmacyAddress ?? nftData.pharmacy_address ?? "",
        imageUrl:
          nftData.imageUrl ??
          nftData.image_url ??
          nftData.image ??
          nftData.metadata?.image ??
          "",
        certificateUrl:
          nftData.certificateUrl ??
          nftData.certificate_url ??
          nftData.metadata?.certificateUrl ??
          "",
        gtin: nftData.gtin ?? nftData.metadata?.gtin ?? "",
      };

      console.log("[Lookup] Normalized NFT data:", normalizedNft);
      setDrugData(normalizedNft);

      // Lấy lịch sử vận chuyển với error handling
      try {
        const msRes = await fetch(
          `${API_BASE_URL}/manufacturer/milestone?nftId=${normalizedNft.id}`
        );
        if (msRes.ok) {
          const msData = await msRes.json();
          const normalizedMilestones = Array.isArray(msData)
            ? msData
                .map((m: any) => ({
                  id: m.id,
                  type: m.type,
                  description: m.description,
                  location: m.location,
                  timestamp:
                    m.timestamp ??
                    m.timeStamp ??
                    m.createdAt ??
                    m.created_at ??
                    null,
                  actorAddress: (m.actorAddress ?? m.actor_address ?? "").toLowerCase(),
                }))
                .filter((m: any) => !!m.timestamp)
                .sort(
                  (a: any, b: any) =>
                    new Date(a.timestamp).getTime() -
                    new Date(b.timestamp).getTime()
                )
            : [];
          setMilestones(normalizedMilestones);
        } else {
          // If milestone fetch fails, just set empty array
          setMilestones([]);
        }
      } catch (msError) {
        // If milestone fetch fails, just set empty array and continue
        console.warn("Error fetching milestones:", msError);
        setMilestones([]);
      }
    } catch (error) {
      toast.error("Có lỗi xảy ra khi tra cứu");
      setDrugData(null);
      setMilestones([]);
    } finally {
      setIsLoading(false);
    }
  };

  const statusInfo =
    statusConfigs[drugData?.status?.toLowerCase()] ?? statusConfigs.default;

  return (
    <div className="max-w-6xl mx-auto p-6">
      <div className="mb-8 text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Tra cứu nguồn gốc thuốc
        </h1>
        <p className="text-gray-600">
          Xác minh tính xác thực và nguồn gốc của thuốc chỉ với một lần quét
        </p>
        <div className="mt-4 p-4 bg-blue-50 rounded-lg inline-block">
          <p className="text-sm text-blue-800">
            <Shield className="w-4 h-4 inline mr-1" />
            Dịch vụ miễn phí - Không cần kết nối ví
          </p>
        </div>
      </div>

      <div className="grid lg:grid-cols-2 gap-8">
        {/* Scanner Section */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <QrCode className="w-5 h-5 mr-2" />
              Quét mã QR trên hộp thuốc
            </CardTitle>
            <CardDescription>
              Sử dụng camera để quét QR hoặc nhập Token ID thủ công
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
                Nhập mã
              </Button>
            </div>

            {scanMode === "qr" ? (
              <QRScanner onScan={handleQRScan} />
            ) : (
              <div className="space-y-3">
                <div>
                  <Label htmlFor="batchName">Tên lô thuốc</Label>
                  <Input
                    id="batchName"
                    value={batchName}
                    onChange={(e) => setBatchName(e.target.value)}
                    placeholder="Nhập tên lô thuốc (ví dụ: Paracetamol 500mg - LOT2024001)"
                  />
                </div>
                <Button
                  onClick={() => lookupDrug(batchName)}
                  disabled={!batchName || isLoading}
                  className="w-full"
                >
                  {isLoading ? "Đang tra cứu..." : "Tra cứu"}
                </Button>
              </div>
            )}

            {/* Demo buttons */}
            <div className="border-t pt-4">
              <p className="text-sm text-gray-600 mb-2">Thử nghiệm:</p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => lookupDrug("1001")}
                >
                  Thuốc chính hãng
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => lookupDrug("9999")}
                >
                  Thuốc cảnh báo
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Results Section */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Shield className="w-5 h-5 mr-2" />
              Kết quả xác minh
            </CardTitle>
            <CardDescription>
              Thông tin chi tiết về nguồn gốc và tính xác thực
            </CardDescription>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-2">Đang xác minh...</span>
              </div>
            ) : drugData ? (
              <div className="space-y-6">
                {/* Status Banner */}
                <div className={`p-4 rounded-lg border-2 ${statusInfo.color}`}>
                  <div className="flex items-center">
                    <statusInfo.Icon className="w-6 h-6 text-current" />
                    <div className="ml-3">
                      <h3 className="font-semibold">{statusInfo.title}</h3>
                      <p className="text-sm mt-1">{statusInfo.description}</p>
                    </div>
                  </div>
                </div>
                {/* Drug Info */}
                <div>
                  <div className="font-mono text-xs text-gray-500 mb-1">
                    ID: {drugData.id}
                  </div>
                  <div className="font-bold text-lg mb-1">{drugData.name}</div>
                  <div className="text-sm text-gray-700 mb-1">
                    Số lô: {drugData.batchNumber || "-"}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Ngày sản xuất: {drugData.manufactureDate || "-"}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Hạn dùng: {drugData.expiryDate || "-"}
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Mô tả: {drugData.description}
                  </div>
                  {drugData.formulation && (
                    <div className="text-sm text-gray-700 mb-1">
                      Dạng bào chế: {drugData.formulation}
                    </div>
                  )}
                  {drugData.gtin && (
                    <div className="text-sm text-gray-700 mb-1">
                      GTIN: {drugData.gtin}
                    </div>
                  )}
                  <div className="text-sm text-gray-700 mb-1">
                    Trạng thái: <b>{drugData.status}</b>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Manufacturer:{" "}
                    <span className="font-mono text-xs">
                      {drugData.manufacturerAddress || "-"}
                    </span>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Distributor:{" "}
                    <span className="font-mono text-xs">
                      {drugData.distributorAddress || "-"}
                    </span>
                  </div>
                  <div className="text-sm text-gray-700 mb-1">
                    Pharmacy:{" "}
                    <span className="font-mono text-xs">
                      {drugData.pharmacyAddress || "-"}
                    </span>
                  </div>
                  {drugData.certificateUrl && (
                    <div className="text-sm text-blue-700 mb-2">
                      <a
                        href={drugData.certificateUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="underline"
                      >
                        Xem giấy chứng nhận
                      </a>
                    </div>
                  )}
                  {resolveMediaUrl(drugData.imageUrl) ? (
                    <img
                      src={resolveMediaUrl(drugData.imageUrl)}
                      alt="Ảnh thuốc"
                      className="max-w-xs rounded my-2 border"
                    />
                  ) : (
                    <div className="text-xs text-gray-500 my-2">
                      Không có ảnh hiển thị
                    </div>
                  )}
                </div>
                {/* Lịch sử vận chuyển */}
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
                                {m.actorAddress || "-"}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="text-center py-12 text-gray-500">
                <QrCode className="w-16 h-16 mx-auto mb-4 opacity-50" />
                <h3 className="text-lg font-medium mb-2">Quét mã để bắt đầu</h3>
                <p className="text-sm">
                  Quét QR trên hộp thuốc hoặc nhập Token ID để xác minh nguồn
                  gốc
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Info Section */}
      <div className="mt-12 grid md:grid-cols-3 gap-6">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <Shield className="w-12 h-12 text-blue-600 mx-auto mb-3" />
              <h3 className="font-semibold mb-2">Xác minh Blockchain</h3>
              <p className="text-sm text-gray-600">
                Mỗi lô thuốc được ghi nhận trên blockchain, đảm bảo tính minh
                bạch và không thể giả mạo
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <MapPin className="w-12 h-12 text-green-600 mx-auto mb-3" />
              <h3 className="font-semibold mb-2">Theo dõi hành trình</h3>
              <p className="text-sm text-gray-600">
                Xem đầy đủ hành trình từ sản xuất đến người tiêu dùng với dữ
                liệu cảm biến AIoT
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <CheckCircle className="w-12 h-12 text-purple-600 mx-auto mb-3" />
              <h3 className="font-semibold mb-2">Miễn phí sử dụng</h3>
              <p className="text-sm text-gray-600">
                Không cần tạo tài khoản hay kết nối ví. Tra cứu ngay lập tức và
                hoàn toàn miễn phí
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
