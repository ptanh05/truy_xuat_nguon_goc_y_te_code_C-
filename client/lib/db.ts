import { Pool } from "pg";

export const pool = new Pool({
  connectionString: process.env.DATABASE_URL, // từ Neon
  ssl: {
    rejectUnauthorized: false,
  },
});
