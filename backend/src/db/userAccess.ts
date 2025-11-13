import pool from './db';
import {User} from "../model/user";

async function registerUser(email: string, passwordHash: string, name: string): Promise<{alreadyExists : boolean, id : number}> {

    const existingUser = await getUserByEmail(email);
    if (existingUser !== null){
        return {
            alreadyExists : true,
            id : existingUser.id
        }
    }

    const query = `
        INSERT INTO users (name, email, password_hash, role_id)
        VALUES ($1, $2, $3, $4)
    `;

    const values = [name, email, passwordHash, 2];

    const result = await pool.query(query, values);
    return {
        alreadyExists : false,
        id : result.rows[0],
    }
}


async function getUserByEmail(email: string): Promise<null | User> {
    const query = `
        SELECT users.id, users.name, users.password_hash, roles.name
        FROM users
                 JOIN roles ON role_id = roles.id
        WHERE email = $1 LIMIT 1
    `;

    const values = [email];

    const result = await pool.query(query, values);

    if (result.rows.length == 0) {
        return null;
    }

    return {
        id: result.rows[0].id,
        email: email,
        name: result.rows[0].name,
        passwordHash: result.rows[0].password_hash,
        role: result.rows[0].name,
    }
}


export default {
    registerUser,
    getUserByEmail
}


//TODO log register