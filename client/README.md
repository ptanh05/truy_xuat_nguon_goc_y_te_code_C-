# Frontend (client/)

Next.js 14 + React 18 + Tailwind CSS + shadcn/ui cung cấp giao diện cho mọi vai trò của PharmaDNA. Đây là Single Page App chạy hoàn toàn trên trình duyệt với MetaMask + ethers v6.

## 1. Thành phần chính
- **Next.js App Router** (`app/`) cho từng trang: manufacturer, distributor, pharmacy, admin, lookup.
- **shadcn/ui** + Tailwind cho UI kit thống nhất, dark/light mode với `next-themes`.
- **ethers.js** để kết nối MetaMask, ký giao dịch mint/tracking NFT.
- **html5-qrcode** để quét QR từ camera / file.
- **Custom hooks**: `useWallet`, `useAdminAuth`, `useRoleAuth`.

## 2. Yêu cầu
| Công cụ | Phiên bản khuyến nghị |
|---------|-----------------------|
| Node.js | >= 18.x |
| npm     | >= 10 |
| MetaMask| Latest (đã add Saga chain) |

## 3. Cài đặt & chạy
```bash
cd client
npm install
npm run dev        # http://localhost:3000
```

Scripts khác:
| Lệnh | Mục đích |
|------|----------|
| `npm run build` | Build production (tạo `.next/`) |
| `npm run start` | Start server production |
| `npm run lint`  | Kiểm tra ESLint |

## 4. Environment Variables
Tạo `client/.env`:
```
NEXT_PUBLIC_API_URL=http://localhost:5196/api
NEXT_PUBLIC_PHARMA_NFT_ADDRESS=<CONTRACT_ADDRESS_SAU_DEPLOY>
NEXT_PUBLIC_PINATA_JWT=<PINATA_JWT>
NEXT_PUBLIC_PINATA_API_URL=https://api.pinata.cloud
NEXT_PUBLIC_PINATA_GATEWAY=https://gateway.pinata.cloud/ipfs/
NEXT_PUBLIC_ADMIN_USERNAME=<ADMIN_USER_DEV>
NEXT_PUBLIC_ADMIN_PASSWORD=<ADMIN_PASS_DEV>
NEXT_PUBLIC_PHARMADNA_RPC=<RPC_URL_CUA_CHAIN>
```

> Tất cả biến đều dùng prefix `NEXT_PUBLIC_` vì cần truy cập ở client. Đừng commit giá trị thật.

## 5. Cấu trúc đáng chú ý
```
client/
├── app/
│   ├── layout.tsx        # Theme + global Toaster
│   ├── manufacturer/     # Upload IPFS + mint NFT
│   ├── distributor/      # Nhận lô, sensor logs, chuyển tới pharmacy
│   ├── pharmacy/         # Quét QR + xác nhận nhập kho
│   ├── lookup/           # Tra cứu công khai
│   └── admin/            # Admin dashboard cấp quyền
├── components/
│   ├── QRScanner.tsx     # html5-qrcode integration
│   ├── TransferToPharmacyForm.tsx
│   ├── PharmacyTransferRequests.tsx
│   └── ui/               # shadcn/ui build
├── hooks/
│   ├── useWallet.ts      # MetaMask connection
│   ├── useAdminAuth.ts   # Simple credential gate
│   └── useRoleAuth.ts    # FE role guard
└── lib/
    ├── api.ts            # Fetch helper with error surface
    ├── pinata.ts         # Pinata helper (cho FE-only flows)
    └── pharmaNFT-abi.json
```

## 6. Chức năng từng trang
| Trang | Mô tả |
|-------|------|
| `/` (Dashboard) | Tóm tắt nhanh + lối tắt theo vai trò |
| `/manufacturer` | Form upload hình ảnh, chứng nhận → call API `/api/manufacturer/upload-ipfs`, sau đó ký `mintProductNFT` |
| `/distributor` | Danh sách lô sẵn sàng, confirm receipt, upload sensor CSV, gửi yêu cầu chuyển tới pharmacy |
| `/pharmacy` | Quét QR, xem metadata, xác nhận nhập kho, reload milestone |
| `/lookup` | Công khai, không cần ví. Tra cứu theo tên/batch/QR |
| `/admin` | Đăng nhập bằng thông tin `.env`, cấp/xoá quyền (gọi API `/api/admin`) |

## 7. Làm việc với MetaMask & Saga chain
1. Mở MetaMask → Settings → Networks → Add network manually.
2. Điền thông tin chain bạn đang sử dụng (ví dụ Saga chainlet nội bộ):
   - Network name: `PharmaDNAVN Saga`
   - RPC URL: `<RPC_URL_CUA_CHAIN>`
   - Chain ID: `<CHAIN_ID_DECIMAL>` (ví dụ `2763717455037000` tương ứng `0x9d1961d2ac248`)
   - Currency: `PDNA`
3. Import private key cho Owner/Manufacturer/Distributor/Pharmacy.
4. Đảm bảo `NEXT_PUBLIC_PHARMA_NFT_ADDRESS` trùng địa chỉ contract mới nhất (tự cập nhật sau mỗi lần deploy).

## 8. Linting, formatting & best practices
- ESLint cấu hình mặc định Next.js (`.eslintrc.json` implicit).  
- Tailwind + `clsx`/`cva` để gom class.  
- Toast (sonner) thay alert(). Console chỉ log error.

## 9. Ghi chú triển khai
- Khi build production, nhớ set `NEXT_PUBLIC_API_URL` trỏ domain HTTPS backend.
- Nếu deploy static hosting (Vercel), cần bật `NEXT_PUBLIC_PINATA_*` thông qua dashboard secrets.
- Admin credential chỉ nên dùng cho dev; production nên tích hợp backend auth/OAuth.

## 10. Troubleshooting
| Lỗi | Cách xử lý |
|-----|------------|
| `MetaMask: chain mismatch` | Kiểm tra đã switch network Saga chưa. |
| `API call failed 4xx/5xx` | Mở console log, xem chi tiết từ `api.ts` (đã in status + body). |
| Quét QR không mở camera | Trình duyệt chặn quyền camera → cấp lại trong settings. |
| Upload IPFS lỗi `PINATA_JWT` | Đảm bảo JWT đúng, còn quota, và backend `.env` cũng có PINATA_JWT để verify. |

---
👉 Tiếp tục xem [README backend](../server/README.md) để cấu hình API hoặc quay lại [README gốc](../README.md) cho overview toàn dự án.

