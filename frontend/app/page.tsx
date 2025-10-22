"use client";

export default function HomePage() {
  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">
          PharmaDNA - Truy xuất nguồn gốc y tế
        </h1>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold mb-4">Quản lý NFT</h2>
            <p className="text-gray-600">
              Theo dõi và quản lý các NFT thuốc trong hệ thống blockchain
            </p>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold mb-4">Truy xuất nguồn gốc</h2>
            <p className="text-gray-600">
              Xem lịch sử di chuyển và nguồn gốc của từng lô thuốc
            </p>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold mb-4">Báo cáo & Thống kê</h2>
            <p className="text-gray-600">
              Xem các báo cáo chi tiết về tình trạng thuốc
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
