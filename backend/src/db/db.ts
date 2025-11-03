import {config} from "../config/config";
import { Pool } from 'pg';

const pool = new Pool({
    host: config.db.host,
    port: config.db.port,
    database: config.db.database,
    user: config.db.user,
    password: config.db.password,
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
