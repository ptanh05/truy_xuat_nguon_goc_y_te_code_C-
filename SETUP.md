# Setup Instructions for PharmaDNA

## Tổng quan
Hệ thống truy xuất nguồn gốc dược phẩm sử dụng Blockchain và IPFS để quản lý vòng đời sản phẩm từ nhà sản xuất → nhà phân phối → nhà thuốc.

## Yêu cầu hệ thống

### Phần mềm cần cài đặt

1. **.NET 8.0 SDK**
   - Download từ: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify: `dotnet --version` (should show 8.x)

2. **Node.js** (cho Tailwind CSS)
   - Download từ: https://nodejs.org/
   - Verify: `node --version` và `npm --version`

3. **PostgreSQL Database**
   - Option 1: Sử dụng Neon.tech (free) tại https://neon.tech
   - Option 2: Cài đặt local PostgreSQL

4. **MetaMask** (extension trình duyệt)
   - Tải từ: https://metamask.io/
   - Kết nối với PharmaDNA chainlet

## Bước cài đặt

### 1. Clone hoặc tải source code
```bash
git clone <repository-url>
cd truy_xuat_nguon_goc_y_te_code_C-
```

### 2. Setup Database

#### Option A: Sử dụng Neon.tech (Recommended)

1. Đăng ký tài khoản tại https://neon.tech
2. Tạo database mới
3. Copy connection string
4. Lưu vào file `.env` trong thư mục `PharmaDNA.Web`

#### Option B: Local PostgreSQL

1. Cài đặt PostgreSQL trên máy
2. Tạo database:
   ```bash
   createdb pharmadna
   ```
3. Chạy script khởi tạo:
   ```bash
   psql -U postgres -d pharmadna -f PharmaDNA.Web/Database/init_database.sql
   ```

### 3. Setup IPFS (Pinata)

1. Đăng ký tài khoản tại https://pinata.cloud
2. Tạo API Key tại https://pinata.cloud/keys
3. Copy JWT token

### 4. Configure Blockchain

#### Deploy Smart Contract

1. Vào thư mục `saga-contract`
2. Install dependencies:
   ```bash
   npm install
   ```

3. Chạy script deploy:
   ```bash
   # Windows
   deploy-pharmadna.bat
   
   # Linux/Mac
   chmod +x deploy-pharmadna.sh
   ./deploy-pharmadna.sh
   ```

4. Lưu địa chỉ contract đã deploy

### 5. Tạo file .env

Copy file `env.example` thành `.env` trong thư mục `PharmaDNA.Web/`:

```env
# Database Configuration
DATABASE_URL=postgresql://user:password@host:5432/database

# Pinata IPFS Configuration
PINATA_JWT=Bearer YOUR_JWT_TOKEN
PINATA_GATEWAY=https://gateway.pinata.cloud/ipfs/

# Blockchain Configuration
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
PHARMA_NFT_ADDRESS=0x_YOUR_CONTRACT_ADDRESS
OWNER_PRIVATE_KEY=0x_YOUR_PRIVATE_KEY
```

### 6. Install Dependencies

#### Install .NET packages
```bash
cd PharmaDNA.Web
dotnet restore
```

#### Install Node.js packages (for Tailwind CSS)
```bash
npm install
```

### 7. Build Tailwind CSS

```bash
npm run build-css-prod
```

Hoặc chạy auto-watch cho development:
```bash
npm run build-css
```

### 8. Chạy ứng dụng

#### Windows
```bash
# Sử dụng script có sẵn
run.bat

# Hoặc chạy trực tiếp
dotnet run
```

#### Linux/Mac
```bash
cd PharmaDNA.Web
dotnet restore
dotnet build
dotnet run
```

Ứng dụng sẽ chạy tại:
- https://localhost:5001 (HTTPS)
- http://localhost:5000 (HTTP)

## Cấu hình MetaMask

1. Mở extension MetaMask
2. Thêm custom network:
   - Network Name: PharmaDNA Chainlet
   - RPC URL: https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
   - Chain ID: 2759821881746000
   - Currency Symbol: SAGA

## Cấu trúc thư mục

```
truy_xuat_nguon_goc_y_te_code_C-/
├── PharmaDNA.Web/               # ASP.NET Core MVC Application
│   ├── Controllers/             # Controllers cho các roles
│   ├── Services/                # Business logic
│   ├── Models/                  # Data models (Entities, DTOs, ViewModels)
│   ├── Views/                   # Razor views
│   ├── Database/                # SQL scripts
│   ├── wwwroot/                 # Static files
│   │   ├── contracts/          # Smart contract ABIs
│   │   ├── css/                # CSS files
│   │   └── js/                 # JavaScript files
│   ├── run.bat                 # Script chạy ứng dụng
│   └── .env                    # Environment variables (tự tạo)
├── saga-contract/              # Smart Contract
│   ├── contracts/              # Solidity contracts
│   └── deploy-pharmadna.bat    # Deploy script
├── README.md                   # Tài liệu chính
├── SETUP.md                    # File này
└── env.example                 # Mẫu file .env
```

## Troubleshooting

### Lỗi kết nối database
- Kiểm tra connection string trong file .env
- Đảm bảo database đã được tạo
- Verify PostgreSQL service đang chạy (nếu dùng local)

### Lỗi IPFS upload
- Kiểm tra PINATA_JWT token có đúng không
- Verify internet connection
- Kiểm tra rate limits trên Pinata

### Lỗi blockchain
- Kiểm tra RPC URL có đúng không
- Verify contract address
- Đảm bảo private key có đủ gas/fee

### Lỗi Tailwind CSS
- Chạy lại: `npm install`
- Rebuild: `npm run build-css-prod`
- Xóa cache: xóa `node_modules` và `package-lock.json`, rồi `npm install` lại

## Development Tips

### Hot Reload
Sử dụng `dotnet watch` để tự động reload khi code thay đổi:
```bash
dotnet watch run
```

### Database Migrations
Sử dụng EF Core migrations:
```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback
dotnet ef database update PreviousMigrationName
```

### Logging
Check logs trong:
- Console output (development)
- `/logs` directory (nếu có)
- Database logging tables

## Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

## Production Deployment

### Environment Variables
Ensure all environment variables are set correctly in production.

### Build for Production
```bash
dotnet publish -c Release
```

### Run as Service
Use process managers like PM2 or systemd.

## Support

Nếu gặp vấn đề, vui lòng:
1. Check logs trong console
2. Verify tất cả dependencies đã cài đặt
3. Kiểm tra file .env có cấu hình đúng
4. Xem documentation trong README.md

## License

MIT

