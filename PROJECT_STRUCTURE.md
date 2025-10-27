# PharmaDNA Project Structure

## Overview
Documentation về cấu trúc project và các thành phần chính của hệ thống.

## Directory Structure

```
truy_xuat_nguon_goc_y_te_code_C-/
│
├── PharmaDNA.Web/                 # Main ASP.NET Core MVC Web Application
│   ├── Controllers/              # MVC Controllers
│   │   ├── HomeController.cs     # Landing page controller
│   │   ├── ManufacturerController.cs      # Manufacturer role controller
│   │   ├── DistributorController.cs        # Distributor role controller
│   │   ├── PharmacyController.cs          # Pharmacy role controller
│   │   ├── LookupController.cs            # Public lookup controller
│   │   └── AdminController.cs             # Admin management controller
│   │
│   ├── Services/                  # Business Logic Layer
│   │   ├── BlockchainService.cs  # Blockchain interactions (Nethereum)
│   │   ├── IPFSService.cs        # IPFS uploads (Pinata API)
│   │   ├── NFTService.cs         # NFT database operations (Entity Framework)
│   │   └── UserService.cs         # User management operations
│   │
│   ├── Models/                    # Data Models
│   │   ├── Entities/             # Entity Framework models
│   │   │   ├── NFT.cs            # NFT entity (drug batches)
│   │   │   ├── User.cs           # User entity
│   │   │   ├── TransferRequest.cs
│   │   │   └── Milestone.cs      # Drug lifecycle milestones
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   │   ├── NFTDto.cs
│   │   │   ├── UserDto.cs
│   │   │   └── TransferRequestDto.cs
│   │   └── ViewModels/            # View Models for Views
│   │       ├── ManufacturerViewModel.cs
│   │       ├── DistributorViewModel.cs
│   │       ├── PharmacyViewModel.cs
│   │       ├── LookupViewModel.cs
│   │       └── AdminViewModel.cs
│   │
│   ├── Views/                     # Razor Views
│   │   ├── Home/                  # Landing page views
│   │   ├── Manufacturer/         # Manufacturer views
│   │   ├── Distributor/          # Distributor views
│   │   ├── Pharmacy/              # Pharmacy views
│   │   ├── Lookup/                # Public lookup views
│   │   ├── Admin/                 # Admin views
│   │   └── Shared/               # Shared layouts
│   │       └── _Layout.cshtml     # Main layout
│   │
│   ├── Data/                      # Database Context
│   │   └── ApplicationDbContext.cs  # EF Core DbContext
│   │
│   ├── Database/                  # Database Scripts
│   │   ├── init_database.sql      # Complete database initialization
│   │   ├── create_transfer_requests_table.sql
│   │   └── README.md              # Database documentation
│   │
│   ├── wwwroot/                   # Static Files
│   │   ├── contracts/            # Smart contract ABIs
│   │   │   ├── PharmaNFT.json
│   │   │   └── pharmaNFT-abi.json
│   │   ├── css/                  # Stylesheets
│   │   │   ├── input.css         # Tailwind CSS source
│   │   │   └── site.css          # Compiled CSS
│   │   ├── js/                   # JavaScript files
│   │   │   └── site.js           # Site-wide JS
│   │   └── images/               # Image assets
│   │
│   ├── Program.cs                 # Application entry point
│   ├── PharmaDNA.Web.csproj      # Project file
│   ├── appsettings.json          # Configuration
│   ├── package.json              # Node.js dependencies (Tailwind)
│   ├── tailwind.config.js        # Tailwind configuration
│   ├── run.bat                   # Quick start script
│   └── .gitignore                # Git ignore rules
│
├── saga-contract/                 # Smart Contract Project
│   ├── contracts/                # Solidity contracts
│   │   ├── PharmaNFT.sol         # Main NFT contract
│   │   └── PharmaDNA.sol         # Helper contracts
│   ├── scripts/                  # Deployment scripts
│   │   └── deployPharmaNFT.ts
│   ├── hardhat.config.ts         # Hardhat configuration
│   ├── deploy-pharmadna.bat      # Deploy script
│   ├── package.json
│   └── tsconfig.json
│
├── README.md                      # Main documentation
├── SETUP.md                       # Setup instructions
├── PROJECT_STRUCTURE.md          # This file
├── env.example                   # Environment variables template
└── cleanup.ps1                   # Cleanup script (PowerShell)
```

## Key Components

### 1. Controllers

Mỗi controller xử lý logic cho một role/actor khác nhau:

- **HomeController**: Trang chủ, giới thiệu hệ thống
- **ManufacturerController**: Tạo NFT, upload metadata, duyệt transfer requests
- **DistributorController**: Quản lý vận chuyển, upload sensor data
- **PharmacyController**: Tra cứu thuốc, xác nhận nhập kho
- **LookupController**: Tra cứu công khai (không cần đăng nhập)
- **AdminController**: Quản lý users, phân quyền

### 2. Services

Các service layer xử lý business logic:

- **BlockchainService**: Tương tác với smart contract qua Nethereum
- **IPFSService**: Upload files và metadata lên IPFS qua Pinata API
- **NFTService**: CRUD operations cho NFTs trong database
- **UserService**: User management và role assignment

### 3. Models

#### Entities (Database)
- `NFT`: Đại diện cho một lô thuốc trong database
- `User`: Người dùng và vai trò của họ
- `TransferRequest`: Yêu cầu chuyển lô giữa các actors
- `Milestone`: Các cột mốc trong vòng đời sản phẩm

#### DTOs (Data Transfer Objects)
Dùng để transfer data giữa layers mà không expose internal details.

#### ViewModels
Data structures cho Views, bao gồm validation attributes.

### 4. Database Schema

```
users
├── id (PK)
├── address (UNIQUE, 42 chars - Ethereum address)
├── role (VARCHAR)
└── assigned_at (TIMESTAMP)

nfts
├── id (PK)
├── batch_number (UNIQUE)
├── status (VARCHAR)
├── manufacturer_address
├── distributor_address
├── pharmacy_address
├── ipfs_hash
├── image_url
├── description
├── manufacture_date
├── expiry_date
└── created_at

transfer_requests
├── id (PK)
├── nft_id (FK)
├── distributor_address
├── pharmacy_address
├── transfer_note
├── status
├── created_at
└── updated_at

milestones
├── id (PK)
├── nft_id (FK)
├── type (VARCHAR)
├── description
├── location
├── timestamp
└── actor_address
```

### 5. Smart Contract (PharmaNFT.sol)

**Features:**
- ERC721 NFT standard với URI storage
- Role-based access control (MANUFACTURER, DISTRIBUTOR, PHARMACY, ADMIN)
- Product lifecycle tracking
- Transfer with business rules
- Pausable functionality

**Key Functions:**
- `mintProductNFT(string uri)`: Mint new NFT
- `transferProductNFT(uint256 tokenId, address to)`: Transfer NFT
- `assignRole(address user, Role role)`: Assign user role
- `getProductHistory(uint256 tokenId)`: Get ownership history
- `getProductCurrentOwner(uint256 tokenId)`: Get current owner

### 6. Workflow

```
1. MANUFACTURER creates drug batch → Creates NFT + uploads metadata to IPFS
   ↓
2. DISTRIBUTOR receives NFT → Uploads sensor data → Creates transfer request
   ↓
3. MANUFACTURER approves transfer request → NFT ownership transferred
   ↓
4. PHARMACY receives transfer notification → Confirms received
   ↓
5. PUBLIC can lookup drug by batch number (no wallet needed)
```

### 7. Technology Stack

**Backend:**
- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core
- Nethereum (blockchain interactions)

**Frontend:**
- Razor Views (server-side rendering)
- Bootstrap CSS framework
- Tailwind CSS (utility-first)
- JavaScript (client-side logic)
- Alpine.js (lightweight JS framework)

**Blockchain:**
- Solidity
- OpenZeppelin contracts
- Hardhat
- Saga Chainlet

**Storage:**
- PostgreSQL (Neon.tech)
- IPFS (Pinata)
- Blockchain (Saga Chainlet)

### 8. Configuration Files

- `appsettings.json`: Base configuration
- `.env`: Environment-specific configuration (not in git)
- `Program.cs`: Application bootstrap và dependency injection
- `PharmaDNA.Web.csproj`: Project file và NuGet packages
- `package.json`: Node.js dependencies
- `tailwind.config.js`: Tailwind CSS configuration

## Development Workflow

1. **Local Development**:
   - Setup PostgreSQL (Neon.tech hoặc local)
   - Configure `.env` file
   - Run `dotnet restore` và `npm install`
   - Run `dotnet watch run` cho hot reload

2. **Smart Contract Deployment**:
   - Go to `saga-contract` directory
   - Run `deploy-pharmadna.bat`
   - Update `PHARMA_NFT_ADDRESS` in `.env`

3. **Database Migration**:
   - EF Core auto-creates database on first run
   - Or manually run `init_database.sql`

## Testing

Structure your tests theo pattern:
```
├── Tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── E2ETests/
```

## Build & Deploy

```bash
# Development
dotnet run

# Production build
dotnet publish -c Release -o ./publish

# Run production
dotnet ./publish/PharmaDNA.Web.dll
```

## Questions?

Xem thêm:
- `README.md` - Main documentation
- `SETUP.md` - Setup instructions  
- `Database/README.md` - Database documentation

