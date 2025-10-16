import { Router } from "express";
import userFunc from "../db/userAccess";
import bcrypt from "bcryptjs";
import jwt from 'jsonwebtoken';

const SECRET_KEY = 'your_secret_key_here';
const router = Router();

interface JwtPayload {
    userId: number;
    email: string;
    name: string;
    role: string;
    exp?: number; // optional, will be set automatically
}

router.post("/register", async (req, res) => {
    try {
        const { email, password, name } = req.body;

        if (!email || !password || !name) {
            return res.status(400).json({ error: "Missing required fields" });
        }

        const passwordHash = await bcrypt.hash(password, 12);
        console.log(passwordHash);
        const userId = await userFunc.registerUser(email, passwordHash, name);

        res.status(201).send({
            message: "Registration successful",
            userId,
            echo: { email, name }
        });
    } catch (err: any) {
        console.error(err);
        res.status(500).send({ message: "Registration failed", error: err.message });
    }
});

router.post("/login", async (req, res) => {
    try {
        const { email, password } = req.body;

        const user = await userFunc.getUserByEmail(email);
        if (!user) {
            return res.status(401).send({ message: "Invalid credentials" });
        }

        const isMatch = await bcrypt.compare(password, user.pw_hash);
        if(!isMatch) {
            return res.status(401).send({ message: "Invalid credentials" });
        }

        // Create JWT payload
        const payload: JwtPayload = {
            userId: user.id,
            email: email,
            name: user.name,
            role: user.role,
            exp: Math.floor(Date.now() / 1000) + 60 * 60 // 1 hour expiration
        };

        // Generate token
        const token = jwt.sign(payload, SECRET_KEY);


        res.send({
            token : token,
        });
    } catch (err: any) {
        console.error(err);
        res.status(500).send({ message: "Login failed", error: err.message });
    }
});

export default router;
