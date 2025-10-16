import pool from './db';

async function registerUser(email: string, passwordHash: string, name: string): Promise<number> {
    const query = `
        INSERT INTO users (name, email, password_hash, role_id)
        VALUES ($1, $2, $3, $4)
    `;

    const values = [name, email, passwordHash, 2];

    const result = await pool.query(query, values);
    return result.rows[0];
}


async function loginUser(email: string, passwordHash: string): Promise<null | {id : number, role : string}> {
    const query = `
    SELECT users.id, roles.name 
    FROM users
    JOIN roles ON role_id = roles.id
    WHERE email = $1 AND password_hash = $2
    LIMIT 1
  `;

    const values = [email, passwordHash];

    const result = await pool.query(query, values);

    if(result.rows.length == 0) {
        return null;
    }

    return {
        id : result.rows[0],
        role : result.rows[1],
    }
}

async function getUserByEmail(email: string): Promise<null|  {id : number, name : string, pw_hash: string, role : string} > {
    const query = `
    SELECT users.id, users.name, users.password_hash, roles.name 
    FROM users
    JOIN roles ON role_id = roles.id
    WHERE email = $1
    LIMIT 1
  `;

    const values = [email];

    const result = await pool.query(query, values);

    if(result.rows.length == 0) {
        return null;
    }

    return {
        id : result.rows[0].id,
        name : result.rows[0].name,
        pw_hash : result.rows[0].password_hash,
        role : result.rows[0].name,
    }
}

export default {
    registerUser,
    loginUser,
    getUserByEmail
}