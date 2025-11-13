import dotenv from "dotenv";
import path from "path";

// Load .env from project root
dotenv.config({ path: path.resolve(__dirname, "../../.env") });

// Helper to get env vars safely
function getEnv(key: string, required = true): string {
    const value = process.env[key];
    if (required && (!value || value.trim() === "")) {
        throw new Error(`Missing required environment variable: ${key}`);
    }
    return value!;
}

// Expose a read-only config object
export const config = Object.freeze({
    server:{
        address : getEnv("SERVER_ADDRESS"),
        port : parseInt(getEnv("SERVER_PORT")),
    },
    db: {
        host: getEnv("DB_HOST"),
        port: parseInt(getEnv("DB_PORT")),
        database: getEnv("DB_DATABASE"),
        user: getEnv("DB_USER"),
        password: getEnv("DB_PASSWORD"),
    },
    jwt: {
        secret: getEnv("JWT_SECRET"),
    },
    shop:{
        defaultName: getEnv("SHOP_NAME", false) ?? "Trafik",
    }
});
