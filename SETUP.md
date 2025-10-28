# Hướng dẫn Setup Dự án PharmaDNA

## Yêu cầu hệ thống

1. **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
2. **Node.js 18+** - [Download](https://nodejs.org/)
3. **PostgreSQL 14+** - [Download](https://www.postgresql.org/download/)
4. **Git** - [Download](https://git-scm.com/)

## Bước 1: Clone và Setup Database

```bash
# Clone repository
git clone <your-repo-url>
cd truy_xuat_nguon_goc_y_te_code_C-

# Tạo database
createdb pharmadna
```

## Bước 2: Setup Server (.NET)

```bash
cd server/PharmaDNAServer

# Restore packages
dotnet restore

# Tạo migrations
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Chạy server
dotnet run
```

Server sẽ chạy tại: `http://localhost:5000`

Kiểm tra: `http://localhost:5000/swagger` (Swagger UI)

## Bước 3: Setup Client (Next.js)

Mở terminal mới:

```bash
cd client

# Cài đặt dependencies
npm install

# Chạy development server
npm run dev
```

Client sẽ chạy tại: `http://localhost:3000`

## Bước 4: Cấu hình biến môi trường

### Server
Tạo hoặc cập nhật `server/PharmaDNAServer/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pharmadna;Username=postgres;Password=postgres;Port=5432"
  }
}
```

### Client
Tạo file `client/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_PHARMA_NFT_ADDRESS=0x...
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
OWNER_PRIVATE_KEY=your_private_key_here
PINATA_JWT=your_pinata_jwt_here
DATABASE_URL=postgresql://user:password@localhost:5432/pharmadna
```

## Bước 5: Xóa thư mục app/api (không cần nữa)

Vì đã chuyển API sang .NET server, bạn có thể xóa folder `app/api/`:

```bash
rm -rf app/api
```

Hoặc để lại nếu cần reference.

## Chạy cả 2 services cùng lúc

Sử dụng 2 terminal riêng:

**Terminal 1 - Server:**
```bash
cd server/PharmaDNAServer
dotnet watch run
```

**Terminal 2 - Client:**
```bash
cd client
npm run dev
```

## Kiểm tra

1. Server chạy: `http://localhost:5000/swagger`
2. Client chạy: `http://localhost:3000`
3. Kết nối client với server: Client sẽ gọi API tại `http://localhost:5000/api/*`

## Troubleshooting

### Lỗi database connection
- Kiểm tra PostgreSQL đang chạy: `pg_isready`
- Kiểm tra credentials trong `appsettings.json`

### Lỗi CORS
- Đảm bảo client đang chạy tại `http://localhost:3000`
- Kiểm tra CORS configuration trong `Program.cs`

### Lỗi API không tìm thấy
- Đảm bảo server đang chạy
- Kiểm tra `NEXT_PUBLIC_API_URL` trong `.env.local`

## Development Tips

### Hot Reload
- Server: `dotnet watch run`
- Client: `npm run dev` (Next.js tự động hot reload)

### Debugging
- Server: Đặt breakpoint trong Visual Studio hoặc VS Code
- Client: React DevTools extension

## Production Build

### Server
```bash
cd server/PharmaDNAServer
dotnet publish -c Release
```

### Client
```bash
cd client
npm run build
npm start
```

