# PharmaDNA Web Application

Hệ thống truy xuất nguồn gốc thuốc bằng Blockchain & AIoT - Phiên bản C# ASP.NET Core MVC

## 🎯 Tổng quan

PharmaDNA là hệ thống truy xuất nguồn gốc thuốc sử dụng công nghệ Blockchain và AIoT (Artificial Intelligence of Things) để đảm bảo tính minh bạch và xác thực nguồn gốc thuốc.

## 🏗️ Kiến trúc

### **Mô hình MVC (Model-View-Controller)**

- **Models**: Entities, DTOs, ViewModels
- **Views**: Razor Views với Tailwind CSS
- **Controllers**: API Controllers cho các chức năng

### **Công nghệ sử dụng**

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: PostgreSQL với Entity Framework Core
- **Blockchain**: Nethereum (Ethereum .NET)
- **IPFS**: Pinata Gateway
- **Frontend**: Tailwind CSS + Alpine.js
- **Smart Contract**: Solidity (PharmaNFT.sol)

## 📁 Cấu trúc dự án

```
PharmaDNA.Web/
├── Controllers/          # API Controllers
├── Models/              # Data Models
│   ├── Entities/        # Database Entities
│   ├── DTOs/           # Data Transfer Objects
│   └── ViewModels/     # View Models
├── Services/           # Business Logic
├── Views/              # Razor Views
├── Data/               # Database Context
├── wwwroot/            # Static Files
│   ├── css/           # Tailwind CSS
│   ├── js/            # JavaScript
│   ├── images/        # Static Images
│   └── contracts/     # Smart Contract ABI
├── Database/          # SQL Scripts
├── Contracts/         # Smart Contracts
└── Program.cs         # Application Entry Point
```

## 🚀 Cài đặt và chạy

### **1. Yêu cầu hệ thống**

- .NET 8.0 SDK
- PostgreSQL
- Node.js (cho Tailwind CSS)

### **2. Cài đặt dependencies**

```bash
# Cài đặt Tailwind CSS
npm install

# Build CSS
npm run build-css-prod
```

### **3. Cấu hình biến môi trường**

Tạo file `.env` hoặc set biến môi trường:

```env
DATABASE_URL=postgresql://username:password@host:port/database
PINATA_JWT=your-pinata-jwt-token
PHARMA_NFT_ADDRESS=0xYourContractAddress
OWNER_PRIVATE_KEY=your-private-key
PINATA_GATEWAY=https://gateway.pinata.cloud/ipfs/
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
```

### **4. Chạy ứng dụng**

```bash
dotnet run
```

Truy cập: `https://localhost:5001`

## 🎭 Vai trò trong hệ thống

### **1. Nhà sản xuất (Manufacturer)**

- Tạo NFT cho từng lô thuốc
- Upload metadata lên IPFS
- Mint NFT trên blockchain

### **2. Nhà phân phối (Distributor)**

- Quản lý vận chuyển thuốc
- Upload dữ liệu cảm biến AIoT
- Thêm milestones vào lịch sử

### **3. Nhà thuốc (Pharmacy)**

- Quét QR code để xác minh
- Xác nhận nhập kho
- Quản lý inventory

### **4. Người tiêu dùng (Consumer)**

- Tra cứu nguồn gốc thuốc
- Không cần kết nối ví
- Xem lịch sử sản phẩm

### **5. Quản trị viên (Admin)**

- Quản lý người dùng
- Cấp quyền vai trò
- Giám sát hệ thống

## 🔧 API Endpoints

### **Manufacturer**

- `POST /Manufacturer/CreateNFT` - Tạo NFT mới
- `GET /Manufacturer/GetTransferRequests` - Lấy yêu cầu chuyển
- `POST /Manufacturer/ApproveTransfer` - Duyệt chuyển NFT

### **Distributor**

- `GET /Distributor/GetNFTs` - Lấy danh sách NFT
- `POST /Distributor/UploadSensorData` - Upload dữ liệu cảm biến
- `POST /Distributor/AddMilestone` - Thêm milestone
- `POST /Distributor/RequestTransfer` - Yêu cầu chuyển NFT

### **Pharmacy**

- `GET /Pharmacy/GetTransferRequests` - Lấy yêu cầu chuyển
- `POST /Pharmacy/ApproveTransfer` - Duyệt nhận NFT
- `POST /Pharmacy/ScanQR` - Quét QR code

### **Lookup**

- `GET /Lookup/Search` - Tìm kiếm thuốc
- `GET /Lookup/GetProductHistory` - Lấy lịch sử sản phẩm

### **Admin**

- `GET /Admin/GetUsers` - Lấy danh sách người dùng
- `POST /Admin/AssignRole` - Cấp quyền vai trò
- `POST /Admin/CreateUser` - Tạo người dùng mới

## 🗄️ Database Schema

### **NFTs Table**

```sql
CREATE TABLE nfts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    batch_number VARCHAR(100) UNIQUE NOT NULL,
    status VARCHAR(50) NOT NULL,
    manufacturer_address VARCHAR(42) NOT NULL,
    distributor_address VARCHAR(42),
    pharmacy_address VARCHAR(42),
    ipfs_hash VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW(),
    manufacture_date DATE,
    expiry_date DATE,
    description TEXT,
    image_url VARCHAR(500)
);
```

### **Users Table**

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    address VARCHAR(42) UNIQUE NOT NULL,
    role VARCHAR(50) NOT NULL,
    assigned_at TIMESTAMP DEFAULT NOW()
);
```

### **Transfer Requests Table**

```sql
CREATE TABLE transfer_requests (
    id SERIAL PRIMARY KEY,
    nft_id INTEGER NOT NULL,
    distributor_address VARCHAR(42) NOT NULL,
    pharmacy_address VARCHAR(42) NOT NULL,
    transfer_note TEXT,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

### **Milestones Table**

```sql
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

## 🔗 Smart Contract

### **PharmaNFT.sol**

- **Functions**: `mintProductNFT`, `transferProductNFT`, `getProductHistory`
- **Roles**: Manufacturer, Distributor, Pharmacy, Admin
- **Events**: `ProductMinted`, `ProductTransferred`, `RoleAssigned`

### **Contract Functions**

```solidity
// Mint NFT
function mintProductNFT(string memory uri) public onlyRole(Role.Manufacturer) returns (uint256)

// Transfer NFT
function transferProductNFT(uint256 tokenId, address to) public onlyTokenOwner(tokenId)

// Get History
function getProductHistory(uint256 tokenId) public view returns (address[] memory)

// Role Management
function assignRole(address user, Role role) public onlyOwner
```

## 🎨 UI/UX

### **Tailwind CSS Components**

- **Buttons**: `.btn-primary`, `.btn-secondary`, `.btn-success`
- **Cards**: `.card`, `.card-header`, `.card-body`
- **Forms**: `.form-input`, `.form-label`
- **Badges**: `.badge`, `.badge-success`, `.badge-warning`
- **Alerts**: `.alert`, `.alert-success`, `.alert-danger`

### **Responsive Design**

- Mobile-first approach
- Breakpoints: sm, md, lg, xl
- Custom animations và transitions

## 🔒 Bảo mật

### **Authentication**

- JWT Bearer tokens
- Role-based authorization
- Session management

### **Data Protection**

- Input validation
- SQL injection prevention
- XSS protection

### **Blockchain Security**

- Private key management
- Transaction signing
- Gas optimization

## 📊 Monitoring & Logging

### **Logging**

- Console logging
- File logging
- Error tracking

### **Performance**

- Database query optimization
- Caching strategies
- Response time monitoring

## 🚀 Deployment

### **Production Setup**

1. Cấu hình production database
2. Set up reverse proxy (Nginx)
3. SSL certificate
4. Environment variables
5. Health checks

### **Docker Support**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "PharmaDNA.Web.dll"]
```

## 🤝 Contributing

1. Fork repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## 📄 License

MIT License - Xem file LICENSE để biết thêm chi tiết.

## 📞 Support

- **Email**: support@pharmadna.com
- **Documentation**: [Wiki](https://github.com/pharmadna/wiki)
- **Issues**: [GitHub Issues](https://github.com/pharmadna/issues)

---

**PharmaDNA** - Truy xuất nguồn gốc thuốc bằng Blockchain & AIoT 🏥💊🔗
