# 📥 Hướng dẫn Cài đặt PharmaDNA

## ⚠️ Yêu cầu Hệ thống

### 1. **.NET 8 SDK** (BẮT BUỘC)
- Download: https://dotnet.microsoft.com/download/dotnet/8.0
- Chọn: **.NET 8.0 SDK (Standalone installer)**
- Cài đặt file `.exe` đã tải về
- Khởi động lại Terminal/PowerShell sau khi cài đặt

**Kiểm tra cài đặt:**
```bash
dotnet --version
# Kết quả mong đợi: 8.0.x
```

### 2. **Node.js & npm** (BẮT BUỘC cho Tailwind CSS)
- Download: https://nodejs.org/
- Chọn phiên bản LTS (v18 hoặc v20)
- Cài đặt file `.msi` đã tải về

**Kiểm tra cài đặt:**
```bash
node --version
npm --version
```

### 3. **PostgreSQL Database** (BẮT BUỘC)
- **Option 1:** Neon.tech (Miễn phí) - https://neon.tech
- **Option 2:** Local PostgreSQL - https://www.postgresql.org/download/

### 4. **Pinata Account** (BẮT BUỘC cho IPFS)
- Đăng ký: https://pinata.cloud
- Tạo API Key trong Dashboard
- Lấy JWT Token

### 5. **MetaMask Wallet** (Cho Blockchain)
- Cài đặt extension: https://metamask.io
- Tạo ví hoặc import
- Thêm Saga Testnet Network:
  - Chain ID: 2759821881746000
  - RPC URL: https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io

## 🚀 Cài đặt dự án

### Bước 1: Cài đặt .NET 8 SDK
```bash
# Download từ:
https://dotnet.microsoft.com/download/dotnet/8.0

# Chạy installer và chọn "Install SDK"
# Restart PowerShell sau khi cài
```

### Bước 2: Cài đặt Node.js
```bash
# Download từ:
https://nodejs.org/

# Chọn LTS version và cài đặt
```

### Bước 3: Clone và Setup dự án
```bash
# Đã có code rồi, skip clone
# Chỉ cần chạy setup:
setup.bat
```

### Bước 4: Cấu hình .env
```bash
# Mở file PharmaDNA.Web\.env
# Điền thông tin:

# Database (từ Neon.tech)
DATABASE_URL=postgresql://user:pass@host:5432/db

# Pinata (từ pinata.cloud)
PINATA_JWT=Bearer YOUR_JWT_HERE

# Blockchain (từ MetaMask)
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
PHARMA_NFT_ADDRESS=0x_DEPLOYED_CONTRACT_ADDRESS
OWNER_PRIVATE_KEY=0x_YOUR_PRIVATE_KEY
```

### Bước 5: Deploy Smart Contract
```bash
cd saga-contract
deploy-pharmadna.bat
# Copy địa chỉ contract và cập nhật PHARMA_NFT_ADDRESS trong .env
```

### Bước 6: Chạy ứng dụng
```bash
cd PharmaDNA.Web
run.bat

# Hoặc
dotnet run
```

## 🔍 Kiểm tra Cài đặt

### 1. Kiểm tra .NET
```bash
dotnet --version
# Phải >= 8.0.0
```

### 2. Kiểm tra Node.js
```bash
node --version
npm --version
```

### 3. Kiểm tra Database
```bash
# Test connection string trong .env
# Hoặc dùng pgAdmin để connect
```

### 4. Kiểm tra Configuration
```bash
cd PharmaDNA.Web
dotnet run -- --validate-config
```

## ❗ Troubleshooting

### Lỗi: "dotnet command not found"
- **Giải pháp:** Cài đặt .NET 8 SDK và restart PowerShell

### Lỗi: "npm command not found"
- **Giải pháp:** Cài đặt Node.js và restart PowerShell

### Lỗi: "Database connection failed"
- **Giải pháp:** 
  - Kiểm tra DATABASE_URL trong .env
  - Test connection với pgAdmin
  - Đảm bảo database đã được tạo

### Lỗi: "Configuration Error"
- **Giải pháp:** 
  - Kiểm tra file .env có đầy đủ biến
  - Chạy lại setup.bat

### Lỗi: "Smart contract not found"
- **Giải pháp:**
  - Deploy smart contract trước
  - Cập nhật PHARMA_NFT_ADDRESS trong .env

## 📞 Hỗ trợ

- **Documentation:** Xem `README.md`, `SETUP.md`
- **Quick Start:** Xem `QUICK_START.md`
- **Project Structure:** Xem `PROJECT_STRUCTURE.md`

## 🎯 Kết quả mong đợi

Sau khi cài đặt xong, bạn sẽ có:
- ✅ App chạy tại: https://localhost:5001
- ✅ API Docs tại: https://localhost:5001/api-docs
- ✅ Health Check tại: https://localhost:5001/api/health

---

**Chúc bạn cài đặt thành công! 🎉**
