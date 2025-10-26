# PharmaDNA - Truy xuất nguồn gốc thuốc bằng Blockchain & AIoT

Hệ thống truy xuất nguồn gốc thuốc sử dụng công nghệ Blockchain và AIoT, được chuyển đổi từ TypeScript/Next.js sang C# ASP.NET Core MVC.

## Tính năng chính

- **Nhà sản xuất**: Tạo NFT cho từng lô thuốc, upload metadata lên IPFS
- **Nhà phân phối**: Quản lý vận chuyển, upload dữ liệu cảm biến AIoT
- **Nhà thuốc**: Quét QR, xác minh và xác nhận nhập kho
- **Người tiêu dùng**: Tra cứu nguồn gốc thuốc không cần kết nối ví
- **Quản trị viên**: Quản lý hệ thống, cấp quyền vai trò

## Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: PostgreSQL với Entity Framework Core
- **Blockchain**: Nethereum (Ethereum .NET)
- **IPFS**: Pinata API
- **Frontend**: Bootstrap 5, JavaScript ES6
- **Architecture**: Repository Pattern, Dependency Injection

## Cấu trúc dự án

```
PharmaDNA.Web/
├── Controllers/          # MVC Controllers
├── Models/              # Entities, DTOs, ViewModels
├── Services/            # Business Logic Services
├── Data/                # Database Context
├── Views/               # Razor Views
├── wwwroot/             # Static files
└── Program.cs           # Application entry point
```

## Cài đặt và chạy

### Yêu cầu hệ thống

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 hoặc VS Code

### Cài đặt

1. **Clone repository**

```bash
git clone <repository-url>
cd PharmaDNA.Web
```

2. **Cài đặt packages**

```bash
dotnet restore
```

3. **Cấu hình database**

```bash
# Tạo database PostgreSQL
createdb pharmadna

# Cập nhật connection string trong appsettings.json
```

4. **Cấu hình Blockchain và IPFS**

```json
{
  "Blockchain": {
    "RpcUrl": "https://your-rpc-url",
    "ContractAddress": "0x...",
    "PrivateKey": "..."
  },
  "IPFS": {
    "PinataJWT": "your-pinata-jwt",
    "GatewayUrl": "https://gateway.pinata.cloud/ipfs/"
  }
}
```

5. **Chạy ứng dụng**

```bash
dotnet run
```

Truy cập: `https://localhost:5001`

## Cấu trúc Database

### Bảng NFTs

- `Id`: Primary key
- `Name`: Tên thuốc
- `BatchNumber`: Số lô (unique)
- `Status`: Trạng thái (CREATED, IN_TRANSIT, IN_PHARMACY)
- `ManufacturerAddress`: Địa chỉ nhà sản xuất
- `DistributorAddress`: Địa chỉ nhà phân phối
- `PharmacyAddress`: Địa chỉ nhà thuốc
- `IpfsHash`: Hash metadata trên IPFS

### Bảng Users

- `Id`: Primary key
- `Address`: Địa chỉ ví (unique)
- `Role`: Vai trò (MANUFACTURER, DISTRIBUTOR, PHARMACY, ADMIN)
- `AssignedAt`: Ngày cấp quyền

### Bảng TransferRequests

- `Id`: Primary key
- `NftId`: ID NFT
- `DistributorAddress`: Địa chỉ nhà phân phối
- `PharmacyAddress`: Địa chỉ nhà thuốc
- `Status`: Trạng thái (PENDING, APPROVED, REJECTED)

### Bảng Milestones

- `Id`: Primary key
- `NftId`: ID NFT
- `Type`: Loại mốc
- `Description`: Mô tả
- `Location`: Vị trí
- `Timestamp`: Thời gian
- `ActorAddress`: Địa chỉ người thực hiện

## API Endpoints

### Manufacturer

- `POST /Manufacturer/CreateNFT` - Tạo NFT mới
- `GET /Manufacturer/GetTransferRequests` - Lấy yêu cầu chuyển giao
- `POST /Manufacturer/ApproveTransfer` - Chấp thuận chuyển giao

### Distributor

- `GET /Distributor/GetNFTs` - Lấy danh sách NFT
- `POST /Distributor/UploadSensorData` - Upload dữ liệu cảm biến
- `POST /Distributor/AddMilestone` - Thêm mốc vận chuyển

### Pharmacy

- `GET /Pharmacy/LookupDrug` - Tra cứu thuốc
- `POST /Pharmacy/ConfirmReceived` - Xác nhận nhập kho

### Admin

- `GET /Admin/GetUsers` - Lấy danh sách người dùng
- `POST /Admin/AssignRole` - Cấp quyền
- `DELETE /Admin/DeleteUser` - Xóa người dùng

### Lookup

- `GET /Lookup/Search` - Tìm kiếm thuốc
- `GET /Lookup/GetDrugHistory` - Lấy lịch sử thuốc

## Services

### BlockchainService

- Tích hợp với smart contract
- Mint NFT, transfer, kiểm tra quyền
- Sử dụng Nethereum library

### IPFSService

- Upload metadata và files lên IPFS
- Sử dụng Pinata API
- Trả về IPFS hash

### NFTService

- Quản lý NFT trong database
- CRUD operations
- Business logic

### UserService

- Quản lý người dùng
- Cấp quyền
- Đồng bộ với blockchain

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "PharmaDNA.Web.dll"]
```

### Azure

1. Tạo App Service
2. Cấu hình connection string
3. Deploy từ GitHub/Azure DevOps

### AWS

1. Sử dụng Elastic Beanstalk
2. Cấu hình RDS PostgreSQL
3. Deploy với AWS CLI

## Monitoring và Logging

- **Logging**: Serilog với file và console sinks
- **Health Checks**: ASP.NET Core Health Checks
- **Metrics**: Application Insights (Azure)

## Security

- **Authentication**: JWT Bearer tokens
- **Authorization**: Role-based access control
- **HTTPS**: Bắt buộc trong production
- **CORS**: Cấu hình cho frontend

## Testing

```bash
# Unit tests
dotnet test

# Integration tests
dotnet test --filter Category=Integration
```

## Contributing

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push và tạo Pull Request

## License

MIT License - xem file LICENSE để biết thêm chi tiết.

## Support

Liên hệ: support@pharmadna.com
