# PharmaDNA - Truy xuất nguồn gốc thuốc bằng Blockchain & AIoT

Dự án được tách thành 2 phần riêng biệt: **Client (Next.js)** và **Server (.NET)**

## 📁 Cấu trúc dự án

```
truy_xuat_nguon_goc_y_te_code_C-/
├── client/              # Next.js Frontend
├── server/              # .NET Backend API
│   └── PharmaDNAServer/
├── components/          # React components (sử dụng bởi client)
├── hooks/               # React hooks (sử dụng bởi client)
├── lib/                 # Utilities và config
└── saga-contract/       # Smart contracts
```

## 🚀 Quick Start

### 1. Khởi chạy Server (.NET)

```bash
cd server/PharmaDNAServer
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Server sẽ chạy tại: `http://localhost:5000`

### 2. Khởi chạy Client (Next.js)

```bash
cd client
npm install
npm run dev
```

Client sẽ chạy tại: `http://localhost:3000`

## 🔧 Cấu hình môi trường

### Server (.NET)
Tạo file `server/PharmaDNAServer/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pharmadna;Username=postgres;Password=postgres;Port=5432"
  }
}
```

### Client (Next.js)
Tạo file `client/.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_PHARMA_NFT_ADDRESS=0x...
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
OWNER_PRIVATE_KEY=your_private_key
PINATA_JWT=your_pinata_jwt
DATABASE_URL=postgresql://user:password@localhost:5432/pharmadna
```

## 📡 API Endpoints

### Admin
- `GET /api/admin` - Lấy danh sách users
- `POST /api/admin` - Cấp quyền cho user
- `DELETE /api/admin` - Xóa user

### Manufacturer
- `GET /api/manufacturer` - Lấy danh sách NFTs
- `POST /api/manufacturer` - Tạo NFT mới
- `PUT /api/manufacturer` - Cập nhật NFT
- `DELETE /api/manufacturer` - Xóa NFT

### Distributor
- `GET /api/distributor` - Lấy NFTs đang vận chuyển
- `PUT /api/distributor` - Cập nhật NFT
- `POST /api/distributor/transfer-to-pharmacy` - Tạo transfer request

### Pharmacy
- `GET /api/pharmacy` - Lấy NFTs trong pharmacy
- `PUT /api/pharmacy` - Cập nhật NFT

Xem chi tiết tại [server/README.md](./server/README.md)

## 🎯 Features

- **Manufacturer**: Tạo NFT cho từng lô thuốc, upload metadata lên IPFS
- **Distributor**: Quản lý vận chuyển, upload dữ liệu cảm biến AIoT
- **Pharmacy**: Quét QR, xác minh và xác nhận nhập kho
- **Consumer**: Tra cứu nguồn gốc thuốc không cần kết nối ví
- **Admin**: Quản lý hệ thống, cấp quyền vai trò

## 🛠️ Tech Stack

### Frontend
- Next.js 14
- React 18
- TypeScript
- TailwindCSS
- Radix UI

### Backend
- .NET 9.0
- Entity Framework Core
- PostgreSQL
- Swagger/OpenAPI

### Blockchain
- Ethers.js
- Smart Contracts (Solidity)
- PharmaDNA Chainlet (Saga)

## 📝 Migration từ cấu trúc cũ

Dự án đã được tái cấu trúc từ Next.js API Routes sang kiến trúc tách biệt Client-Server:

**Trước:**
- Tất cả code ở trong root folder
- API routes trong `app/api/`
- Client và Server cùng một process

**Sau:**
- Client trong folder `client/` (Next.js Frontend)
- Server trong folder `server/PharmaDNAServer/` (.NET Backend)
- API endpoints được chuyển thành .NET Controllers
- CORS được cấu hình để cho phép client kết nối

## 🔐 Security Notes

- Không commit private keys vào git
- Sử dụng biến môi trường cho các thông tin nhạy cảm
- CORS chỉ cho phép từ localhost:3000 và localhost:3001

