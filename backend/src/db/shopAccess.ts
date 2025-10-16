import pool from './db';
import {ShopCoords, ShopDetails, DaySchedule, getErrorCoords, getErrorDetails, getErrorSchedule} from "../model/shop";


async function getCities(): Promise<string[]> {
    const query = `
        SELECT DISTINCT city
        FROM shops
    `;

    try {
        const result = await pool.query<{ city: string }>(query);
        return result.rows.map(row => row.city);
    } catch (err) {
        console.error('Error querying cities:', err);
        return [];
    }
}

async function getShopCoordsByCity(city: string): Promise<{ id: number, coords: ShopCoords }[]> {
    const query = `
        SELECT id,
               ST_X(location) AS longitude,
               ST_Y(location) AS latitude
        FROM shops
        WHERE city = $1
    `;

    const values = [city];

    try {
        const result = await pool.query<{ id: number, longitude: number, latitude: number }>(query, values);

        return result.rows.map(row => ({
            id: row.id,
            coords: {
                longitude: row.longitude,
                latitude: row.latitude
            }
        }));

    } catch (err) {
        console.error('Error querying shops:', err);
        return [];
    }
}

async function getShopCoords(id: number): Promise<ShopCoords> {
    const query = `
        SELECT ST_X(location) AS longitude,
               ST_Y(location) AS latitude
        FROM shops
        WHERE id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ longitude: number, latitude: number }>(query, values);

        if (result.rows[0] == null) {
            return getErrorCoords();
        }

        return {
            longitude: result.rows[0].longitude,
            latitude: result.rows[0].latitude
        }
    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorCoords();
    }
}

async function getShopDetails(id: number): Promise<ShopDetails> {
    const query = `
        SELECT name,
               address
        FROM shops
        WHERE id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ name: string | null, address: string }>(query, values);

        if (result.rows[0] == null) {
            return getErrorDetails()
        }

        return {
            name: result.rows[0].name,
            address: result.rows[0].address
        }
    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorDetails()
    }
}

async function getShopSchedules(id: number): Promise<DaySchedule[]> {
    const query = `
        SELECT days_of_week.day, open_hour, close_hour
        FROM shop_opening_hours
                 JOIN days_of_week ON shop_opening_hours.day_id = days_of_week.id
        WHERE shop_opening_hours.shop_id = $1
    `;

    const values = [id];

    try {
        const result = await pool.query<{ day: string, open_hour: string, close_hour: string }>(query, values);

        return result.rows.map(row => ({
            dayOfWeek: row.day,
            opening: row.open_hour,
            closing: row.close_hour,
        }));

    } catch (err) {
        console.error('Error querying shops:', err);
        return getErrorSchedule();
    }
}

export default {
    getCities,
    getShopCoordsByCity,
    getShopCoords,
    getShopDetails,
    getShopSchedules
}