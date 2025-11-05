import express, { Application } from "express";
import userRoutes from "./routes/userRoutes";
import shopRoutes from "./routes/shopRoutes";
import cors from 'cors';


const app: Application = express();
const HOST = '0.0.0.0';
const PORT = 3000;

// Middleware to parse JSON
app.use(express.json());
app.use(cors());

// Routes
app.use("/api/auth", userRoutes);
app.use("/api/shops", shopRoutes);

app.listen(PORT, HOST, () => {
    console.log(`Server running on http://localhost:${PORT}`);
});
