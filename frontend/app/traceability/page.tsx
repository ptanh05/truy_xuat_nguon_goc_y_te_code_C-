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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Search,
  Shield,
  MapPin,
  Calendar,
  Package,
  ArrowRight,
  CheckCircle,
  AlertCircle,
} from "lucide-react";

interface TraceabilityRecord {
  id: number;
  nftId: number;
  productName: string;
  productCode: string;
  batchId: string;
  manufacturer: string;
  location: string;
  timestamp: string;
  action: string;
  status: string;
  blockchainHash?: string;
}

interface TraceabilityResult {
  productName: string;
  productCode: string;
  batchId: string;
  manufacturer: string;
  totalRecords: number;
  records: TraceabilityRecord[];
  blockchainVerified: boolean;
}

export default function TraceabilityPage() {
  const [searchType, setSearchType] = useState("productCode");
  const [searchValue, setSearchValue] = useState("");
  const [result, setResult] = useState<TraceabilityResult | null>(null);
  const [loading, setLoading] = useState(false);

  const searchTraceability = async () => {
    if (!searchValue.trim()) return;

    setLoading(true);
    try {
      const response = await fetch(
        `/api/traceability/search?type=${searchType}&value=${encodeURIComponent(
          searchValue
        )}`
      );
      if (response.ok) {
        const data = await response.json();
        setResult(data);
      }
    } catch (error) {
      console.error("Error searching traceability:", error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "verified":
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case "pending":
        return <AlertCircle className="h-4 w-4 text-yellow-600" />;
      default:
        return <AlertCircle className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "verified":
        return (
          <Badge variant="outline" className="text-green-600 border-green-600">
            Đã xác minh
          </Badge>
        );
      case "pending":
        return (
          <Badge
            variant="outline"
            className="text-yellow-600 border-yellow-600"
          >
            Đang chờ
          </Badge>
        );
      default:
        return (
          <Badge variant="outline" className="text-gray-600 border-gray-600">
            Không xác định
          </Badge>
        );
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-blue-100">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            🔍 Truy xuất nguồn gốc
          </h1>
          <p className="text-gray-600">
            Theo dõi lịch sử di chuyển và nguồn gốc của thuốc trên Pharma
            Network
          </p>
        </div>

        {/* Search Section */}
        <Card className="mb-8 bg-white/80 backdrop-blur-sm">
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Search className="h-5 w-5 text-blue-600" />
              <span>Tìm kiếm thông tin truy xuất</span>
            </CardTitle>
            <CardDescription>
              Nhập thông tin sản phẩm để xem lịch sử di chuyển
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <Label htmlFor="searchType">Loại tìm kiếm</Label>
                <Select value={searchType} onValueChange={setSearchType}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="productCode">Mã sản phẩm</SelectItem>
                    <SelectItem value="batchId">Số lô</SelectItem>
                    <SelectItem value="nftId">NFT ID</SelectItem>
                    <SelectItem value="blockchainHash">
                      Blockchain Hash
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label htmlFor="searchValue">Giá trị tìm kiếm</Label>
                <Input
                  id="searchValue"
                  placeholder="Nhập thông tin cần tìm..."
                  value={searchValue}
                  onChange={(e) => setSearchValue(e.target.value)}
                  onKeyPress={(e) => e.key === "Enter" && searchTraceability()}
                />
              </div>
              <div className="flex items-end">
                <Button
                  onClick={searchTraceability}
                  disabled={loading || !searchValue.trim()}
                  className="w-full"
                >
                  {loading ? "Đang tìm..." : "Tìm kiếm"}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Results */}
        {result && (
          <div className="space-y-6">
            {/* Product Summary */}
            <Card className="bg-white/80 backdrop-blur-sm">
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <span>Thông tin sản phẩm</span>
                  <Badge
                    variant={
                      result.blockchainVerified ? "default" : "destructive"
                    }
                  >
                    {result.blockchainVerified
                      ? "✅ Blockchain Verified"
                      : "❌ Not Verified"}
                  </Badge>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div>
                    <p className="text-sm text-gray-600">Tên sản phẩm</p>
                    <p className="font-medium">{result.productName}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Mã sản phẩm</p>
                    <p className="font-medium">{result.productCode}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Số lô</p>
                    <p className="font-medium">{result.batchId}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Nhà sản xuất</p>
                    <p className="font-medium">{result.manufacturer}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Traceability Timeline */}
            <Card className="bg-white/80 backdrop-blur-sm">
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Shield className="h-5 w-5 text-green-600" />
                  <span>Lịch sử truy xuất ({result.totalRecords} bản ghi)</span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {result.records.map((record, index) => (
                    <div
                      key={record.id}
                      className="flex items-start space-x-4 p-4 border rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      <div className="flex-shrink-0">
                        {getStatusIcon(record.status)}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center justify-between mb-2">
                          <h4 className="font-medium text-gray-900">
                            {record.action}
                          </h4>
                          <div className="flex items-center space-x-2">
                            {getStatusBadge(record.status)}
                            <span className="text-xs text-gray-500">
                              {new Date(record.timestamp).toLocaleString()}
                            </span>
                          </div>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-2 text-sm text-gray-600">
                          <div className="flex items-center space-x-1">
                            <MapPin className="h-3 w-3" />
                            <span>{record.location}</span>
                          </div>
                          <div className="flex items-center space-x-1">
                            <Package className="h-3 w-3" />
                            <span>NFT #{record.nftId}</span>
                          </div>
                          {record.blockchainHash && (
                            <div className="flex items-center space-x-1">
                              <Shield className="h-3 w-3" />
                              <span className="font-mono text-xs">
                                {record.blockchainHash.substring(0, 10)}...
                              </span>
                            </div>
                          )}
                        </div>
                      </div>
                      {index < result.records.length - 1 && (
                        <div className="absolute left-8 top-16 w-px h-8 bg-gray-300"></div>
                      )}
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        )}

        {/* No Results */}
        {!result && !loading && (
          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-8 text-center">
              <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Tìm kiếm thông tin truy xuất
              </h3>
              <p className="text-gray-600">
                Nhập mã sản phẩm, số lô hoặc NFT ID để xem lịch sử di chuyển
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
