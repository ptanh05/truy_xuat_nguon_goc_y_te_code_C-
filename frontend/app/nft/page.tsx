"use client";

import { useState, useEffect } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Package,
  Search,
  Plus,
  Eye,
  Edit,
  Trash2,
  Network,
  Activity,
} from "lucide-react";

interface NFT {
  id: number;
  productName: string;
  productCode: string;
  batchId: string;
  manufacturer: string;
  expiryDate: string;
  price: number;
  quantity: number;
  productType: string;
  createdDate: string;
  blockchainTransactionHash?: string;
  blockchainAddress?: string;
  pharmaNetworkId?: string;
  isActive: boolean;
}

interface PharmaNetworkInfo {
  networkName: string;
  chainId: number;
  rpcUrl: string;
  contractAddress: string;
  gasPrice: string;
  gasLimit: string;
  isConnected: boolean;
}

export default function NFTManagementPage() {
  const [nfts, setNfts] = useState<NFT[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [networkInfo, setNetworkInfo] = useState<PharmaNetworkInfo | null>(
    null
  );
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [newNFT, setNewNFT] = useState({
    productName: "",
    productCode: "",
    batchId: "",
    manufacturer: "",
    expiryDate: "",
    price: 0,
    quantity: 1,
    productType: "Medicine",
  });

  useEffect(() => {
    fetchNFTs();
    fetchNetworkInfo();
  }, []);

  const fetchNFTs = async () => {
    try {
      const response = await fetch("/api/pharmanetwork/nfts");
      if (response.ok) {
        const data = await response.json();
        setNfts(data.nfts || []);
      }
    } catch (error) {
      console.error("Error fetching NFTs:", error);
    } finally {
      setLoading(false);
    }
  };

  const fetchNetworkInfo = async () => {
    try {
      const response = await fetch("/api/pharmanetwork/info");
      if (response.ok) {
        const data = await response.json();
        setNetworkInfo(data);
      }
    } catch (error) {
      console.error("Error fetching network info:", error);
    }
  };

  const createNFT = async () => {
    try {
      const response = await fetch("/api/pharmanetwork/nft", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(newNFT),
      });

      if (response.ok) {
        const data = await response.json();
        if (data.success) {
          setShowCreateDialog(false);
          setNewNFT({
            productName: "",
            productCode: "",
            batchId: "",
            manufacturer: "",
            expiryDate: "",
            price: 0,
            quantity: 1,
            productType: "Medicine",
          });
          fetchNFTs();
        }
      }
    } catch (error) {
      console.error("Error creating NFT:", error);
    }
  };

  const filteredNFTs = nfts.filter(
    (nft) =>
      nft.productName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      nft.productCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
      nft.manufacturer.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              📦 Quản lý NFT - Pharma Network
            </h1>
            <p className="text-gray-600">
              Quản lý và theo dõi các NFT thuốc trên mạng PharmaDNA
            </p>
          </div>
          <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
            <DialogTrigger asChild>
              <Button className="bg-blue-600 hover:bg-blue-700">
                <Plus className="h-4 w-4 mr-2" />
                Thêm NFT mới
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl">
              <DialogHeader>
                <DialogTitle>Tạo NFT mới trên Pharma Network</DialogTitle>
                <DialogDescription>
                  Tạo NFT thuốc mới và đăng ký trên blockchain PharmaDNA
                </DialogDescription>
              </DialogHeader>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="productName">Tên thuốc</Label>
                  <Input
                    id="productName"
                    value={newNFT.productName}
                    onChange={(e) =>
                      setNewNFT({ ...newNFT, productName: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="productCode">Mã sản phẩm</Label>
                  <Input
                    id="productCode"
                    value={newNFT.productCode}
                    onChange={(e) =>
                      setNewNFT({ ...newNFT, productCode: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="batchId">Số lô</Label>
                  <Input
                    id="batchId"
                    value={newNFT.batchId}
                    onChange={(e) =>
                      setNewNFT({ ...newNFT, batchId: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="manufacturer">Nhà sản xuất</Label>
                  <Input
                    id="manufacturer"
                    value={newNFT.manufacturer}
                    onChange={(e) =>
                      setNewNFT({ ...newNFT, manufacturer: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="expiryDate">Ngày hết hạn</Label>
                  <Input
                    id="expiryDate"
                    type="date"
                    value={newNFT.expiryDate}
                    onChange={(e) =>
                      setNewNFT({ ...newNFT, expiryDate: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="price">Giá (USD)</Label>
                  <Input
                    id="price"
                    type="number"
                    value={newNFT.price}
                    onChange={(e) =>
                      setNewNFT({
                        ...newNFT,
                        price: parseFloat(e.target.value),
                      })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="quantity">Số lượng</Label>
                  <Input
                    id="quantity"
                    type="number"
                    value={newNFT.quantity}
                    onChange={(e) =>
                      setNewNFT({
                        ...newNFT,
                        quantity: parseInt(e.target.value),
                      })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="productType">Loại sản phẩm</Label>
                  <Select
                    value={newNFT.productType}
                    onValueChange={(value) =>
                      setNewNFT({ ...newNFT, productType: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Medicine">Thuốc</SelectItem>
                      <SelectItem value="Vaccine">Vắc xin</SelectItem>
                      <SelectItem value="Medical Device">
                        Thiết bị y tế
                      </SelectItem>
                      <SelectItem value="Supplement">
                        Thực phẩm chức năng
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="flex justify-end space-x-2 mt-4">
                <Button
                  variant="outline"
                  onClick={() => setShowCreateDialog(false)}
                >
                  Hủy
                </Button>
                <Button onClick={createNFT}>Tạo NFT</Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>

        {/* Network Status */}
        {networkInfo && (
          <Card className="mb-6 bg-white/80 backdrop-blur-sm">
            <CardHeader>
              <div className="flex items-center space-x-2">
                <Network className="h-5 w-5 text-blue-600" />
                <CardTitle className="text-lg">Pharma Network Status</CardTitle>
              </div>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Mạng</p>
                  <p className="font-medium">{networkInfo.networkName}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Chain ID</p>
                  <p className="font-medium">{networkInfo.chainId}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Trạng thái</p>
                  <Badge
                    variant={
                      networkInfo.isConnected ? "default" : "destructive"
                    }
                  >
                    {networkInfo.isConnected ? "Kết nối" : "Mất kết nối"}
                  </Badge>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Contract</p>
                  <p className="font-medium text-xs">
                    {networkInfo.contractAddress}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Search */}
        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Tìm kiếm theo tên thuốc, mã sản phẩm hoặc nhà sản xuất..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Tổng NFT</p>
                  <p className="text-2xl font-bold text-blue-600">
                    {nfts.length}
                  </p>
                </div>
                <Package className="h-8 w-8 text-blue-600" />
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Đang hoạt động</p>
                  <p className="text-2xl font-bold text-green-600">
                    {nfts.filter((nft) => nft.isActive).length}
                  </p>
                </div>
                <Badge
                  variant="outline"
                  className="text-green-600 border-green-600"
                >
                  Active
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Có trên blockchain</p>
                  <p className="text-2xl font-bold text-purple-600">
                    {nfts.filter((nft) => nft.blockchainTransactionHash).length}
                  </p>
                </div>
                <Activity className="h-8 w-8 text-purple-600" />
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Tổng giá trị</p>
                  <p className="text-2xl font-bold text-orange-600">
                    $
                    {nfts
                      .reduce((sum, nft) => sum + nft.price * nft.quantity, 0)
                      .toLocaleString()}
                  </p>
                </div>
                <Badge
                  variant="outline"
                  className="text-orange-600 border-orange-600"
                >
                  Value
                </Badge>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* NFT List */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[...Array(6)].map((_, i) => (
              <Card key={i} className="animate-pulse">
                <CardHeader>
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="h-3 bg-gray-200 rounded"></div>
                    <div className="h-3 bg-gray-200 rounded w-2/3"></div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredNFTs.map((nft) => (
              <Card
                key={nft.id}
                className="bg-white/80 backdrop-blur-sm hover:shadow-lg transition-shadow"
              >
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div>
                      <CardTitle className="text-lg">
                        {nft.productName}
                      </CardTitle>
                      <CardDescription>{nft.productCode}</CardDescription>
                    </div>
                    <div className="flex flex-col space-y-1">
                      <Badge
                        variant="outline"
                        className={
                          nft.isActive
                            ? "text-green-600 border-green-600"
                            : "text-red-600 border-red-600"
                        }
                      >
                        {nft.isActive ? "Active" : "Inactive"}
                      </Badge>
                      {nft.blockchainTransactionHash && (
                        <Badge
                          variant="outline"
                          className="text-blue-600 border-blue-600 text-xs"
                        >
                          On Chain
                        </Badge>
                      )}
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">
                        Nhà sản xuất:
                      </span>
                      <span className="text-sm font-medium">
                        {nft.manufacturer}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Lô:</span>
                      <span className="text-sm font-medium">{nft.batchId}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Số lượng:</span>
                      <span className="text-sm font-medium">
                        {nft.quantity}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Giá:</span>
                      <span className="text-sm font-medium">${nft.price}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Hết hạn:</span>
                      <span className="text-sm font-medium">
                        {new Date(nft.expiryDate).toLocaleDateString()}
                      </span>
                    </div>
                    {nft.blockchainTransactionHash && (
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-600">Tx Hash:</span>
                        <span className="text-xs font-mono text-blue-600">
                          {nft.blockchainTransactionHash.substring(0, 10)}...
                        </span>
                      </div>
                    )}
                  </div>
                  <div className="flex space-x-2 mt-4">
                    <Button size="sm" variant="outline" className="flex-1">
                      <Eye className="h-4 w-4 mr-1" />
                      Xem
                    </Button>
                    <Button size="sm" variant="outline" className="flex-1">
                      <Edit className="h-4 w-4 mr-1" />
                      Sửa
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      className="text-red-600 hover:text-red-700"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}

        {!loading && filteredNFTs.length === 0 && (
          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-8 text-center">
              <Package className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Không tìm thấy NFT nào
              </h3>
              <p className="text-gray-600">
                {searchTerm
                  ? "Thử thay đổi từ khóa tìm kiếm"
                  : "Hãy thêm NFT đầu tiên của bạn"}
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
