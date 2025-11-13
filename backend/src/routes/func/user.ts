import {Request, Response} from "express";
import userAccess from "../../db/userAccess";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import {config} from "../../config/config";
import {JwtPayload} from "../../model/auth";


const currentExpirationTime = (): number => Math.floor(Date.now() / 1000) + expirationTimeAsSeconds;
const expirationTimeAsSeconds = 60 * 60 * 24 * 7;

export async function registerUser(req: Request, res: Response): Promise<void> {
    try {
        const {email, password, name} = req.body;

        if (!email || !password || !name || (password && password.length < 6)) {
            res.status(400).json({message: "Hiányzó vagy érvénytelen mezők"});
            return;
        }

        const passwordHash = await bcrypt.hash(password, 12);
        const {alreadyExists, id} = await userAccess.registerUser(email, passwordHash, name);

        if (alreadyExists) {
            res.status(409).json({message: "Email már létezik"});
            return;
        }

        res.status(201).json({
            message: "Registration successful",
            id,
        });
    } catch (err: any) {
        console.error(`[REGISTER] ${err}`);
        res.status(500).json({message: "Szerver hiba"});
    }
}

export async function loginUser(req: Request, res: Response): Promise<void> {
    try {
        const {email, password} = req.body;

        if (!email || !password) {
            res.status(400).json({message: "Hiányzó mezők"});
            return;
        }


        const user = await userAccess.getUserByEmail(email);
        if (!user) {
            res.status(401).json({message: "Érvénytelem email vagy jelszó"});
            return;
        }

        const isMatch = await bcrypt.compare(password, user.passwordHash);
        if (!isMatch) {
            res.status(401).json({message: "Érvénytelem email vagy jelszó"});
            return;
        }

        // Create JWT payload
        const payload: JwtPayload = {
            userId: user.id,
            email: email,
            name: user.name,
            role: user.role,
            exp: currentExpirationTime(),
        };

        // Generate token
        const token = jwt.sign(payload, config.jwt.secret);


        res.status(200)
            .json({
                token: token,
                user: {
                    id: user.id,
                    name: user.name,
                    email: user.email,
                    role: user.role,
                }
            });
    } catch (err: any) {
        console.error(`[LOGIN] ${err}`);
        res.status(500).send({message: "Szerver hiba"});
    }
}