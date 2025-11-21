# Backend (server/PharmaDNAServer)

ASP.NET Core 9 API chịu trách nhiệm:
- Lưu trữ metadata NFT, transfer request, sensor log trong PostgreSQL.
- Kết nối blockchain (ethers thông qua `BlockchainService`) để xác thực role.
- Upload và trả về dữ liệu IPFS (Pinata) cho frontend.

## 1. Công nghệ
- .NET 9 + Minimal Hosting model.
- EF Core 9 + Npgsql cho PostgreSQL.
- DotNetEnv để load `.env`.
- Swagger/Swashbuckle cho tài liệu API khi chạy dev.

## 2. Yêu cầu
| Công cụ | Phiên bản |
|---------|----------|
| .NET SDK | 9.0.x |
| PostgreSQL | 14+ (Neon.tech cũng được) |
| Pinata API Key/JWT | Bắt buộc khi upload |

## 3. Cấu hình môi trường
Tạo file `server/PharmaDNAServer/.env`:
```
# PostgreSQL (chọn 1 trong 3 cách)
DATABASE_URL=postgresql://user:pass@host/dbname?sslmode=require
# HOẶC
POSTGRES_CONNECTION=Host=...;Database=...;Username=...;Password=...;SSL Mode=Require
# HOẶC
NEON_CONNECTION=Host=...  # nếu dùng Neon auto-generated string

# Blockchain
PHARMA_NFT_ADDRESS=<CONTRACT_ADDRESS_SAU_DEPLOY>
OWNER_PRIVATE_KEY=<OWNER_PRIVATE_KEY>
PHARMADNA_RPC=<RPC_URL_CUA_CHAIN>

# Pinata / IPFS
PINATA_JWT=eyJhbGciOiJI...   # JWT hoặc API key phù hợp
PINATA_API_URL=https://api.pinata.cloud
PINATA_GATEWAY=https://gateway.pinata.cloud/ipfs/

# API
CORS_ORIGINS=http://localhost:3000,http://localhost:3001
```

> `DATABASE_URL` sẽ được convert sang format `Host=...` nếu bạn dùng Neon. Nếu thiếu, app sẽ throw exception khi khởi động.

## 4. Migration & Database
1. Sửa `ConnectionStrings:PostgresConnection` trong `appsettings.json` (nếu không dùng `.env`).  
2. Chạy migration:
   ```bash
   cd server/PharmaDNAServer
   dotnet ef database update
   ```
   Migrations nằm trong `Migrations/2025xxxx_InitialCreate.cs`.

## 5. Chạy ứng dụng
```bash
cd server/PharmaDNAServer
dotnet run          # hoặc dotnet watch run
# API base: http://localhost:5196
# Swagger: http://localhost:5196/swagger (dev only)
```

## 6. Kiến trúc
| Thư mục | Nội dung |
|---------|----------|
| `Controllers/` | API endpoints: `ManufacturerController`, `DistributorController`, `PharmacyController`, `AdminController`, `IPFSController`. |
| `Data/ApplicationDbContext.cs` | DbContext EF Core. |
| `Models/` | Entity + DTO (NFT, TransferRequest, Milestone, SensorLog, User, ContractOptions). |
| `Services/` | Business logic: blockchain service, role service, milestone service, sensor service. |

### Luồng API chính
- `POST /api/manufacturer/upload-ipfs` → upload metadata, lưu NFT vào DB.
- `POST /api/manufacturer/mint` (thông qua blockchain service) → FE ký giao dịch.
- `POST /api/manufacturer/transfer-request` → tạo yêu cầu giao NFT cho distributor.
- `PUT /api/manufacturer/transfer-request` → approve/deny (transaction + validation).
- `POST /api/distributor/confirm-receipt` → xác nhận đã nhận, tạo milestone.
- `POST /api/distributor/transfer-to-pharmacy` & `PUT ...` → gửi/từ chối yêu cầu tới pharmacy.
- `POST /api/distributor/upload-sensor` → lưu sensor log (TODO: push queue).
- `GET /api/pharmacy` + `PUT /api/pharmacy` → danh sách và cập nhật status trong kho.
- `POST /api/admin` / `DELETE /api/admin` → cấp/xoá role.

## 7. Bảo mật & best practices
- Luôn chạy HTTPS reverse proxy khi deploy production.
- Không commit `.env`, private key, hoặc JWT.
- Xem xét bổ sung authentication thực sự cho Admin thay vì cred cố định.
- Bật rate limiting / API key nếu mở API public.

## 8. Deploy gợi ý
1. Build release: `dotnet publish -c Release`.
2. Deploy lên container (Docker) hoặc Azure App Service. Ví dụ Dockerfile nhanh:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
   WORKDIR /app
   EXPOSE 8080

   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   WORKDIR /src
   COPY . .
   RUN dotnet publish PharmaDNAServer.csproj -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=build /app/publish .
   ENTRYPOINT ["dotnet", "PharmaDNAServer.dll"]
   ```
3. Cấp biến môi trường qua secret manager / deployment pipeline.

## 9. Troubleshooting
| Vấn đề | Nguyên nhân / Cách xử lý |
|--------|-------------------------|
| `No PostgreSQL connection string found` | Thiếu `DATABASE_URL` hoặc `POSTGRES_CONNECTION`. |
| `PINATA_JWT chưa được cấu hình` | Backend cần JWT để gọi Pinata. |
| 403 CORS | Bổ sung domain FE vào `CORS_ORIGINS`. |
| EF Core timeout | Kiểm tra Neon security rules, bật pooling, đảm bảo SSL. |

---
👉 Quay lại [README tổng](../README.md) hoặc xem [Frontend README](../client/README.md) để hoàn tất thiết lập.

