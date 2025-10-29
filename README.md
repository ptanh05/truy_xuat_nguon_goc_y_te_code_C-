## PharmaDNA

Hệ thống truy xuất nguồn gốc dược phẩm sử dụng Blockchain (NFT), AIoT và IPFS. Kiến trúc tách biệt Client-Server:

- Client: Next.js (React) — thư mục `client/`
- Server: ASP.NET Core Web API (.NET 9) — thư mục `server/PharmaDNAServer/`

### Tính năng chính

- Mint NFT cho lô thuốc (metadata trên IPFS qua Pinata)
- Quản lý vận chuyển và milestones (Distributor)
- Nhà thuốc xác nhận nhập kho (Pharmacy)
- Quản trị viên gán quyền on-chain và đồng bộ backend
- Tra cứu lịch sử/mốc vận chuyển theo lô

## Cấu trúc thư mục

```
.
├─ client/                       # Next.js frontend
│  ├─ app/                       # Pages (manufacturer, distributor, pharmacy, admin, lookup)
│  ├─ components/ hooks/ lib/    # UI, hooks và API helper tới server
│  └─ next.config.mjs
└─ server/
   └─ PharmaDNAServer/           # ASP.NET Core API
      ├─ Controllers/            # Manufacturer, Distributor, Pharmacy, Admin, IPFS
      ├─ Data/ Models/ Services/ # EF Core, entities, blockchain service (placeholder)
      ├─ Program.cs              # Cấu hình CORS, DB, cấu hình contract
      └─ appsettings*.json
```

## Yêu cầu hệ thống

- Node.js 18+ và npm
- .NET SDK 9
- PostgreSQL (khuyến nghị dùng Neon `DATABASE_URL`) hoặc PostgreSQL cục bộ/Docker
- Pinata JWT cho IPFS

## Biến môi trường

### Server (`server/PharmaDNAServer`) — bảo mật, không commit

Đặt trong biến môi trường hệ thống hoặc file user-secret/.env local (không push):

- `DATABASE_URL` — chuỗi kết nối Postgres (Neon), dạng `postgres://USER:PASS@HOST:PORT/DB`
- `PINATA_JWT` — token JWT của Pinata để upload IPFS
- `PHARMA_NFT_ADDRESS` — địa chỉ contract PharmaNFT
- `OWNER_PRIVATE_KEY` — private key owner để ký giao dịch quản trị
- `PHARMADNA_RPC` — RPC endpoint của chain (ví dụ Saga/pharmadna)

Ghi chú:
- Server tự động ưu tiên đọc `DATABASE_URL` và parse sang Npgsql connection string (bật SSL phù hợp Neon). Nếu không có, sẽ fallback `ConnectionStrings:DefaultConnection` trong `appsettings.Development.json`.
- CORS đã mở cho `http://localhost:3000` và `http://localhost:3001`.

### Client (`client/`) — chỉ biến công khai

- `NEXT_PUBLIC_API_URL` — Base URL tới API .NET, mặc định `http://localhost:5196/api`
- `NEXT_PUBLIC_PHARMA_NFT_ADDRESS` — địa chỉ contract để client hiển thị/tra cứu

Không đặt các secret như `DATABASE_URL`, `OWNER_PRIVATE_KEY`, `PINATA_JWT` ở client.

## Cài đặt & chạy

### Server (.NET API)

```powershell
cd "server/PharmaDNAServer"
dotnet restore
# Đảm bảo đã set biến môi trường DATABASE_URL (Neon) hoặc cấu hình ConnectionStrings
dotnet ef database update  # nếu có migrations
dotnet run                 # sẽ lắng nghe http://localhost:5196
```

Chạy nhanh PostgreSQL bằng Docker (tuỳ chọn):

```powershell
docker run --name postgres-pharma -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=pharmadna_dev -p 5432:5432 -d postgres:15
```

### Client (Next.js)

```powershell
cd "client"
npm install
npm run dev      # http://localhost:3000
```

Đặt `NEXT_PUBLIC_API_URL` nếu server chạy cổng khác:

```powershell
$env:NEXT_PUBLIC_API_URL = "http://localhost:5196/api"
npm run dev
```

## Điểm tích hợp chính

- Client gọi API thông qua helper `client/lib/api.ts` trỏ tới `.NET API`
- Tất cả API routes Next.js trước đây đã được migrate sang Controllers .NET:
  - `ManufacturerController`, `DistributorController`, `PharmacyController`, `AdminController`, `IPFSController`
- EF Core quản lý bảng `nfts`, `milestones`, `users`, `transfer_requests` (tuỳ models hiện có)
- `BlockchainService` là placeholder để tích hợp `ethers`/.NET web3: assign role, mint, sync on-chain

## Thông số chain (ví dụ)

- Chain ID: `2759821881746000` (0x9ce0b1ae7a250)
- RPC URL: `https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io`
- Explorer: `https://pharmadna-2759821881746000-1.sagaexplorer.io`
- Native: `PDNA`

## Troubleshooting

- PowerShell khóa script: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`
- Không tìm thấy project khi `dotnet run`: `cd server/PharmaDNAServer`
- Lỗi kết nối DB (Postgres): kiểm tra `DATABASE_URL`, service Postgres/Docker, hoặc firewall
- File khóa khi build: dừng tiến trình cũ `taskkill /F /IM PharmaDNAServer.exe`

## Ghi chú phát triển

- Không để secrets trong client. Server đọc từ biến môi trường.
- Khi đổi schema DB, thêm migration và `dotnet ef database update`.
- Khi triển khai, cấu hình `DATABASE_URL`, `PHARMADNA_RPC`, `PINATA_JWT`, `PHARMA_NFT_ADDRESS`, `OWNER_PRIVATE_KEY` trên môi trường server.

