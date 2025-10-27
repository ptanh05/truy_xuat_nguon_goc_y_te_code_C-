# PharmaDNA - Hệ Thống Truy Xuất Nguồn Gốc Dược Phẩm

## 📋 Tổng Quan

Hệ thống truy xuất nguồn gốc dược phẩm sử dụng Blockchain và IPFS để quản lý vòng đời sản phẩm từ nhà sản xuất → nhà phân phối → nhà thuốc.

### ✨ Tính Năng Chính

- 🏭 **Manufacturer (Nhà Sản Xuất)**

  - Tạo NFT cho lô thuốc
  - Upload ảnh sản phẩm, chứng nhận lên IPFS
  - Xem danh sách NFT đã tạo
  - Duyệt yêu cầu chuyển từ distributor

- 🚚 **Distributor (Nhà Phân Phối)**

  - Upload dữ liệu cảm biến (AIoT)
  - Xem danh sách lô đang vận chuyển
  - Tạo yêu cầu chuyển lô
  - Theo dõi milestones

- 💊 **Pharmacy (Nhà Thuốc)**

  - Tra cứu thông tin thuốc bằng số lô
  - Xem toàn bộ lịch sử vận chuyển
  - Xác nhận nhập kho
  - Quản lý transfer requests

- 👤 **Admin**
  - Quản lý người dùng và phân quyền
  - Gán role trên blockchain
  - Xem thống kê hệ thống

## 🏗️ Kiến Trúc

### Dự án C# MVC - PharmaDNA.Web

```
PharmaDNA.Web/
├── Controllers/          # Controllers cho các roles
│   ├── AdminController.cs
│   ├── ManufacturerController.cs
│   ├── DistributorController.cs
│   ├── PharmacyController.cs
│   ├── LookupController.cs
│   └── HomeController.cs
├── Services/             # Business logic
│   ├── BlockchainService.cs    # Tương tác với smart contract
│   ├── IPFSService.cs          # Upload/Download IPFS
│   ├── NFTService.cs           # Quản lý NFT trong DB
│   └── UserService.cs          # Quản lý users
├── Models/               # Data models
│   ├── Entities/        # Entity Framework entities
│   ├── DTOs/           # Data transfer objects
│   └── ViewModels/     # View models
├── Views/              # Razor views
├── wwwroot/            # Static files
│   ├── contracts/     # Smart contract ABIs
│   ├── css/          # Tailwind CSS
│   └── images/       # Images
├── Contracts/         # Smart contract source (.sol)
└── Database/          # SQL scripts

```

### Dự án Smart Contract - saga-contract

```
saga-contract/
├── contracts/
│   └── PharmaNFT.sol    # Smart contract chính
├── scripts/
│   └── deployPharmaNFT.ts
└── deploy-pharmadna.bat
```

## 🚀 Cài Đặt và Chạy

### Yêu Cầu

- .NET 8.0 SDK
- Node.js (cho Tailwind CSS)
- PostgreSQL (Neon.tech)
- MetaMask (kết nối với PharmaDNA chainlet)

### Biến Môi Trường

Tạo file `.env` hoặc thiết lập biến môi trường hệ thống:

```env
DATABASE_URL=postgresql://...    # Connection string từ Neon.tech
PINATA_JWT=Bearer ...            # JWT token từ Pinata
PHARMA_NFT_ADDRESS=0x...         # Địa chỉ contract đã deploy
OWNER_PRIVATE_KEY=0x...          # Private key để sign transactions
PINATA_GATEWAY=https://gateway.pinata.cloud/ipfs/
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
```

### Chạy Dự Án C# MVC

**Cách 1: Sử dụng file run.bat**

```bash
cd PharmaDNA.Web
run.bat
```

**Cách 2: Chạy trực tiếp**

```bash
cd PharmaDNA.Web
dotnet restore
dotnet build
dotnet run
```

Mở trình duyệt tại: `https://localhost:5001` hoặc `http://localhost:5000`

### Build Tailwind CSS

```bash
cd PharmaDNA.Web
npm install
npm run build-css
```

## 🔗 Blockchain

### Network: PharmaDNA Chainlet (Saga)

- **Chain ID**: 2759821881746000 (0x9ce0b1ae7a250)
- **RPC**: https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
- **Explorer**: https://pharmadna-2759821881746000-1.sagaexplorer.io

### Deploy Smart Contract

```bash
cd saga-contract
deploy-pharmadna.bat
```

Sau khi deploy, cập nhật `PHARMA_NFT_ADDRESS` trong file `.env`.

### Roles

- `MANUFACTURER` = 1
- `DISTRIBUTOR` = 2
- `PHARMACY` = 3
- `ADMIN` = 4

## 📦 IPFS (Pinata)

Upload metadata và files lên IPFS:

- Metadata thuốc
- Ảnh sản phẩm
- Chứng nhận/chứng chỉ
- Dữ liệu cảm biến (AIoT)

## 🗄️ Database

### Schema

```sql
-- Bảng NFTs
CREATE TABLE nfts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    batch_number VARCHAR(100) NOT NULL UNIQUE,
    status VARCHAR(50) NOT NULL,
    manufacturer_address VARCHAR(42) NOT NULL,
    distributor_address VARCHAR(42),
    pharmacy_address VARCHAR(42),
    ipfs_hash VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW(),
    ...
);

-- Bảng Users
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    address VARCHAR(42) UNIQUE NOT NULL,
    role VARCHAR(50) NOT NULL,
    assigned_at TIMESTAMP DEFAULT NOW()
);

-- Bảng TransferRequests
CREATE TABLE transfer_requests (
    id SERIAL PRIMARY KEY,
    nft_id INTEGER NOT NULL,
    distributor_address VARCHAR(42) NOT NULL,
    pharmacy_address VARCHAR(42),
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT NOW(),
    ...
);

-- Bảng Milestones
CREATE TABLE milestones (
    id SERIAL PRIMARY KEY,
    nft_id INTEGER NOT NULL,
    type VARCHAR(50) NOT NULL,
    description TEXT,
    location VARCHAR(255),
    timestamp TIMESTAMP DEFAULT NOW(),
    actor_address VARCHAR(42) NOT NULL
);
```

## 🔧 Công Nghệ Sử Dụng

### Backend

- **ASP.NET Core 8.0 MVC**
- **Entity Framework Core** - ORM cho PostgreSQL
- **Nethereum** - Tương tác với Ethereum/blockchain
- **HttpClient** - Gọi API Pinata

### Frontend

- **Razor Views** - Server-side rendering
- **Tailwind CSS** - Styling
- **JavaScript** - Client-side logic
- **MetaMask** - Wallet integration (frontend)

### Blockchain

- **Solidity** - Smart contract language
- **Hardhat** - Development framework
- **Saga Chainlet** - Custom blockchain network

### Database

- **PostgreSQL** (Neon.tech hosted)

### IPFS

- **Pinata** - IPFS pinning service

## 📝 API Endpoints

### Web Controllers

#### Manufacturer
- `POST /Manufacturer/CreateNFT` - Tạo NFT mới
- `GET /Manufacturer/GetNFTs` - Lấy danh sách NFT
- `POST /Manufacturer/ApproveTransfer` - Duyệt yêu cầu chuyển

#### Distributor
- `GET /Distributor/GetNFTs` - Lấy lô đang vận chuyển
- `POST /Distributor/UploadSensorData` - Upload dữ liệu cảm biến
- `POST /Distributor/RequestTransfer` - Tạo yêu cầu chuyển

#### Pharmacy
- `GET /Pharmacy/LookupDrug?batchNumber=xxx` - Tra cứu thuốc
- `POST /Pharmacy/ConfirmReceived` - Xác nhận nhập kho
- `GET /Pharmacy/GetTransferRequests` - Lấy danh sách yêu cầu

#### Admin
- `POST /Admin/AssignRole` - Phân quyền người dùng
- `DELETE /Admin/DeleteUser` - Xóa người dùng
- `GET /Admin/GetUsers` - Lấy danh sách users

### REST API (Mobile/External)

#### Drug Lookup
- `GET /api/lookup/{batchNumber}` - Tra cứu thuốc
- `GET /api/nfts` - Lấy danh sách NFTs
- `POST /api/nfts` - Tạo NFT mới

#### Milestones
- `GET /api/nfts/{id}/milestones` - Lấy milestones
- `POST /api/nfts/{id}/milestones` - Thêm milestone

#### Users
- `GET /api/users/{address}` - Lấy thông tin user

#### Health Check
- `GET /api/health` - Health check chi tiết
- `GET /api/health/ready` - Readiness check
- `GET /api/health/live` - Liveness check

### Swagger Documentation
- `GET /api-docs` - Swagger UI (Development only)

## 🧪 Testing

```bash
# Chạy tests (nếu có)
dotnet test
```

## 📄 License

MIT

## 👨‍💻 Author

PharmaDNA Development Team
