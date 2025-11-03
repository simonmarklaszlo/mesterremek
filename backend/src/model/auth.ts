export type JwtPayload = {
    userId: number;
    email: string;
    name: string;
    role: string;
    exp?: number; // optional, will be set automatically
}