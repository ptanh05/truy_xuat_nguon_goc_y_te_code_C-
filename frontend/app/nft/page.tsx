"use client";

import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Package, Search, Plus, Eye, Edit, Trash2 } from 'lucide-react';

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
}

export default function NFTManagementPage() {
  const [nfts, setNfts] = useState<NFT[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchNFTs();
  }, []);

  const fetchNFTs = async () => {
    try {
      const response = await fetch('/api/nft');
      if (response.ok) {
        const data = await response.json();
        setNfts(data);
      }
    } catch (error) {
      console.error('Error fetching NFTs:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredNFTs = nfts.filter(nft =>
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
              📦 Quản lý NFT
            </h1>
            <p className="text-gray-600">
              Quản lý và theo dõi các NFT thuốc trong hệ thống
            </p>
          </div>
          <Button className="bg-blue-600 hover:bg-blue-700">
            <Plus className="h-4 w-4 mr-2" />
            Thêm NFT mới
          </Button>
        </div>

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
                  <p className="text-2xl font-bold text-blue-600">{nfts.length}</p>
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
                    {nfts.filter(nft => new Date(nft.expiryDate) > new Date()).length}
                  </p>
                </div>
                <Badge variant="outline" className="text-green-600 border-green-600">
                  Active
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Sắp hết hạn</p>
                  <p className="text-2xl font-bold text-orange-600">
                    {nfts.filter(nft => {
                      const expiry = new Date(nft.expiryDate);
                      const now = new Date();
                      const diffDays = (expiry.getTime() - now.getTime()) / (1000 * 3600 * 24);
                      return diffDays <= 30 && diffDays > 0;
                    }).length}
                  </p>
                </div>
                <Badge variant="outline" className="text-orange-600 border-orange-600">
                  Warning
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card className="bg-white/80 backdrop-blur-sm">
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Tổng giá trị</p>
                  <p className="text-2xl font-bold text-purple-600">
                    ${nfts.reduce((sum, nft) => sum + (nft.price * nft.quantity), 0).toLocaleString()}
                  </p>
                </div>
                <Badge variant="outline" className="text-purple-600 border-purple-600">
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
              <Card key={nft.id} className="bg-white/80 backdrop-blur-sm hover:shadow-lg transition-shadow">
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div>
                      <CardTitle className="text-lg">{nft.productName}</CardTitle>
                      <CardDescription>{nft.productCode}</CardDescription>
                    </div>
                    <Badge 
                      variant="outline" 
                      className={
                        new Date(nft.expiryDate) > new Date() 
                          ? "text-green-600 border-green-600" 
                          : "text-red-600 border-red-600"
                      }
                    >
                      {new Date(nft.expiryDate) > new Date() ? 'Active' : 'Expired'}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Nhà sản xuất:</span>
                      <span className="text-sm font-medium">{nft.manufacturer}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Lô:</span>
                      <span className="text-sm font-medium">{nft.batchId}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Số lượng:</span>
                      <span className="text-sm font-medium">{nft.quantity}</span>
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
                    <Button size="sm" variant="outline" className="text-red-600 hover:text-red-700">
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
                {searchTerm ? 'Thử thay đổi từ khóa tìm kiếm' : 'Hãy thêm NFT đầu tiên của bạn'}
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
