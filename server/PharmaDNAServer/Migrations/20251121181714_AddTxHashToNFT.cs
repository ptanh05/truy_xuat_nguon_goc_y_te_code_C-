using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaDNAServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTxHashToNFT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if column already exists before adding
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'SanPhamNFT' 
                        AND column_name = 'TxHash'
                    ) THEN
                        ALTER TABLE ""SanPhamNFT"" ADD COLUMN ""TxHash"" text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TxHash",
                table: "SanPhamNFT");
        }
    }
}
