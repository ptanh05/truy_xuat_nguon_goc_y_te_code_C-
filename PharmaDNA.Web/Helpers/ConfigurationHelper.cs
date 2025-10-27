namespace PharmaDNA.Web.Helpers
{
    public static class ConfigurationHelper
    {
        public static class Blockchain
        {
            public const string RpcUrl = "PHARMADNA_RPC";
            public const string ContractAddress = "PHARMA_NFT_ADDRESS";
            public const string PrivateKey = "OWNER_PRIVATE_KEY";
        }

        public static class IPFS
        {
            public const string PinataJwt = "PINATA_JWT";
            public const string GatewayUrl = "PINATA_GATEWAY";
        }

        public static class Database
        {
            public const string ConnectionString = "DATABASE_URL";
        }

        public static class Roles
        {
            public const int None = 0;
            public const int Manufacturer = 1;
            public const int Distributor = 2;
            public const int Pharmacy = 3;
            public const int Admin = 4;
        }

        public static class NFTStatus
        {
            public const string Created = "CREATED";
            public const string InTransit = "in_transit";
            public const string InPharmacy = "in_pharmacy";
            public const string Delivered = "DELIVERED";
        }

        public static class TransferRequestStatus
        {
            public const string Pending = "pending";
            public const string Approved = "approved";
            public const string Rejected = "rejected";
            public const string Completed = "completed";
        }

        public static class MilestoneTypes
        {
            public const string Manufacturing = "Sản xuất";
            public const string Transportation = "Vận chuyển";
            public const string QualityCheck = "Kiểm tra chất lượng";
            public const string Received = "Đã nhập kho";
            public const string Sold = "Bán hàng";
            public const string ExpiryWarning = "Cảnh báo hết hạn";
        }

        public static string GetRoleName(int role)
        {
            return role switch
            {
                Roles.Manufacturer => "MANUFACTURER",
                Roles.Distributor => "DISTRIBUTOR",
                Roles.Pharmacy => "PHARMACY",
                Roles.Admin => "ADMIN",
                _ => "NONE"
            };
        }

        public static int GetRoleValue(string role)
        {
            return role.ToUpper() switch
            {
                "MANUFACTURER" => Roles.Manufacturer,
                "DISTRIBUTOR" => Roles.Distributor,
                "PHARMACY" => Roles.Pharmacy,
                "ADMIN" => Roles.Admin,
                _ => Roles.None
            };
        }

        public static bool IsValidRole(string role)
        {
            return GetRoleValue(role) != Roles.None;
        }

        public static bool IsValidStatus(string status)
        {
            return status switch
            {
                NFTStatus.Created or 
                NFTStatus.InTransit or 
                NFTStatus.InPharmacy or 
                NFTStatus.Delivered => true,
                _ => false
            };
        }

        public static bool IsValidTransferStatus(string status)
        {
            return status switch
            {
                TransferRequestStatus.Pending or
                TransferRequestStatus.Approved or
                TransferRequestStatus.Rejected or
                TransferRequestStatus.Completed => true,
                _ => false
            };
        }
    }
}
