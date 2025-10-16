import { Pool } from 'pg';

// Connection pool setup
const pool = new Pool({
    user: 'develop',
    host: '193.201.185.129',
    database: 'szivarclub',
    password: 'Szivar25',
    port: 5432,
});

// Test connection
(async () => {
    try {
        const client = await pool.connect();
        console.log('✅ Connected to PostgreSQL');
        client.release();
    } catch (err) {
        console.error('❌ Connection error', err);
    }
})();

export default pool;
