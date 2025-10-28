# PharmaDNA Server (.NET)

Backend API server cho ứng dụng truy xuất nguồn gốc thuốc sử dụng .NET 9.0.

## Cấu trúc

```
PharmaDNAServer/
├── Controllers/        # API Controllers
├── Data/              # Database Context
├── Models/            # Entity Models
└── Program.cs         # Application entry point
```

## Yêu cầu

- .NET 9.0 SDK
- PostgreSQL database

## Cài đặt

1. Cài đặt .NET 9.0 SDK từ https://dotnet.microsoft.com/download

2. Cài đặt PostgreSQL và tạo database:
```bash
createdb pharmadna
```

3. Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pharmadna;Username=postgres;Password=postgres;Port=5432"
  }
}
```

4. Tạo migrations và cập nhật database:
```bash
cd PharmaDNAServer
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Chạy Server

```bash
cd PharmaDNAServer
dotnet run
```

Server sẽ chạy tại `https://localhost:5001` hoặc `http://localhost:5000`

## API Endpoints

### Admin
- `GET /api/admin` - Lấy danh sách users
- `POST /api/admin` - Cấp quyền cho user
- `DELETE /api/admin` - Xóa user

### Manufacturer
- `GET /api/manufacturer` - Lấy danh sách NFTs
- `POST /api/manufacturer` - Tạo NFT mới
- `PUT /api/manufacturer` - Cập nhật NFT
- `DELETE /api/manufacturer` - Xóa NFT
- `GET /api/manufacturer/transfer-request` - Lấy danh sách transfer requests
- `POST /api/manufacturer/transfer-request` - Tạo transfer request
- `PUT /api/manufacturer/transfer-request` - Duyệt transfer request
- `GET /api/manufacturer/milestone` - Lấy milestones
- `POST /api/manufacturer/milestone` - Tạo milestone

### Distributor
- `GET /api/distributor` - Lấy NFTs đang vận chuyển
- `GET /api/distributor/roles` - Lấy danh sách distributors
- `PUT /api/distributor` - Cập nhật NFT
- `GET /api/distributor/transfer-to-pharmacy` - Lấy transfer requests
- `POST /api/distributor/transfer-to-pharmacy` - Tạo transfer request sang pharmacy
- `PUT /api/distributor/transfer-to-pharmacy` - Cập nhật transfer request
- `DELETE /api/distributor/transfer-to-pharmacy` - Hủy transfer request

### Pharmacy
- `GET /api/pharmacy` - Lấy NFTs trong pharmacy
- `PUT /api/pharmacy` - Cập nhật NFT

## CORS

CORS được cấu hình để cho phép client Next.js ở `http://localhost:3000` kết nối.

