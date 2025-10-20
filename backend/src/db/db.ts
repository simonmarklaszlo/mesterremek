import { Pool } from 'pg';

const pool = new Pool({
    host: process.env.DB_HOST,
    port: parseInt(process.env.DB_PORT ?? function(){throw new Error("Missing DB_PORT from .env")}()),
    database: process.env.DB_DATABASE,
    user: process.env.DATABASE_USER,
    password: process.env.DATABASE_PASSWORD,
});

// Test connection
(async () => {
    try {
        const client = await pool.connect();
        console.log('Connected to PostgreSQL');
        client.release();
    } catch (err) {
        console.error(`Errod connecting to PostgreSQL : ${err}`);
    }
})();

export default pool;


/*
TODO:

Users   -   unique email

all     -   default for CREATED_AT, UPDATED_AT

CREATE INDEX idx_places_location ON places USING GIST (location);
*/
