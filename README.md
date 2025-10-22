# PharmaDNA - Truy xuất nguồn gốc y tế

Dự án hybrid gồm:

- **Backend**: ASP.NET Core 8 Web API + Razor Pages
- **Frontend**: Next.js 15 + React + TypeScript

## 🚀 HƯỚNG DẪN CHẠY

### **Bước 1: Cài đặt Database**

1. Mở SQL Server Management Studio
2. Chạy script SQL trong file `database_setup.sql` (sẽ tạo sau)
3. Database: `PharmaDNA` trên server `PHUNGTHEANH\SQLEXPRESS`

### **Bước 2: Cấu hình Environment Variables**

```cmd
# Infura (Blockchain)
setx INFURA_ENDPOINT "https://mainnet.infura.io/v3/YOUR_PROJECT_ID"
setx INFURA_PROJECT_ID "YOUR_PROJECT_ID"

# Pinata (IPFS)
setx PINATA_API_KEY "YOUR_PINATA_API_KEY"
setx PINATA_SECRET_API_KEY "YOUR_PINATA_SECRET_API_KEY"
setx PINATA_JWT "YOUR_PINATA_JWT_TOKEN"
setx PINATA_GATEWAY "https://gateway.pinata.cloud/ipfs"
```

### **Bước 3: Chạy Backend**

```bash
# Mở Terminal 1
cd backend
dotnet restore
dotnet build
dotnet run
```

**URL**: `https://localhost:5001` hoặc `http://localhost:5000`
**Swagger**: `https://localhost:5001/swagger`

### **Bước 4: Chạy Frontend**

```bash
# Mở Terminal 2 (mới)
cd frontend
npm install
npm run dev
```

**URL**: `http://localhost:3000`

## 📁 CẤU TRÚC DỰ ÁN

```
├── backend/                 # ASP.NET Core 8
│   ├── Controllers/         # API Controllers
│   ├── Pages/              # Razor Pages
│   ├── Models/             # Data Models
│   ├── Services/           # Business Logic
│   ├── Data/               # Entity Framework
│   ├── Middleware/         # Custom Middleware
│   └── wwwroot/            # Static Files
│
├── frontend/               # Next.js 15
│   ├── app/                # App Router
│   ├── components/         # React Components
│   ├── lib/                # Utilities
│   ├── hooks/              # Custom Hooks
│   └── public/             # Static Assets
│
└── README.md
```

## 🔧 CÁC TÍNH NĂNG CHÍNH

### Backend (.NET)

- ✅ Web API với Swagger
- ✅ Razor Pages cho admin
- ✅ Entity Framework Core
- ✅ Authentication & Authorization
- ✅ Blockchain integration (Ethereum)
- ✅ IPFS storage (Pinata)
- ✅ QR Code generation
- ✅ Batch operations
- ✅ Audit logging

### Frontend (Next.js)

- ✅ React 18 + TypeScript
- ✅ Tailwind CSS
- ✅ Responsive design
- ✅ Web3 integration
- ✅ QR Code scanner
- ✅ Real-time updates

## 🛠️ TROUBLESHOOTING

### Lỗi kết nối Database

- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `backend/appsettings.Development.json`

### Lỗi Environment Variables

- Mở terminal mới sau khi setx
- Kiểm tra: `echo %INFURA_ENDPOINT%`

### Lỗi build .NET

- Chạy `dotnet clean` rồi `dotnet restore`
- Kiểm tra .NET 8 SDK đã cài

### Lỗi build Next.js

- Xóa `node_modules` và chạy `npm install`
- Kiểm tra Node.js version >= 18

## 📞 HỖ TRỢ

Nếu gặp lỗi, hãy:

1. Kiểm tra logs trong terminal
2. Xem Swagger UI: `https://localhost:5001/swagger`
3. Kiểm tra browser console (F12)
