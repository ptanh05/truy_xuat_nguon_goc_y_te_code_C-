# PharmaDNA - Hệ Thống Truy Xuất Nguồn Gốc Thuốc

Dự án ứng dụng blockchain để truy xuất nguồn gốc thuốc, sử dụng NFT để theo dõi quá trình sản xuất, phân phối và bán lẻ.

## 📋 Tổng Quan Dự Án

### Công Nghệ Sử Dụng

- **Frontend:** Next.js 14, React 18, TypeScript, Tailwind CSS
- **Backend:** ASP.NET Core 9.0, C#
- **Database:** PostgreSQL (Neon.tech)
- **Blockchain:** Ethereum/Saga Network
- **IPFS:** Pinata
- **Smart Contracts:** Solidity (Hardhat)

### Cấu Trúc Dự Án

```
truy_xuat_nguon_goc_y_te_code_C-/
├── client/                 # Frontend (Next.js)
├── server/                 # Backend (ASP.NET Core)
│   └── PharmaDNAServer/
├── saga-contract/          # Smart Contracts (Hardhat)
└── database/               # SQL scripts
```

---

## 🚀 Cài Đặt và Chạy Dự Án

### Yêu Cầu Hệ Thống

- **Node.js:** >= 18.x
- **.NET SDK:** 9.0
- **PostgreSQL:** Neon.tech (cloud) hoặc local
- **Git**

### Bước 1: Clone Repository

```bash
git clone <repository-url>
cd truy_xuat_nguon_goc_y_te_code_C-
```

### Bước 2: Cấu Hình Environment Variables

#### Frontend (client/.env)

Tạo file `.env` trong thư mục `client/`:

```env
# Database
DATABASE_URL=postgresql://username:password@host.neon.tech/database?sslmode=require

# Pinata (IPFS)
PINATA_API_KEY=your-pinata-api-key
PINATA_SECRET_API_KEY=your-pinata-secret-key
PINATA_JWT=your-pinata-jwt-token
PINATA_GATEWAY=your-pinata-gateway-url

# Blockchain
PHARMA_NFT_ADDRESS=0xYourContractAddress
NEXT_PUBLIC_PHARMA_NFT_ADDRESS=0xYourContractAddress
OWNER_PRIVATE_KEY=your-private-key
```

#### Backend (server/PharmaDNAServer/.env)

Tạo file `.env` trong thư mục `server/PharmaDNAServer/`:

```env
# Database (Neon.tech PostgreSQL)
DATABASE_URL=postgresql://username:password@host.neon.tech/database?sslmode=require

# Hoặc dùng format connection string:
# POSTGRES_CONNECTION=Host=host.neon.tech;Database=database;Username=username;Password=password;SSL Mode=Require

# Blockchain
PHARMA_NFT_ADDRESS=0xYourContractAddress
OWNER_PRIVATE_KEY=your-private-key
PHARMADNA_RPC=your-rpc-url (optional)

# Pinata (IPFS) - Optional, nếu server cần truy cập IPFS
PINATA_JWT=your-pinata-jwt-token
PINATA_GATEWAY=your-pinata-gateway-url
```

**Lưu ý:** File `.env` đã được bảo vệ bởi `.gitignore`, không bị commit lên Git.

### Bước 3: Cài Đặt Backend

```powershell
# Di chuyển vào thư mục server
cd server/PharmaDNAServer

# Cài đặt packages
dotnet restore

# Cài đặt EF Core tools (nếu chưa có)
dotnet tool install --global dotnet-ef --version 9.0.0

# Tạo và áp dụng migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Chạy server
dotnet run
```

Server sẽ chạy tại: **http://localhost:5196**

**Swagger UI:** http://localhost:5196/swagger

### Bước 4: Cài Đặt Frontend

```powershell
# Di chuyển vào thư mục client
cd client

# Cài đặt dependencies
npm install

# Chạy development server
npm run dev
```

Client sẽ chạy tại: **http://localhost:3000**

---

## 📁 Cấu Trúc Chi Tiết

### Backend (server/PharmaDNAServer/)

```
PharmaDNAServer/
├── Controllers/           # API Controllers
│   ├── AdminController.cs
│   ├── ManufacturerController.cs
│   ├── DistributorController.cs
│   ├── PharmacyController.cs
│   └── IPFSController.cs
├── Models/                # Entity Models
│   ├── User.cs
│   ├── NFT.cs
│   ├── TransferRequest.cs
│   ├── Milestone.cs
│   └── ContractOptions.cs
├── Data/                  # Database Context
│   └── ApplicationDbContext.cs
├── Services/              # Business Services
│   └── BlockchainService.cs
├── Migrations/            # Database Migrations
├── Program.cs             # Application entry point
├── appsettings.json       # Configuration (có secrets - đã ignore)
└── .env                   # Environment variables
```

### Frontend (client/)

```
client/
├── app/                   # Next.js App Router
│   ├── page.tsx          # Home page
│   ├── admin/            # Admin dashboard
│   ├── manufacturer/     # Manufacturer interface
│   ├── distributor/      # Distributor interface
│   ├── pharmacy/         # Pharmacy interface
│   └── lookup/           # Lookup page
├── components/           # React Components
│   ├── ui/              # shadcn/ui components
│   ├── AdminGuard.tsx
│   ├── RoleGuard.tsx
│   ├── QRScanner.tsx
│   └── ...
├── hooks/               # Custom Hooks
│   ├── useAdminAuth.ts
│   ├── useRoleAuth.ts
│   └── useWallet.ts
├── lib/                 # Utilities
│   ├── api.ts          # API client
│   ├── pinata.ts       # IPFS integration
│   └── utils.ts
└── .env                 # Environment variables
```

### Smart Contracts (saga-contract/)

```
saga-contract/
├── contracts/          # Solidity contracts
│   ├── PharmaDNA.sol
│   └── PharmaNFT.sol
├── scripts/            # Deployment scripts
└── test/              # Contract tests
```

---

## 🗄️ Database Schema

### Bảng `NguoiDung` (Users)
- `Id` (Primary Key)
- `Address` (Wallet address)
- `Role` (Admin, Manufacturer, Distributor, Pharmacy)
- `AssignedAt` (DateTime)

### Bảng `SanPhamNFT` (NFTs)
- `Id` (Primary Key)
- `Name`
- `BatchNumber`
- `ManufactureDate`
- `ExpiryDate`
- `Description`
- `ImageUrl`
- `CertificateUrl`
- `Status`
- `IpfsHash`
- `ManufacturerAddress`
- `DistributorAddress`
- `PharmacyAddress`
- `CreatedAt`

### Bảng `YeuCauChuyen` (TransferRequests)
- `Id` (Primary Key)
- `NftId` (Foreign Key)
- `DistributorAddress`
- `PharmacyAddress`
- `TransferNote`
- `Status` (pending, approved, rejected)
- `CreatedAt`
- `UpdatedAt`

### Bảng `MocDanhDau` (Milestones)
- `Id` (Primary Key)
- `NftId` (Foreign Key)
- `Type`
- `Description`
- `Location`
- `Timestamp`
- `ActorAddress`

---

## 🔌 API Endpoints

### Admin
- `GET /api/admin` - Lấy danh sách users
- `GET /api/admin/config` - Kiểm tra cấu hình
- `POST /api/admin` - Cấp quyền cho user
- `DELETE /api/admin` - Xóa user

### Manufacturer
- `GET /api/manufacturer` - Lấy danh sách NFTs
- `POST /api/manufacturer` - Tạo NFT mới
- `PUT /api/manufacturer` - Cập nhật NFT
- `DELETE /api/manufacturer` - Xóa NFT
- `GET /api/manufacturer/transfer-request` - Lấy transfer requests
- `POST /api/manufacturer/transfer-request` - Tạo transfer request
- `PUT /api/manufacturer/transfer-request` - Duyệt transfer request
- `GET /api/manufacturer/milestone` - Lấy milestones
- `POST /api/manufacturer/milestone` - Tạo milestone
- `POST /api/manufacturer/upload-ipfs` - Upload lên IPFS

### Distributor
- `GET /api/distributor` - Lấy NFTs đang vận chuyển
- `GET /api/distributor/roles` - Lấy danh sách distributors
- `PUT /api/distributor` - Cập nhật NFT
- `GET /api/distributor/transfer-to-pharmacy` - Lấy transfer requests
- `POST /api/distributor/transfer-to-pharmacy` - Tạo transfer request
- `PUT /api/distributor/transfer-to-pharmacy` - Cập nhật transfer request
- `DELETE /api/distributor/transfer-to-pharmacy` - Hủy transfer request

### Pharmacy
- `GET /api/pharmacy` - Lấy NFTs trong pharmacy
- `PUT /api/pharmacy` - Cập nhật NFT

---

## 🔧 Cấu Hình

### Database Connection

Backend hỗ trợ nhiều cách cấu hình connection string:

1. **DATABASE_URL** (postgresql:// format) - Tự động chuyển đổi
2. **POSTGRES_CONNECTION** (connection string format)
3. **NEON_CONNECTION** (connection string format)
4. **appsettings.json** (fallback)

### CORS

Backend đã cấu hình CORS cho:
- `http://localhost:3000`
- `http://localhost:3001`

### Environment Variables

**Thứ tự ưu tiên đọc config:**
1. Environment variables (từ `.env` hoặc system)
2. `appsettings.json`
3. `appsettings.Development.json`

---

## 🛠️ Development

### Chạy Backend

```powershell
cd server/PharmaDNAServer
dotnet run
```

### Chạy Frontend

```powershell
cd client
npm run dev
```

### Tạo Migration Mới

```powershell
cd server/PharmaDNAServer
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Build Production

**Backend:**
```powershell
cd server/PharmaDNAServer
dotnet publish -c Release
```

**Frontend:**
```powershell
cd client
npm run build
npm start
```

---

## 🔒 Bảo Mật

### Files Được Bảo Vệ

- `*.env` - Environment variables
- `appsettings.json` - Configuration với secrets
- `appsettings.Development.json` - Development config

Tất cả đã được thêm vào `.gitignore` và sẽ không bị commit lên Git.

### Khuyến Nghị

- Không commit file `.env` hoặc `appsettings.json` có chứa secrets
- Sử dụng environment variables trong production
- Rotate keys và passwords định kỳ

---

## 📝 Lưu Ý Quan Trọng

1. **Database:** Đảm bảo database Neon.tech đã được tạo và connection string đúng
2. **Migrations:** Chạy migrations trước khi chạy server lần đầu
3. **Environment Variables:** Đảm bảo tất cả biến môi trường đã được cấu hình
4. **Ports:** 
   - Backend: `5196` (HTTP), `7164` (HTTPS)
   - Frontend: `3000`
5. **CORS:** Nếu thay đổi port frontend, cần cập nhật CORS trong `Program.cs`

---

## 🐛 Troubleshooting

### Lỗi: "No database connection string found"
- Kiểm tra file `.env` có `DATABASE_URL` hoặc `POSTGRES_CONNECTION`
- Hoặc kiểm tra `appsettings.json` có `PostgresConnection`

### Lỗi: "SSL connection required"
- Đảm bảo connection string có `SSL Mode=Require`
- Hoặc `DATABASE_URL` có `?sslmode=require`

### Lỗi: Migration failed
- Xóa thư mục `Migrations/` và tạo lại
- Đảm bảo database đã được tạo

### Lỗi: CORS
- Kiểm tra port frontend có đúng trong CORS config không
- Kiểm tra `Program.cs` có cấu hình CORS đúng không

---

## 📚 Tài Liệu Tham Khảo

- [Next.js Documentation](https://nextjs.org/docs)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Neon.tech Documentation](https://neon.tech/docs)
- [Pinata Documentation](https://docs.pinata.cloud/)

---

## 📄 License

[Thêm license của dự án]

---

## 👥 Contributors

[Thêm thông tin contributors]

---

**Cập nhật lần cuối:** 19/11/2025

