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
import {
  Activity,
  Package,
  Users,
  TrendingUp,
  Shield,
  FileText,
} from "lucide-react";

interface PharmaNetworkInfo {
  networkName: string;
  chainId: number;
  rpcUrl: string;
  contractAddress: string;
  gasPrice: string;
  gasLimit: string;
  isConnected: boolean;
}

export default function HomePage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [networkInfo, setNetworkInfo] = useState<PharmaNetworkInfo | null>(
    null
  );
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardStats();
    fetchNetworkInfo();
  }, []);

  const fetchDashboardStats = async () => {
    try {
      const response = await fetch("/api/analytics/dashboard");
      if (response.ok) {
        const data = await response.json();
        setStats(data);
      }
    } catch (error) {
      console.error("Error fetching dashboard stats:", error);
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

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            🏥 PharmaDNA
          </h1>
          <p className="text-xl text-gray-600 mb-2">
            Hệ thống truy xuất nguồn gốc y tế
          </p>
          <p className="text-gray-500 mb-4">
            Sử dụng blockchain và NFT để theo dõi chuỗi cung ứng dược phẩm
          </p>
          {networkInfo && (
            <div className="flex justify-center items-center space-x-4">
              <Badge
                variant="outline"
                className="text-blue-600 border-blue-600"
              >
                🌐 {networkInfo.networkName}
              </Badge>
              <Badge
                variant="outline"
                className="text-green-600 border-green-600"
              >
                Chain ID: {networkInfo.chainId}
              </Badge>
              <Badge
                variant={networkInfo.isConnected ? "default" : "destructive"}
              >
                {networkInfo.isConnected ? "🟢 Kết nối" : "🔴 Mất kết nối"}
              </Badge>
            </div>
          )}
        </div>

        {/* Stats Cards */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {[...Array(4)].map((_, i) => (
              <Card key={i} className="animate-pulse">
                <CardHeader>
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                </CardHeader>
                <CardContent>
                  <div className="h-8 bg-gray-200 rounded w-1/2"></div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <Card className="bg-white/80 backdrop-blur-sm border-blue-200">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-gray-600">
                  Tổng NFT
                </CardTitle>
                <Package className="h-4 w-4 text-blue-600" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-blue-600">
                  {stats?.totalNFTs || 0}
                </div>
                <p className="text-xs text-gray-500">Thuốc được đăng ký</p>
              </CardContent>
            </Card>

            <Card className="bg-white/80 backdrop-blur-sm border-green-200">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-gray-600">
                  Chuyển giao
                </CardTitle>
                <Activity className="h-4 w-4 text-green-600" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-green-600">
                  {stats?.totalTransfers || 0}
                </div>
                <p className="text-xs text-gray-500">Giao dịch thành công</p>
              </CardContent>
            </Card>

            <Card className="bg-white/80 backdrop-blur-sm border-purple-200">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-gray-600">
                  Người dùng
                </CardTitle>
                <Users className="h-4 w-4 text-purple-600" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-purple-600">
                  {stats?.activeUsers || 0}
                </div>
                <p className="text-xs text-gray-500">Hoạt động tích cực</p>
              </CardContent>
            </Card>

            <Card className="bg-white/80 backdrop-blur-sm border-orange-200">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-gray-600">
                  Giá trị
                </CardTitle>
                <TrendingUp className="h-4 w-4 text-orange-600" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-orange-600">
                  ${stats?.totalValue?.toLocaleString() || "0"}
                </div>
                <p className="text-xs text-gray-500">Tổng giá trị</p>
              </CardContent>
            </Card>
          </div>
        )}

        {/* Feature Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <Card className="bg-white/80 backdrop-blur-sm hover:shadow-lg transition-shadow">
            <CardHeader>
              <div className="flex items-center space-x-2">
                <Package className="h-6 w-6 text-blue-600" />
                <CardTitle className="text-xl">Quản lý NFT</CardTitle>
              </div>
              <CardDescription>
                Theo dõi và quản lý các NFT thuốc trong hệ thống blockchain
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button className="w-full" variant="outline">
                Xem chi tiết
              </Button>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm hover:shadow-lg transition-shadow">
            <CardHeader>
              <div className="flex items-center space-x-2">
                <Shield className="h-6 w-6 text-green-600" />
                <CardTitle className="text-xl">Truy xuất nguồn gốc</CardTitle>
              </div>
              <CardDescription>
                Xem lịch sử di chuyển và nguồn gốc của từng lô thuốc
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button className="w-full" variant="outline">
                Truy xuất
              </Button>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm hover:shadow-lg transition-shadow">
            <CardHeader>
              <div className="flex items-center space-x-2">
                <FileText className="h-6 w-6 text-purple-600" />
                <CardTitle className="text-xl">Báo cáo & Thống kê</CardTitle>
              </div>
              <CardDescription>
                Xem các báo cáo chi tiết về tình trạng thuốc
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button className="w-full" variant="outline">
                Xem báo cáo
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Status Badge */}
        <div className="mt-8 text-center">
          <Badge variant="outline" className="text-green-600 border-green-600">
            ✅ Hệ thống hoạt động bình thường
          </Badge>
        </div>
      </div>
    </div>
  );
}
