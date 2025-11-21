# PharmaDNA Monorepo

PharmaDNA là nền tảng truy xuất nguồn gốc thuốc sử dụng NFT + IoT. Repo này gom cả **frontend (Next.js)**, **backend (ASP.NET Core)** và **smart contract Hardhat**, kèm quy trình triển khai đầy đủ.

## Tóm tắt tính năng
- Mỗi lô thuốc tương ứng một NFT ERC‑721 với metadata trên IPFS.
- Chuỗi cung ứng nhiều vai trò: Manufacturer → Distributor → Pharmacy → Người dùng cuối.
- Upload metadata + giấy tờ lên Pinata, lưu trạng thái lô thuốc trong PostgreSQL.
- Tracking milestone + sensor log (nhiệt độ, độ ẩm) theo từng NFT.
- Tra cứu công khai qua QR / batch number, tích hợp MetaMask + Saga Chainlet.

## Cấu trúc thư mục
```
.
├── client/            # Next.js 14 + Tailwind + shadcn/ui (FE)
│   └── README.md      # Tài liệu chi tiết cho frontend
├── server/
│   └── PharmaDNAServer/   # ASP.NET Core 9 API + EF Core
│       └── README.md      # Tài liệu chi tiết cho backend
├── saga-contract/     # Hardhat project (PharmaNFT.sol)
└── README.md          # (file hiện tại) Overview + onboarding
```

## Chuẩn bị môi trường
| Thành phần | Phiên bản gợi ý |
|-----------|-----------------|
| Node.js   | >= 18.x |
| npm       | v10 (hoặc tương thích Node 18) |
| .NET SDK  | 9.0.x |
| PostgreSQL| Neon.tech (cloud) hoặc local 14+ |
| MetaMask  | Latest (thêm custom network Saga) |
| Pinata    | API key/JWT để upload IPFS |

## Quy trình thiết lập nhanh
1. **Clone & cài đặt**  
   ```bash
   git clone <repo-url>
   cd truy_xuat_nguon_goc_y_te_code_C-
   npm install              # cài root lock nếu cần
   cd client && npm install
   cd ../saga-contract && npm install
   ```

2. **Cấu hình environment**
   - `client/.env`: tham khảo mẫu ở `client/README.md`.
   - `server/PharmaDNAServer/.env`: tham khảo mẫu ở `server/README.md`.
   - `saga-contract/.env`: thêm (tự thay bằng giá trị của chain bạn dùng):
     ```
     PHARMADNA_RPC=<RPC_URL_CUA_CHAIN>
     PHARMADNA_CHAIN_ID=<CHAIN_ID_DECIMAL>
     DEPLOYER_PRIVATE_KEY=<PRIVATE_KEY_DEPLOYER>
     ```
   - Luôn đồng bộ `NEXT_PUBLIC_PHARMA_NFT_ADDRESS` (FE) & `PHARMA_NFT_ADDRESS` (BE) với địa chỉ contract bạn vừa deploy (không commit giá trị thật).

3. **Triển khai smart contract (tuỳ chọn)**  
   ```bash
   cd saga-contract
   npx hardhat compile
   npx hardhat run scripts/deployPharmaNFT.ts --network pharmadna
   ```
   Hoặc chạy `deploy-pharmadna.bat` (Windows) để tự động compile ↔ deploy.  
   - Chainlet Saga ví dụ: cung cấp RPC, chain ID, explorer theo tài liệu nội bộ; đừng commit chi tiết nhạy cảm.  
   - Mỗi lần deploy xong nhớ cập nhật lại địa chỉ contract ở các file `.env`.

4. **Chuẩn bị database**  
   - Tạo database trên PostgreSQL (Neon/local).  
   - Chạy migration:  
     ```bash
     cd server/PharmaDNAServer
     dotnet ef database update
     ```

5. **Chạy backend**  
   ```bash
   cd server/PharmaDNAServer
   dotnet run
   # API mặc định: http://localhost:5196 (Swagger /api)
   ```

6. **Chạy frontend**  
   ```bash
   cd client
   npm run dev
   # UI: http://localhost:3000
   ```

7. **Kết nối MetaMask**  
   - Thêm custom network Saga Chainlet.  
   - Import private key cho tài khoản owner (dùng để mint & cấp quyền).  
   - Admin đăng nhập với thông tin trong `.env`, cấp role cho ví Manufacturer/Distributor/Pharmacy.

## Hướng dẫn cho người dùng mới

| Vai trò | Việc cần làm |
|---------|--------------|
| **Admin** | Đăng nhập tại `/admin`, cấp quyền cho ví (Manufacturer/Distributor/Pharmacy). |
| **Manufacturer** | Vào `/manufacturer`, upload metadata + giấy tờ → hệ thống lưu IPFS + DB, sau đó ký giao dịch mint NFT. |
| **Distributor** | `/distributor`: nhận NFT đã mint, confirm receipt, cập nhật milestone, gửi yêu cầu chuyển đến Pharmacy. |
| **Pharmacy** | `/pharmacy`: quét QR hoặc nhập batch, xác nhận nhập kho, xem log sensor/milestone. |
| **Người dùng công khai** | `/lookup`: tra cứu thông tin bằng QR/batch/name mà không cần ví. |

### Luồng tham khảo
1. Admin cấp quyền cho Manufacturer (ví A).  
2. Manufacturer upload lô thuốc, ký giao dịch mint.  
3. Distributor yêu cầu nhận lô, Manufacturer duyệt → Distributor xác nhận đã nhận.  
4. Distributor chuyển cho Pharmacy, Pharmacy xác nhận nhập kho.  
5. Người dùng quét QR xem lịch sử + cảm biến.

## Tài liệu chi tiết
- [Frontend README](client/README.md) – scripts, kiến trúc, hướng dẫn UI.
- [Backend README](server/README.md) – environment, migration, API map.
- `saga-contract/` – sử dụng Hardhat chuẩn, xem `scripts/deployPharmaNFT.ts`.

## Checklist triển khai Production
- [ ] Thiết lập `CORS_ORIGINS` khớp domain thật.
- [ ] Bật HTTPS & reverse proxy cho API.
- [ ] Thêm rate limiting / API key nếu cần public API.
- [ ] Giám sát Pinata quota + lỗi chain.
- [ ] Sao lưu database + thông tin contract address/private key.

---
> Nếu cần thêm hướng dẫn chi tiết từng phần, đọc các README con hoặc tạo issue mới. Chúc bạn triển khai thuận lợi! 💊

