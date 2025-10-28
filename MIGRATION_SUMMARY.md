# Tóm tắt Migration: API Routes → .NET Controllers

## ✅ Đã hoàn thành

### 1. Models (.NET)
- `User.cs` - Quản lý users và roles
- `NFT.cs` - Thông tin NFT/drug lot
- `TransferRequest.cs` - Yêu cầu chuyển giao
- `Milestone.cs` - Lịch sử vận chuyển

### 2. DbContext
- `ApplicationDbContext.cs` - Entity Framework Context với PostgreSQL

### 3. Controllers
- ✅ `AdminController.cs` - Quản lý users và roles
- ✅ `ManufacturerController.cs` - Quản lý NFTs cho manufacturer
- ✅ `DistributorController.cs` - Quản lý vận chuyển
- ✅ `PharmacyController.cs` - Quản lý pharmacy

### 4. Configuration
- ✅ CORS setup trong `Program.cs`
- ✅ Database connection trong `appsettings.json`
- ✅ Swagger/OpenAPI enabled

## 📊 Mapping API Routes

| Old Route (Next.js) | New Route (.NET) | Controller Method |
|---------------------|------------------|-------------------|
| `GET /api/admin` | `GET /api/admin` | `AdminController.GetUsers()` |
| `POST /api/admin` | `POST /api/admin` | `AdminController.AssignRole()` |
| `DELETE /api/admin` | `DELETE /api/admin` | `AdminController.DeleteUser()` |
| `GET /api/manufacturer` | `GET /api/manufacturer` | `ManufacturerController.GetNFTs()` |
| `POST /api/manufacturer` | `POST /api/manufacturer` | `ManufacturerController.CreateNFT()` |
| `PUT /api/manufacturer` | `PUT /api/manufacturer` | `ManufacturerController.UpdateNFT()` |
| `DELETE /api/manufacturer` | `DELETE /api/manufacturer` | `ManufacturerController.DeleteNFT()` |
| `GET /api/manufacturer/transfer-request` | `GET /api/manufacturer/transfer-request` | `ManufacturerController.GetTransferRequests()` |
| `POST /api/manufacturer/transfer-request` | `POST /api/manufacturer/transfer-request` | `ManufacturerController.CreateTransferRequest()` |
| `PUT /api/manufacturer/transfer-request` | `PUT /api/manufacturer/transfer-request` | `ManufacturerController.ApproveTransferRequest()` |
| `GET /api/manufacturer/milestone` | `GET /api/manufacturer/milestone` | `ManufacturerController.GetMilestones()` |
| `POST /api/manufacturer/milestone` | `POST /api/manufacturer/milestone` | `ManufacturerController.CreateMilestone()` |
| `GET /api/distributor` | `GET /api/distributor` | `DistributorController.GetNFTsInTransit()` |
| `GET /api/distributor/roles` | `GET /api/distributor/roles` | `DistributorController.GetDistributors()` |
| `PUT /api/distributor` | `PUT /api/distributor` | `DistributorController.UpdateNFT()` |
| `GET /api/distributor/transfer-to-pharmacy` | `GET /api/distributor/transfer-to-pharmacy` | `DistributorController.GetTransferRequests()` |
| `POST /api/distributor/transfer-to-pharmacy` | `POST /api/distributor/transfer-to-pharmacy` | `DistributorController.CreateTransferToPharmacy()` |
| `PUT /api/distributor/transfer-to-pharmacy` | `PUT /api/distributor/transfer-to-pharmacy` | `DistributorController.UpdateTransferRequest()` |
| `DELETE /api/distributor/transfer-to-pharmacy` | `DELETE /api/distributor/transfer-to-pharmacy` | `DistributorController.CancelTransferRequest()` |
| `GET /api/pharmacy` | `GET /api/pharmacy` | `PharmacyController.GetNFTsInPharmacy()` |
| `PUT /api/pharmacy` | `PUT /api/pharmacy` | `PharmacyController.UpdateNFT()` |

## ⚠️ Còn thiếu

1. **Upload IPFS Endpoint**
   - Old: `POST /api/manufacturer/upload-ipfs`
   - Cần tạo controller riêng hoặc thêm vào ManufacturerController
   - Xử lý file upload và Pinata integration

2. **Auto Assign Role**
   - Old: `POST /api/admin/auto-assign-role`
   - Logic này nên được tích hợp vào AdminController hoặc tạo middleware

3. **Sensor Data Upload**
   - Old: `POST /api/distributor/upload-sensor`
   - Cần tạo endpoint cho distributor upload sensor data lên IPFS

## 🔄 Next Steps

1. Cập nhật client code để gọi API từ `.NET server` thay vì Next.js API routes
2. Xóa hoặc comment lại các file trong `app/api/`
3. Test các endpoints với Swagger UI
4. Implement missing endpoints (IPFS upload, auto-assign-role, sensor upload)

## 📝 Notes

- Tất cả endpoints giữ nguyên path structure
- Request/Response format giữ nguyên
- CORS đã được cấu hình để allow `localhost:3000`
- Database connection cần được cấu hình trong `appsettings.json`

