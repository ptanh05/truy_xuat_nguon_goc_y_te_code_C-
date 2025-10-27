-- PharmaDNA Database Initialization Script
-- Database: PostgreSQL
-- Description: This script creates all necessary tables for the PharmaDNA application

-- Create Users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    address VARCHAR(42) UNIQUE NOT NULL,
    role VARCHAR(50) NOT NULL,
    assigned_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT users_address_check CHECK (char_length(address) = 42)
);

-- Create NFTs table
CREATE TABLE IF NOT EXISTS nfts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    batch_number VARCHAR(100) NOT NULL UNIQUE,
    status VARCHAR(50) NOT NULL,
    manufacturer_address VARCHAR(42) NOT NULL,
    distributor_address VARCHAR(42),
    pharmacy_address VARCHAR(42),
    ipfs_hash VARCHAR(255),
    image_url VARCHAR(500),
    description TEXT,
    manufacture_date TIMESTAMP,
    expiry_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT nfts_batch_number_check CHECK (batch_number IS NOT NULL AND batch_number != ''),
    CONSTRAINT nfts_status_check CHECK (status IN ('CREATED', 'in_transit', 'in_pharmacy', 'DELIVERED'))
);

-- Create transfer_requests table
CREATE TABLE IF NOT EXISTS transfer_requests (
    id SERIAL PRIMARY KEY,
    nft_id INTEGER NOT NULL REFERENCES nfts(id) ON DELETE CASCADE,
    distributor_address VARCHAR(42) NOT NULL,
    pharmacy_address VARCHAR(42) NOT NULL,
    transfer_note TEXT,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT transfer_requests_status_check CHECK (status IN ('pending', 'approved', 'rejected', 'completed'))
);

-- Create milestones table
CREATE TABLE IF NOT EXISTS milestones (
    id SERIAL PRIMARY KEY,
    nft_id INTEGER NOT NULL REFERENCES nfts(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    description TEXT,
    location VARCHAR(255),
    timestamp TIMESTAMP DEFAULT NOW(),
    actor_address VARCHAR(42) NOT NULL,
    CONSTRAINT milestones_type_check CHECK (type IN (
        'Sản xuất', 
        'Vận chuyển', 
        'Kiểm tra chất lượng', 
        'Đã nhập kho', 
        'Bán hàng'
    ))
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_address ON users(address);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);

CREATE INDEX IF NOT EXISTS idx_nfts_batch_number ON nfts(batch_number);
CREATE INDEX IF NOT EXISTS idx_nfts_status ON nfts(status);
CREATE INDEX IF NOT EXISTS idx_nfts_manufacturer ON nfts(manufacturer_address);
CREATE INDEX IF NOT EXISTS idx_nfts_distributor ON nfts(distributor_address);
CREATE INDEX IF NOT EXISTS idx_nfts_pharmacy ON nfts(pharmacy_address);

CREATE INDEX IF NOT EXISTS idx_transfer_requests_nft ON transfer_requests(nft_id);
CREATE INDEX IF NOT EXISTS idx_transfer_requests_distributor ON transfer_requests(distributor_address);
CREATE INDEX IF NOT EXISTS idx_transfer_requests_pharmacy ON transfer_requests(pharmacy_address);
CREATE INDEX IF NOT EXISTS idx_transfer_requests_status ON transfer_requests(status);

CREATE INDEX IF NOT EXISTS idx_milestones_nft ON milestones(nft_id);
CREATE INDEX IF NOT EXISTS idx_milestones_type ON milestones(type);
CREATE INDEX IF NOT EXISTS idx_milestones_timestamp ON milestones(timestamp);
CREATE INDEX IF NOT EXISTS idx_milestones_actor ON milestones(actor_address);

-- Insert sample data (optional - for development)
INSERT INTO users (address, role, assigned_at) VALUES 
    ('0x1111111111111111111111111111111111111111', 'ADMIN', NOW()),
    ('0x2222222222222222222222222222222222222222', 'MANUFACTURER', NOW()),
    ('0x3333333333333333333333333333333333333333', 'DISTRIBUTOR', NOW()),
    ('0x4444444444444444444444444444444444444444', 'PHARMACY', NOW())
ON CONFLICT (address) DO NOTHING;

-- Comments for documentation
COMMENT ON TABLE users IS 'Users and their roles in the system';
COMMENT ON TABLE nfts IS 'NFTs representing drug batches';
COMMENT ON TABLE transfer_requests IS 'Transfer requests between actors';
COMMENT ON TABLE milestones IS 'Lifecycle milestones for each drug batch';

COMMENT ON COLUMN users.address IS 'Ethereum address (42 characters)';
COMMENT ON COLUMN nfts.batch_number IS 'Unique batch number for each drug batch';
COMMENT ON COLUMN nfts.ipfs_hash IS 'IPFS hash for metadata and files';

-- Grant permissions (adjust as needed)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_user;

