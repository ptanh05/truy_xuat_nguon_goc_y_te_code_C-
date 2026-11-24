# THÔNG TIN CHI TIẾT ĐỀ ÁN: PHARMADNA - HỆ THỐNG TRUY XUẤT NGUỒN GỐC THUỐC

## 📋 TỔNG QUAN DỰ ÁN

**PharmaDNA** là nền tảng truy xuất nguồn gốc thuốc sử dụng công nghệ **NFT (ERC-721) + Blockchain + IoT** để đảm bảo tính minh bạch, an toàn và có thể truy vết toàn bộ chuỗi cung ứng dược phẩm từ nhà sản xuất đến người tiêu dùng cuối.

### Mục tiêu chính
- Đảm bảo tính xác thực và minh bạch của thuốc trong chuỗi cung ứng
- Ngăn chặn thuốc giả, thuốc kém chất lượng
- Cung cấp thông tin đầy đủ về nguồn gốc, lịch sử vận chuyển và điều kiện bảo quản
- Cho phép người dùng cuối tra cứu thông tin công khai mà không cần ví blockchain

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

### Cấu trúc Monorepo
```
truy_xuat_nguon_goc_y_te_code_C-/
├── client/              # Frontend: Next.js 14 + React 18
├── server/              # Backend: ASP.NET Core 9 API
├── saga-contract/       # Smart Contracts: Hardhat + Solidity
└── deploy/              # Docker & deployment scripts
```

### Công nghệ sử dụng

#### 1. **Frontend (Client)**
- **Framework**: Next.js 14 (App Router)
- **UI Library**: React 18, Tailwind CSS, shadcn/ui
- **Blockchain**: ethers.js v6, MetaMask integration
- **QR Code**: html5-qrcode
- **State Management**: React Hooks (useWallet, useAdminAuth, useRoleAuth)
- **Theming**: next-themes (dark/light mode)

#### 2. **Backend (Server)**
- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL (EF Core 9 + Npgsql)
- **Blockchain**: Nethereum.Web3 (kết nối blockchain)
- **IPFS**: Pinata API (lưu trữ metadata)
- **API Documentation**: Swagger/Swashbuckle

#### 3. **Smart Contracts**
- **Platform**: Hardhat
- **Language**: Solidity ^0.8.20
- **Standard**: ERC-721 (NFT) với extensions
- **Libraries**: OpenZeppelin Contracts v5.4.0
- **Blockchain**: Saga Chainlet (custom network)

---

## 🔐 MÔ HÌNH VAI TRÒ (ROLES)

Hệ thống quản lý 4 vai trò chính:

1. **Admin**
   - Cấp quyền cho các ví (Manufacturer, Distributor, Pharmacy)
   - Quản lý toàn bộ hệ thống
   - Có thể pause/unpause contract

2. **Manufacturer (Nhà sản xuất)**
   - Upload metadata và giấy tờ lên IPFS
   - Mint NFT cho từng lô thuốc
   - Chuyển NFT cho Distributor

3. **Distributor (Nhà phân phối)**
   - Nhận NFT từ Manufacturer
   - Xác nhận đã nhận hàng
   - Cập nhật milestone và sensor logs (nhiệt độ, độ ẩm)
   - Chuyển NFT cho Pharmacy

4. **Pharmacy (Nhà thuốc)**
   - Quét QR code hoặc nhập batch number
   - Xác nhận nhập kho
   - Xem lịch sử và sensor logs

5. **Người dùng công khai**
   - Tra cứu thông tin thuốc qua QR/batch number
   - Không cần ví blockchain
   - Xem metadata, lịch sử, sensor logs

---

## 📦 CHỨC NĂNG CHÍNH

### 1. Quản lý NFT và Metadata
- **Mint NFT**: Mỗi lô thuốc tương ứng với một NFT ERC-721 duy nhất
- **IPFS Storage**: Metadata (hình ảnh, chứng nhận, thông tin sản phẩm) được lưu trên IPFS qua Pinata
- **Token URI**: Liên kết NFT với metadata trên IPFS

### 2. Chuỗi cung ứng (Supply Chain)
- **Transfer Flow**: Manufacturer → Distributor → Pharmacy
- **Transfer Requests**: Hệ thống yêu cầu và phê duyệt chuyển NFT
- **Product History**: Lưu trữ toàn bộ lịch sử chuyển giao trên blockchain

### 3. Tracking & Monitoring
- **Milestones**: Theo dõi các mốc quan trọng trong chuỗi cung ứng
- **Sensor Logs**: Ghi nhận dữ liệu IoT (nhiệt độ, độ ẩm) trong quá trình vận chuyển
- **Status Tracking**: Trạng thái sản phẩm tại từng giai đoạn

### 4. Tra cứu công khai
- **QR Code Scanner**: Quét mã QR trên bao bì
- **Batch Number Lookup**: Tra cứu theo số lô
- **Product Name Search**: Tìm kiếm theo tên sản phẩm
- **Public Access**: Không yêu cầu ví blockchain

### 5. Quản lý quyền
- **Role-based Access Control**: Phân quyền theo vai trò
- **Admin Dashboard**: Giao diện quản trị để cấp/xóa quyền
- **Blockchain Verification**: Xác thực quyền trên smart contract

---

## 🗄️ CẤU TRÚC DATABASE

### Các bảng chính (PostgreSQL)

1. **SanPhamNFT (NFT)**
   - Thông tin sản phẩm: Name, BatchNumber, GTIN
   - Ngày sản xuất, hết hạn
   - URLs: ImageUrl, CertificateUrl, IpfsHash
   - Blockchain: TxHash, ManufacturerAddress, DistributorAddress, PharmacyAddress
   - Status: Trạng thái hiện tại

2. **TransferRequest**
   - Yêu cầu chuyển NFT giữa các bên
   - Status: Pending, Approved, Denied
   - From/To addresses

3. **Milestone**
   - Các mốc quan trọng trong chuỗi cung ứng
   - Liên kết với NFT

4. **SensorLog**
   - Dữ liệu cảm biến IoT
   - Nhiệt độ, độ ẩm, timestamp
   - Liên kết với NFT

5. **User**
   - Thông tin người dùng
   - Wallet address, role

---

## 🔧 SMART CONTRACT: PharmaNFT.sol

### Chức năng chính

#### Role Management
```solidity
- assignRole(address user, Role role): Cấp quyền
- revokeRole(address user): Thu hồi quyền
- batchAssignRoles(): Cấp quyền hàng loạt
- hasRole(): Kiểm tra quyền
- getRole(): Lấy vai trò
```

#### NFT Operations
```solidity
- mintProductNFT(string uri): Tạo NFT mới (chỉ Manufacturer)
- transferProductNFT(uint256 tokenId, address to): Chuyển NFT
- getProductHistory(uint256 tokenId): Lấy lịch sử
- getProductCurrentOwner(uint256 tokenId): Lấy chủ sở hữu hiện tại
```

#### Security Features
```solidity
- Pausable: Tạm dừng contract khi cần
- Role-based modifiers: Kiểm tra quyền trước khi thực thi
- Token owner verification: Xác thực chủ sở hữu
```

### Events
- `RoleAssigned`: Khi cấp quyền
- `RoleRevoked`: Khi thu hồi quyền
- `ProductMinted`: Khi mint NFT
- `ProductTransferred`: Khi chuyển NFT

---

## 🌐 API ENDPOINTS

### Manufacturer
- `POST /api/manufacturer/upload-ipfs`: Upload metadata lên IPFS
- `POST /api/manufacturer/mint`: Mint NFT (gọi từ frontend)
- `POST /api/manufacturer/transfer-request`: Tạo yêu cầu chuyển
- `PUT /api/manufacturer/transfer-request`: Phê duyệt/từ chối

### Distributor
- `POST /api/distributor/confirm-receipt`: Xác nhận đã nhận
- `POST /api/distributor/transfer-to-pharmacy`: Chuyển cho Pharmacy
- `POST /api/distributor/upload-sensor`: Upload sensor logs
- `GET /api/distributor`: Danh sách NFT đã nhận

### Pharmacy
- `GET /api/pharmacy`: Danh sách NFT trong kho
- `PUT /api/pharmacy`: Cập nhật trạng thái
- `GET /api/pharmacy/lookup`: Tra cứu theo batch/QR

### Admin
- `POST /api/admin`: Cấp quyền
- `DELETE /api/admin`: Xóa quyền
- `GET /api/admin/users`: Danh sách người dùng

### IPFS
- `POST /api/ipfs/upload`: Upload file lên Pinata
- `GET /api/ipfs/{hash}`: Lấy metadata từ IPFS

---

## 📱 GIAO DIỆN NGƯỜI DÙNG

### Các trang chính

1. **Dashboard (`/`)**
   - Tổng quan hệ thống
   - Lối tắt theo vai trò
   - Thống kê nhanh

2. **Manufacturer (`/manufacturer`)**
   - Form upload sản phẩm
   - Upload hình ảnh, chứng nhận
   - Mint NFT
   - Quản lý lô thuốc

3. **Distributor (`/distributor`)**
   - Danh sách lô đã nhận
   - Xác nhận receipt
   - Upload sensor logs (CSV)
   - Gửi yêu cầu chuyển cho Pharmacy

4. **Pharmacy (`/pharmacy`)**
   - Quét QR code
   - Xem metadata sản phẩm
   - Xác nhận nhập kho
   - Xem milestone và sensor logs

5. **Lookup (`/lookup`)**
   - Tra cứu công khai
   - Quét QR hoặc nhập batch number
   - Hiển thị thông tin đầy đủ

6. **Admin (`/admin`)**
   - Đăng nhập admin
   - Dashboard quản trị
   - Cấp/xóa quyền cho ví

---

## 🔐 BẢO MẬT

### Các biện pháp bảo mật

1. **Blockchain Security**
   - Role-based access control trên smart contract
   - Pausable contract để xử lý sự cố
   - Owner verification cho các thao tác quan trọng

2. **API Security**
   - CORS configuration
   - Environment variables cho sensitive data
   - Role verification từ blockchain

3. **Data Security**
   - Private keys không được commit
   - JWT/PINATA keys trong .env
   - HTTPS cho production

4. **Frontend Security**
   - MetaMask integration an toàn
   - Role guards cho các trang
   - Input validation

---

## 🚀 QUY TRÌNH TRIỂN KHAI

### Yêu cầu môi trường

| Thành phần | Phiên bản |
|-----------|-----------|
| Node.js | >= 18.x |
| npm | >= 10 |
| .NET SDK | 9.0.x |
| PostgreSQL | 14+ (hoặc Neon.tech) |
| MetaMask | Latest |
| Pinata | API key/JWT |

### Các bước thiết lập

1. **Clone repository**
   ```bash
   git clone <repo-url>
   cd truy_xuat_nguon_goc_y_te_code_C-
   ```

2. **Cài đặt dependencies**
   ```bash
   # Frontend
   cd client && npm install
   
   # Smart Contract
   cd ../saga-contract && npm install
   ```

3. **Cấu hình environment**
   - `client/.env`: Frontend config
   - `server/PharmaDNAServer/.env`: Backend config
   - `saga-contract/.env`: Blockchain config

4. **Deploy smart contract**
   ```bash
   cd saga-contract
   npx hardhat compile
   npx hardhat run scripts/deployPharmaNFT.ts --network pharmadna
   ```

5. **Setup database**
   ```bash
   cd server/PharmaDNAServer
   dotnet ef database update
   ```

6. **Chạy ứng dụng**
   ```bash
   # Backend
   cd server/PharmaDNAServer
   dotnet run
   
   # Frontend
   cd client
   npm run dev
   ```

---

## 📊 LUỒNG HOẠT ĐỘNG

### Luồng cơ bản

1. **Admin cấp quyền**
   - Admin đăng nhập tại `/admin`
   - Cấp quyền Manufacturer cho ví A
   - Cấp quyền Distributor cho ví B
   - Cấp quyền Pharmacy cho ví C

2. **Manufacturer tạo sản phẩm**
   - Vào `/manufacturer`
   - Upload metadata (hình ảnh, chứng nhận)
   - Hệ thống lưu lên IPFS
   - Ký giao dịch mint NFT trên blockchain

3. **Distributor nhận hàng**
   - Manufacturer tạo transfer request
   - Distributor xác nhận đã nhận
   - Cập nhật milestone
   - Upload sensor logs (nhiệt độ, độ ẩm)

4. **Pharmacy nhận hàng**
   - Distributor gửi yêu cầu chuyển
   - Pharmacy quét QR hoặc nhập batch number
   - Xác nhận nhập kho
   - Cập nhật status

5. **Người dùng tra cứu**
   - Vào `/lookup`
   - Quét QR code trên bao bì
   - Xem toàn bộ thông tin: metadata, lịch sử, sensor logs

---

## 🧪 TESTING & DEVELOPMENT

### Scripts có sẵn

**Frontend:**
- `npm run dev`: Development server
- `npm run build`: Build production
- `npm run start`: Start production server
- `npm run lint`: ESLint check

**Backend:**
- `dotnet run`: Run development
- `dotnet watch run`: Auto-reload
- `dotnet ef database update`: Run migrations

**Smart Contract:**
- `npx hardhat compile`: Compile contracts
- `npx hardhat test`: Run tests
- `npx hardhat run scripts/deployPharmaNFT.ts`: Deploy

---

## 📝 CHECKLIST TRIỂN KHAI PRODUCTION

- [ ] Thiết lập CORS_ORIGINS khớp domain thật
- [ ] Bật HTTPS & reverse proxy cho API
- [ ] Thêm rate limiting / API key cho public API
- [ ] Giám sát Pinata quota + lỗi chain
- [ ] Sao lưu database + thông tin contract address/private key
- [ ] Cấu hình monitoring và logging
- [ ] Setup CI/CD pipeline
- [ ] Security audit cho smart contract
- [ ] Load testing cho API
- [ ] Backup strategy cho IPFS data

---

## 🔗 TÀI LIỆU THAM KHẢO

- [README chính](README.md) - Overview và onboarding
- [Frontend README](client/README.md) - Chi tiết frontend
- [Backend README](server/README.md) - Chi tiết backend
- Smart Contract: Xem `saga-contract/contracts/PharmaNFT.sol`

---

## 💡 TÍNH NĂNG NỔI BẬT

1. **Blockchain + IPFS**: Kết hợp tính bất biến của blockchain với lưu trữ phi tập trung của IPFS
2. **IoT Integration**: Theo dõi điều kiện bảo quản trong quá trình vận chuyển
3. **Public Lookup**: Người dùng cuối tra cứu không cần ví blockchain
4. **Role-based System**: Phân quyền rõ ràng theo chuỗi cung ứng
5. **QR Code Support**: Dễ dàng tra cứu bằng quét mã QR
6. **Modern UI**: Giao diện đẹp với shadcn/ui, hỗ trợ dark/light mode

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra các file README con
2. Xem troubleshooting section trong từng README
3. Kiểm tra environment variables
4. Xem logs trong console/terminal
5. Tạo issue mới nếu cần

---

**Phiên bản**: 0.1.0  
**Ngày cập nhật**: 2025  
**Tác giả**: K64CNTT1-231220711-Phùng-Thế-Anh

---

> 💊 Chúc bạn triển khai thuận lợi! Nếu cần thêm thông tin chi tiết, vui lòng tham khảo các README con hoặc liên hệ team phát triển.

