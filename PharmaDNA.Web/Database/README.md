# Database Setup for PharmaDNA

## Overview
This directory contains SQL scripts for initializing and managing the PharmaDNA PostgreSQL database.

## Files

### init_database.sql
Complete database initialization script that creates all necessary tables:
- `users` - User management and role assignment
- `nfts` - Drug batch NFTs
- `transfer_requests` - Transfer requests between actors
- `milestones` - Drug batch lifecycle tracking

### create_transfer_requests_table.sql
Individual script for creating the transfer_requests table (legacy).

## Setup Instructions

### Using Neon.tech (Recommended)

1. Create a PostgreSQL database on [Neon.tech](https://neon.tech)
2. Get the connection string from your Neon dashboard
3. Update the connection string in your `.env` file:
   ```env
   DATABASE_URL=postgresql://user:password@host/database
   ```

### Local PostgreSQL Setup

1. Install PostgreSQL locally
2. Create a database:
   ```sql
   CREATE DATABASE pharmadna;
   ```

3. Run the initialization script:
   ```bash
   psql -U postgres -d pharmadna -f Database/init_database.sql
   ```

## Schema Details

### Tables

#### users
Stores user information and blockchain addresses.
- `id` (SERIAL PRIMARY KEY)
- `address` (VARCHAR(42) UNIQUE) - Ethereum address
- `role` (VARCHAR(50)) - User role (MANUFACTURER, DISTRIBUTOR, PHARMACY, ADMIN)
- `assigned_at` (TIMESTAMP) - When role was assigned

#### nfts
Stores drug batch NFTs and their metadata.
- `id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(255)) - Product name
- `batch_number` (VARCHAR(100) UNIQUE) - Unique batch identifier
- `status` (VARCHAR(50)) - Current status (CREATED, in_transit, in_pharmacy, DELIVERED)
- `manufacturer_address` (VARCHAR(42)) - Blockchain address of manufacturer
- `distributor_address` (VARCHAR(42)) - Blockchain address of distributor
- `pharmacy_address` (VARCHAR(42)) - Blockchain address of pharmacy
- `ipfs_hash` (VARCHAR(255)) - IPFS hash for metadata
- `image_url` (VARCHAR(500)) - Product image URL
- `description` (TEXT) - Product description
- `manufacture_date` (TIMESTAMP) - Manufacturing date
- `expiry_date` (TIMESTAMP) - Expiry date
- `created_at` (TIMESTAMP) - Creation timestamp

#### transfer_requests
Stores transfer requests between actors.
- `id` (SERIAL PRIMARY KEY)
- `nft_id` (INTEGER) - Foreign key to nfts table
- `distributor_address` (VARCHAR(42)) - Distributor requesting transfer
- `pharmacy_address` (VARCHAR(42)) - Target pharmacy
- `transfer_note` (TEXT) - Additional notes
- `status` (VARCHAR(20)) - Request status (pending, approved, rejected, completed)
- `created_at` (TIMESTAMP) - Creation timestamp
- `updated_at` (TIMESTAMP) - Last update timestamp

#### milestones
Tracks lifecycle milestones for each drug batch.
- `id` (SERIAL PRIMARY KEY)
- `nft_id` (INTEGER) - Foreign key to nfts table
- `type` (VARCHAR(50)) - Milestone type (Sản xuất, Vận chuyển, Kiểm tra chất lượng, Đã nhập kho, Bán hàng)
- `description` (TEXT) - Detailed description
- `location` (VARCHAR(255)) - Geographic location
- `timestamp` (TIMESTAMP) - When milestone occurred
- `actor_address` (VARCHAR(42)) - Blockchain address of actor

## Entity Framework Core

The application uses Entity Framework Core with Code-First approach. The database will be automatically created on first run if `EnsureCreated()` is used in `Program.cs`.

However, if you want to use migrations instead:

```bash
# Create a migration
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

## Backup and Restore

### Backup
```bash
pg_dump -U username -d pharmadna > backup.sql
```

### Restore
```bash
psql -U username -d pharmadna < backup.sql
```

## Testing

To reset the database for testing:

```sql
-- Drop all tables
DROP TABLE IF EXISTS milestones CASCADE;
DROP TABLE IF EXISTS transfer_requests CASCADE;
DROP TABLE IF EXISTS nfts CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Run init_database.sql again
```

