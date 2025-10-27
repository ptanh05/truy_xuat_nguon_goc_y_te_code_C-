# 🚀 PharmaDNA - Quick Start Guide

## ⚡ Cài đặt nhanh (5 phút)

### 1. **Chạy script setup tự động**

```bash
# Windows
setup.bat

# Linux/Mac
chmod +x setup.sh
./setup.sh
```

### 2. **Cấu hình file .env**

Mở file `PharmaDNA.Web/.env` và điền thông tin:

```env
# BẮT BUỘC - Database
DATABASE_URL=postgresql://user:password@host:5432/database

# BẮT BUỘC - Pinata IPFS
PINATA_JWT=Bearer YOUR_JWT_TOKEN

# BẮT BUỘC - Blockchain
PHARMADNA_RPC=https://pharmadna-2759821881746000-1.jsonrpc.sagarpc.io
PHARMA_NFT_ADDRESS=0x_YOUR_CONTRACT_ADDRESS
OWNER_PRIVATE_KEY=0x_YOUR_PRIVATE_KEY
```

### 3. **Deploy Smart Contract**

```bash
cd saga-contract
# Windows
deploy-pharmadna.bat

# Linux/Mac
chmod +x deploy-pharmadna.sh
./deploy-pharmadna.sh
```

### 4. **Cập nhật địa chỉ contract**

Sau khi deploy, copy địa chỉ contract và cập nhật vào file `.env`:
```env
PHARMA_NFT_ADDRESS=0x1234567890abcdef...
```

### 5. **Chạy ứng dụng**

```bash
# Windows
cd PharmaDNA.Web
run.bat

# Linux/Mac
./run.sh
```

## 🌐 Truy cập ứng dụng

- **Web App**: https://localhost:5001
- **API Docs**: https://localhost:5001/api-docs
- **Health Check**: https://localhost:5001/api/health

## 🐳 Hoặc chạy với Docker

```bash
# Copy env cho Docker
cp env.docker .env

# Chạy với Docker Compose
docker-compose up -d
```

## ❗ Lưu ý quan trọng

1. **Bắt buộc phải có file .env** với đầy đủ thông tin
2. **Database phải được tạo trước** (Neon.tech hoặc local PostgreSQL)
3. **Smart contract phải được deploy** trước khi chạy app
4. **Pinata JWT token** phải hợp lệ

## 🔧 Troubleshooting

### Lỗi "Configuration Error"
- Kiểm tra file `.env` có đầy đủ biến bắt buộc
- Chạy lại `setup.bat` hoặc `setup.sh`

### Lỗi "Database connection failed"
- Kiểm tra `DATABASE_URL` trong file `.env`
- Đảm bảo database đã được tạo

### Lỗi "Blockchain connection failed"
- Kiểm tra `PHARMADNA_RPC` và `PHARMA_NFT_ADDRESS`
- Đảm bảo smart contract đã được deploy

### Lỗi "IPFS upload failed"
- Kiểm tra `PINATA_JWT` token
- Đảm bảo token có quyền upload

## 📞 Hỗ trợ

- **Documentation**: Xem `SETUP.md` và `README.md`
- **API Reference**: https://localhost:5001/api-docs
- **Health Status**: https://localhost:5001/api/health

---

**Chúc bạn triển khai thành công! 🎉**
