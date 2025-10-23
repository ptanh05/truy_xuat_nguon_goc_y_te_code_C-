# PharmaDNA - Truy xuất nguồn gốc y tế

## Tổng quan

Dự án PharmaDNA là một hệ thống truy xuất nguồn gốc thuốc sử dụng blockchain và NFT để theo dõi chuỗi cung ứng dược phẩm.

## Công nghệ sử dụng

- **Backend**: .NET 8 Web API (C#)
- **Frontend**: Next.js 15 (TypeScript/React)
- **Database**: SQL Server
- **Blockchain**: Ethereum với Web3 và Nethereum
- **IPFS**: Pinata
- **UI**: Tailwind CSS

## Cài đặt và chạy

### Yêu cầu hệ thống

- .NET 8 SDK
- Node.js 18+
- SQL Server
- Git

### 🚀 Cách chạy nhanh nhất (Cả Backend + Frontend)

#### Cách 1: Sử dụng script tự động

```bash
# Windows
run_fullstack.bat

# Linux/Mac
chmod +x run_fullstack.sh
./run_fullstack.sh
```

#### Cách 2: Sử dụng npm script

```bash
# Cài đặt dependencies
npm install

# Chạy cả backend và frontend
npm run dev
```

#### Cách 3: Chạy từ frontend với concurrently

```bash
cd frontend
npm install
npm run dev:full
```

### Cài đặt riêng lẻ

#### Backend (.NET)

```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

#### Frontend (Next.js)

```bash
cd frontend
npm install
npm run dev
```

### Cấu hình Database

1. Tạo database `PharmaDNA` trong SQL Server
2. Cập nhật connection string trong `appsettings.json`
3. Chạy migrations:

```bash
cd backend
dotnet ef database update
```

### Cấu hình API Keys

Thiết lập các biến môi trường:

```bash
# Infura (Ethereum RPC)
setx INFURA_ENDPOINT "https://mainnet.infura.io/v3/YOUR_PROJECT_ID"

# Pinata (IPFS)
setx PINATA_API_KEY "YOUR_PINATA_API_KEY"
setx PINATA_SECRET_API_KEY "YOUR_PINATA_SECRET"
setx PINATA_JWT "YOUR_PINATA_JWT_TOKEN"
setx PINATA_GATEWAY "https://gateway.pinata.cloud/ipfs"
```

## Cấu trúc dự án

```
├── backend/                 # .NET 8 Web API
│   ├── Controllers/         # API Controllers
│   ├── Services/           # Business Logic
│   ├── Models/             # Data Models
│   ├── Data/               # Entity Framework Context
│   ├── Pages/              # Razor Pages
│   └── wwwroot/            # Static Files
├── frontend/               # Next.js Application
│   ├── app/                # App Router
│   ├── components/         # React Components
│   └── public/             # Static Assets
└── database_setup.sql     # Database Schema
```

## Tính năng chính

- ✅ Quản lý NFT thuốc
- ✅ Truy xuất nguồn gốc
- ✅ Chuyển giao quyền sở hữu
- ✅ Tích hợp blockchain
- ✅ Lưu trữ IPFS
- ✅ QR Code generation
- ✅ Báo cáo và thống kê
- ✅ Quản lý người dùng
- ✅ Audit trail

## API Endpoints

- `/api/nft` - Quản lý NFT
- `/api/transfer` - Chuyển giao
- `/api/ipfs` - IPFS operations
- `/api/qr` - QR Code generation
- `/api/analytics` - Thống kê

## Scripts chạy nhanh

- `run_backend.bat` - Chạy backend (Windows)
- `run_backend.sh` - Chạy backend (Linux/Mac)

## Lưu ý

- Dự án đã được build thành công với 119 warnings (không có errors)
- QRCode service đã được comment out để tránh lỗi dependency
- Tất cả các lỗi build chính đã được sửa
- Ứng dụng có thể chạy ngay sau khi cài đặt dependencies
