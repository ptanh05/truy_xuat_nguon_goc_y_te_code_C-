"use client";

import type React from "react";

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
import { Textarea } from "@/components/ui/textarea";
import {
  Upload,
  Package,
  CheckCircle,
  AlertCircle,
  AlertTriangle,
  ExternalLink,
  Database,
} from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useWallet } from "@/hooks/useWallet";
import RoleGuard from "@/components/RoleGuard";
import { ethers } from "ethers";
import pharmaNFTAbi from "@/lib/pharmaNFT-abi.json";
import { toast } from "sonner";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface UploadResult {
  success: boolean;
  ipfsHash: string;
  metadata: {
    drugName: string;
    batchNumber: string;
    manufacturingDate: string;
    expiryDate: string;
    description: string;
    gtin?: string;
    formulation?: string;
    manufacturerAddress: string;
    timestamp: string;
    files: string[];
    version: string;
  };
  filesUploaded: number;
  databaseId?: number;
  databaseError?: string;
  message: string;
}

function ManufacturerContent() {
  const { isConnected, account, isCorrectNetwork, switchToPharmaDNA } =
    useWallet();

  const [formData, setFormData] = useState({
    drugName: "",
    batchNumber: "",
    gtin: "",
    manufacturingDate: "",
    expiryDate: "",
    description: "",
    formulation: "",
  });
  const [drugImage, setDrugImage] = useState<File | null>(null);
  const [certificate, setCertificate] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadStatus, setUploadStatus] = useState<
    "idle" | "success" | "error"
  >("idle");
  const [uploadResult, setUploadResult] = useState<UploadResult | null>(null);
  const [mintTxHash, setMintTxHash] = useState<string | null>(null);
  const [userList, setUserList] = useState<any[]>([]);
  const [isManufacturer, setIsManufacturer] = useState<boolean>(true);
  const [contractRole, setContractRole] = useState<number | null>(null);
  const [roleCheckError, setRoleCheckError] = useState<string | null>(null);
  const [transferRequests, setTransferRequests] = useState<any[]>([]);
  const [isApproving, setIsApproving] = useState(false);

  // Lấy danh sách user từ backend
  useEffect(() => {
    if (!isConnected || !account) return;
    import("@/lib/api").then(({ api }) =>
      api.get("/admin").then((users) => {
        setUserList(users);
        const myUser = users.find(
          (u: any) => u.address.toLowerCase() === account.toLowerCase()
        );
        setIsManufacturer(myUser?.role === "MANUFACTURER");
      })
      .catch(() => setIsManufacturer(false))
    );
  }, [isConnected, account]);

  // Kiểm tra role thực tế trên contract
  useEffect(() => {
    const checkRoleOnChain = async () => {
      if (!isConnected || !account) return;
      try {
        const provider = new ethers.BrowserProvider(window.ethereum);
        if (!contractAddress) {
          setRoleCheckError("Contract address not configured");
          return;
        }
        const contract = new ethers.Contract(
          contractAddress!,
          pharmaNFTAbi.abi || pharmaNFTAbi,
          provider
        );
        const role = await contract.roles(account);
        setContractRole(Number(role));
        setRoleCheckError(null);
      } catch (err: any) {
        setContractRole(null);
        setRoleCheckError(
          "Không thể kiểm tra quyền trên contract: " + (err?.message || "")
        );
      }
    };
    checkRoleOnChain();
  }, [isConnected, account, uploadStatus]);

  // Lấy danh sách yêu cầu chuyển giao NFT
  const fetchTransferRequests = async () => {
    try {
      const { api } = await import("@/lib/api");
      const data = await api.get("/manufacturer/transfer-request");
      setTransferRequests(Array.isArray(data) ? data : []);
    } catch (error) {
      setTransferRequests([]);
    }
  };

  useEffect(() => {
    fetchTransferRequests();
  }, [uploadStatus, isApproving]);

  // Auto-refresh mỗi 10 giây
  useEffect(() => {
    const interval = setInterval(() => {
      if (isConnected && account) {
        // Refresh user list
        import("@/lib/api").then(({ api }) =>
          api.get("/admin").then((users) => {
            setUserList(users);
            const myUser = users.find(
              (u: any) => u.address.toLowerCase() === account.toLowerCase()
            );
            setIsManufacturer(myUser?.role === "MANUFACTURER");
          }).catch(() => setIsManufacturer(false))
        );
        // Refresh transfer requests
        fetchTransferRequests();
      }
    }, 10000); // Refresh mỗi 10 giây

    return () => clearInterval(interval);
  }, [isConnected, account]);

  useEffect(() => {
    if (isConnected && account && contractRole !== 1) {
      import("@/lib/api").then(({ api }) =>
        api.post("/admin/auto-assign-role", { address: account })
      );
    }
  }, [isConnected, account, contractRole]);

  const approveTransfer = async (
    requestId: number,
    nftId: number,
    distributorAddress: string
  ) => {
    setIsApproving(true);
    try {
      const { api } = await import("@/lib/api");
      const data = await api.put("/manufacturer/transfer-request", { requestId, nftId, distributorAddress });
      if (data.success) {
        toast.success("Chấp thuận thành công!");
        // Refresh transfer requests
        fetchTransferRequests();
      } else {
        toast.error(data.error || "Chấp thuận thất bại");
      }
    } finally {
      setIsApproving(false);
    }
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setDrugImage(e.target.files[0]);
    }
  };

  const handleCertificateUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setCertificate(e.target.files[0]);
    }
  };

  const uploadToIPFS = async () => {
    if (!isConnected) {
      toast.error("Vui lòng kết nối ví để tiếp tục");
      return;
    }
    if (!isCorrectNetwork) {
      toast.error("Vui lòng chuyển sang mạng PharmaDNAVN Chainlet");
      return;
    }
    if (!account) {
      toast.error("Không thể lấy địa chỉ ví");
      return;
    }

    setIsUploading(true);
    setUploadStatus("idle");
    setUploadResult(null);

    try {
      const form = new FormData();
      form.append("drugName", formData.drugName);
      form.append("batchNumber", formData.batchNumber);
      form.append("gtin", formData.gtin);
      form.append("manufacturingDate", formData.manufacturingDate);
      form.append("expiryDate", formData.expiryDate);
      form.append("description", formData.description);
      form.append("formulation", formData.formulation);
      form.append("manufacturerAddress", account); // Thêm địa chỉ ví
      if (drugImage) form.append("drugImage", drugImage);
      if (certificate) form.append("certificate", certificate);

      const { API_BASE_URL } = await import("@/lib/api");
      const res = await fetch(`${API_BASE_URL}/manufacturer/upload-ipfs`, { method: "POST", body: form });
      
      let data;
      const contentType = res.headers.get("content-type") || "";
      if (contentType.includes("application/json")) {
        try {
          data = await res.json();
        } catch (e) {
          const text = await res.text();
          throw new Error(`Server trả về dữ liệu không hợp lệ: ${text.substring(0, 200)}`);
        }
      } else {
        const text = await res.text();
        throw new Error(`Server trả về lỗi: ${res.status} ${res.statusText} - ${text.substring(0, 200)}`);
      }

      if (res.ok && data.success) {
        setUploadResult(data);
        setUploadStatus("success");
        // Refresh transfer requests sau khi upload
        fetchTransferRequests();
        
        // Database ID đã được lưu và hiển thị trong UI
      } else {
        setUploadStatus("error");
        const errorMsg = data.error || data.message || "Upload thất bại";
        toast.error(errorMsg);
        console.error("Upload error:", data);
      }
    } catch (error) {
      setUploadStatus("error");
      const errorMessage = error instanceof Error ? error.message : "Có lỗi xảy ra khi upload IPFS";
      toast.error(errorMessage);
      console.error("Upload error:", error);
    } finally {
      setIsUploading(false);
    }
  };

  const mintNFT = async () => {
    if (!isConnected) {
      toast.error("Vui lòng kết nối ví để tiếp tục");
      return;
    }

    if (!isCorrectNetwork) {
      toast.error("Vui lòng chuyển sang đúng mạng PharmaDNAVN Chainlet");
      return;
    }

    if (!uploadResult?.ipfsHash) {
      toast.error("Chưa có IPFS hash để mint NFT");
      return;
    }

    if (!contractAddress) {
      toast.error("Contract address chưa được cấu hình. Vui lòng liên hệ admin.");
      return;
    }

    setIsUploading(true);
    setUploadStatus("idle");
    let loadingToastId: string | number | undefined;
    try {
      const provider = new ethers.BrowserProvider(window.ethereum);
      const signer = await provider.getSigner();
      const contract = new ethers.Contract(
        contractAddress,
        pharmaNFTAbi.abi || pharmaNFTAbi,
        signer
      );
      
      loadingToastId = toast.loading("Đang mint NFT trên blockchain...");
      const tx = await contract.mintProductNFT(uploadResult.ipfsHash);
      const txHash = tx.hash;
      setMintTxHash(txHash);
      toast.dismiss(loadingToastId);
      toast.loading(`Transaction đã được gửi! Hash: ${txHash.slice(0, 10)}...`, { id: "tx-pending" });
      
      await tx.wait();
      toast.dismiss("tx-pending");
      setUploadStatus("success");
      toast.success("Mint NFT thành công! Form sẽ được reset để nhập lô mới.");

      // Refresh transfer requests sau khi mint
      fetchTransferRequests();

      // Reset form để nhập lô mới
      setTimeout(() => {
        resetForm();
      }, 2000); // Đợi 2 giây để người dùng thấy thông báo thành công
    } catch (error: any) {
      setUploadStatus("error");
      if (loadingToastId) {
        toast.dismiss(loadingToastId); // Dismiss specific loading toast if exists
      } else {
        toast.dismiss(); // Fallback dismiss
      }
      if (
        error?.message?.includes("Invalid role") ||
        error?.message?.includes("revert")
      ) {
        toast.error(
          "Ví của bạn chưa được cấp quyền Manufacturer trên contract. Hãy liên hệ admin để được cấp quyền."
        );
      } else {
        toast.error(error?.message || "Mint NFT thất bại");
      }
      console.error("Mint NFT error:", error);
    } finally {
      setIsUploading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      drugName: "",
      batchNumber: "",
      gtin: "",
      manufacturingDate: "",
      expiryDate: "",
      description: "",
      formulation: "",
    });
    setDrugImage(null);
    setCertificate(null);
    setUploadStatus("idle");
    setUploadResult(null);
    setMintTxHash(null);
  };

  const contractAddress = process.env.NEXT_PUBLIC_PHARMA_NFT_ADDRESS;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Tạo lô thuốc mới
        </h1>
        <p className="text-gray-600">
          Nhập thông tin lô thuốc và mint NFT trên blockchain
        </p>
      </div>

      {!isConnected && (
        <Alert variant="destructive" className="mb-6">
          <AlertTriangle className="h-4 w-4" />
          <AlertDescription>
            Vui lòng kết nối ví MetaMask để sử dụng chức năng này
          </AlertDescription>
        </Alert>
      )}

      {isConnected && !isCorrectNetwork && (
        <Alert className="mb-6 bg-yellow-50 text-yellow-800 border-yellow-200">
          <AlertTriangle className="h-4 w-4" />
          <AlertDescription className="flex items-center justify-between">
            <span>Vui lòng chuyển sang mạng PharmaDNAVN Chainlet</span>
          </AlertDescription>
        </Alert>
      )}

      <div className="grid lg:grid-cols-2 gap-8">
        {/* Form Section */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Package className="w-5 h-5 mr-2" />
              Thông tin lô thuốc
            </CardTitle>
            <CardDescription>
              Điền đầy đủ thông tin về lô thuốc cần tạo NFT
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label htmlFor="drugName">Tên thuốc *</Label>
              <Input
                id="drugName"
                name="drugName"
                value={formData.drugName}
                onChange={handleInputChange}
                placeholder="Ví dụ: Paracetamol 500mg"
                required
                disabled={uploadStatus === "success"}
              />
            </div>

            <div>
              <Label htmlFor="batchNumber">Số lô *</Label>
              <Input
                id="batchNumber"
                name="batchNumber"
                value={formData.batchNumber}
                onChange={handleInputChange}
                placeholder="Ví dụ: LOT2024001"
                required
                disabled={uploadStatus === "success"}
              />
            </div>

            <div>
              <Label htmlFor="gtin">GTIN / Mã sản phẩm</Label>
              <Input
                id="gtin"
                name="gtin"
                value={formData.gtin}
                onChange={handleInputChange}
                placeholder="Ví dụ: 8938505974195"
                disabled={uploadStatus === "success"}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="manufacturingDate">Ngày sản xuất *</Label>
                <Input
                  id="manufacturingDate"
                  name="manufacturingDate"
                  type="date"
                  value={formData.manufacturingDate}
                  onChange={handleInputChange}
                  required
                  disabled={uploadStatus === "success"}
                />
              </div>
              <div>
                <Label htmlFor="expiryDate">Hạn dùng *</Label>
                <Input
                  id="expiryDate"
                  name="expiryDate"
                  type="date"
                  value={formData.expiryDate}
                  onChange={handleInputChange}
                  required
                  disabled={uploadStatus === "success"}
                />
              </div>
            </div>

            <div>
              <Label htmlFor="description">Mô tả</Label>
              <Textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                placeholder="Thông tin bổ sung về lô thuốc..."
                rows={3}
                disabled={uploadStatus === "success"}
              />
            </div>

            <div>
              <Label htmlFor="formulation">Thành phần / Dạng bào chế</Label>
              <Textarea
                id="formulation"
                name="formulation"
                value={formData.formulation}
                onChange={handleInputChange}
                placeholder="Mỗi viên chứa..., dạng bào chế..."
                rows={4}
                disabled={uploadStatus === "success"}
              />
            </div>

            <div>
              <Label htmlFor="drugImage">Ảnh thuốc</Label>
              <Input
                id="drugImage"
                type="file"
                accept="image/*"
                onChange={handleImageUpload}
                disabled={uploadStatus === "success"}
              />
              {drugImage && (
                <p className="text-sm text-green-600 mt-1">
                  ✓ Đã chọn: {drugImage.name}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="certificate">Chứng chỉ (PDF)</Label>
              <Input
                id="certificate"
                type="file"
                accept=".pdf"
                onChange={handleCertificateUpload}
                disabled={uploadStatus === "success"}
              />
              {certificate && (
                <p className="text-sm text-green-600 mt-1">
                  ✓ Đã chọn: {certificate.name}
                </p>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Actions Section */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Upload className="w-5 h-5 mr-2" />
              Tạo NFT
            </CardTitle>
            <CardDescription>
              Upload metadata lên IPFS và lưu vào database
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {isConnected && (
              <Alert className="bg-blue-50 text-blue-800 border-blue-200">
                <CheckCircle className="h-4 w-4" />
                <AlertDescription>
                  Đã kết nối với ví: {account?.slice(0, 6)}...
                  {account?.slice(-4)}
                </AlertDescription>
              </Alert>
            )}

            {uploadStatus === "success" && uploadResult && (
              <Alert>
                <CheckCircle className="h-4 w-4" />
                <AlertDescription>
                  {uploadResult.message}
                  {uploadResult.filesUploaded > 0 && (
                    <span> ({uploadResult.filesUploaded} file đã upload)</span>
                  )}
                  {uploadResult.databaseId && (
                    <div className="flex items-center mt-1">
                      <Database className="w-4 h-4 mr-1" />
                      <span>Database ID: {uploadResult.databaseId}</span>
                    </div>
                  )}
                </AlertDescription>
              </Alert>
            )}

            {uploadResult?.databaseError && (
              <Alert className="bg-yellow-50 text-yellow-800 border-yellow-200">
                <AlertTriangle className="h-4 w-4" />
                <AlertDescription>
                  {uploadResult.databaseError}
                </AlertDescription>
              </Alert>
            )}

            {uploadStatus === "error" && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  Có lỗi xảy ra. Vui lòng thử lại.
                </AlertDescription>
              </Alert>
            )}

            <div className="space-y-3">
              {uploadStatus !== "success" ? (
                <>
                  {!isManufacturer && (
                    <Alert variant="destructive">
                      <AlertCircle className="h-4 w-4" />
                      <AlertDescription>
                        Ví của bạn chưa được cấp quyền <b>Manufacturer</b> trên
                        hệ thống. Hãy liên hệ admin để được cấp quyền.
                      </AlertDescription>
                    </Alert>
                  )}

                  <Button
                    onClick={uploadToIPFS}
                    disabled={
                      isUploading ||
                      !formData.drugName ||
                      !formData.batchNumber ||
                      !formData.manufacturingDate ||
                      !formData.expiryDate ||
                      !isConnected
                    }
                    className="w-full bg-transparent"
                    variant="outline"
                  >
                    {isUploading
                      ? "Đang upload..."
                      : "Upload lên IPFS & Database"}
                  </Button>
                </>
              ) : (
                <div className="space-y-2">
                  <Button
                    onClick={mintNFT}
                    disabled={isUploading || !uploadResult || !isConnected}
                    className="w-full"
                  >
                    {isUploading ? "Đang mint NFT..." : "Mint NFT"}
                  </Button>
                  <Button
                    onClick={resetForm}
                    variant="outline"
                    className="w-full bg-transparent"
                  >
                    Tạo lô thuốc mới
                  </Button>
                </div>
              )}
            </div>

            {uploadResult && (
              <div className="p-4 bg-gray-50 rounded-lg space-y-3">
                <div>
                  <h4 className="font-semibold mb-2">IPFS Metadata Hash:</h4>
                  <p className="text-sm font-mono break-all">
                    {uploadResult.ipfsHash}
                  </p>
                  <Button
                    variant="link"
                    className="p-0 h-auto mt-2"
                    onClick={() => {
                      const gateway = process.env.NEXT_PUBLIC_PINATA_GATEWAY || "https://gateway.pinata.cloud/ipfs/";
                      const gatewayUrl = gateway.endsWith("/") ? gateway : `${gateway}/`;
                      window.open(`${gatewayUrl}${uploadResult.ipfsHash}`, "_blank");
                    }}
                  >
                    <ExternalLink className="w-4 h-4 mr-1" />
                    Xem trên IPFS Gateway
                  </Button>
                </div>

                {uploadResult.databaseId && (
                  <div>
                    <h4 className="font-semibold mb-2">Database Info:</h4>
                    <p className="text-sm">
                      <span className="font-medium">ID:</span>{" "}
                      {uploadResult.databaseId}
                    </p>
                    <p className="text-sm">
                      <span className="font-medium">Status:</span> CREATED
                    </p>
                    <p className="text-sm">
                      <span className="font-medium">Manufacturer:</span>{" "}
                      {account?.slice(0, 6)}...{account?.slice(-4)}
                    </p>
                  </div>
                )}

                {mintTxHash && (
                  <div>
                    <h4 className="font-semibold mb-2">Transaction Hash (Mint NFT):</h4>
                    <p className="text-sm font-mono break-all">
                      {mintTxHash}
                    </p>
                    <Button
                      variant="link"
                      className="p-0 h-auto mt-2"
                      onClick={() => {
                        const explorerUrl = process.env.NEXT_PUBLIC_BLOCK_EXPLORER_URL || "https://pharmadnavn-2763717455037000-1.sagaexplorer.io";
                        const explorerBase = explorerUrl.endsWith("/") ? explorerUrl.slice(0, -1) : explorerUrl;
                        window.open(`${explorerBase}/tx/${mintTxHash}`, "_blank");
                      }}
                    >
                      <ExternalLink className="w-4 h-4 mr-1" />
                      Xem trên Block Explorer
                    </Button>
                  </div>
                )}

                {uploadResult.metadata.files.length > 0 && (
                  <div>
                    <h4 className="font-semibold mb-2">Files đã upload:</h4>
                    <ul className="text-sm space-y-1">
                      {uploadResult.metadata.files.map((fileHash, index) => (
                        <li key={index} className="flex items-center">
                          <span className="font-mono text-xs break-all">
                            {fileHash}
                          </span>
                          <Button
                            variant="link"
                            size="sm"
                            className="p-0 h-auto ml-2"
                            onClick={() => {
                              const gateway = process.env.NEXT_PUBLIC_PINATA_GATEWAY || "https://gateway.pinata.cloud/ipfs/";
                              const gatewayUrl = gateway.endsWith("/") ? gateway : `${gateway}/`;
                              // Remove ipfs/ prefix if fileHash already has it
                              const hash = fileHash.replace("ipfs/", "");
                              window.open(`${gatewayUrl}${hash}`, "_blank");
                            }}
                          >
                            <ExternalLink className="w-3 h-3" />
                          </Button>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            )}

            <div className="text-sm text-gray-500 space-y-2">
              <p>
                <strong>Bước 1:</strong> Upload metadata và files lên IPFS
              </p>
              <p>
                <strong>Bước 2:</strong> Lưu thông tin vào database
              </p>
              <p>
                <strong>Bước 3:</strong> Mint NFT với metadata IPFS
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Thêm bảng danh sách yêu cầu chuyển giao NFT */}
      <div className="mt-12">
        <h2 className="text-xl font-bold mb-4">Yêu cầu nhận lô chờ duyệt</h2>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>ID</TableHead>
              <TableHead>Lô thuốc (NFT)</TableHead>
              <TableHead>Ví distributor</TableHead>
              <TableHead>Trạng thái</TableHead>
              <TableHead>Thao tác</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {transferRequests.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="text-center text-gray-500">
                  Không có yêu cầu nào
                </TableCell>
              </TableRow>
            ) : (
              transferRequests.map((req) => (
                <TableRow key={req.id}>
                  <TableCell>{req.id}</TableCell>
                  <TableCell>#{req.nftId ?? req.nft_id}</TableCell>
                  <TableCell className="font-mono text-xs">
                    {req.distributorAddress ?? req.distributor_address}
                  </TableCell>
                  <TableCell>
                    {req.status === "approved" ? (
                      <span className="text-green-600 font-semibold">
                        Đã được chấp thuận
                      </span>
                    ) : (
                      req.status
                    )}
                  </TableCell>
                  <TableCell>
                    {req.status === "pending" && (
                      <Button
                        size="sm"
                        onClick={() =>
                          approveTransfer(
                            req.id,
                            req.nftId ?? req.nft_id,
                            (req.distributorAddress ?? req.distributor_address) ?? ""
                          )
                        }
                        disabled={isApproving}
                      >
                        Chấp thuận
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

export default function ManufacturerPage() {
  return (
    <RoleGuard requiredRoles={["MANUFACTURER"]}>
      <ManufacturerContent />
    </RoleGuard>
  );
}
