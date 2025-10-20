import express, { Application } from "express";
import userRoutes from "./routes/userRoutes";
import shopRoutes from "./routes/shopRoutes";
import cityRoutes from "./routes/cityRoutes";
import cors from 'cors';
import dotenv from "dotenv";


dotenv.config();

const app: Application = express();
const PORT = 3000;

// Middleware to parse JSON
app.use(express.json());
app.use(cors());

// Routes
app.use("/api/auth", userRoutes);
app.use("/api/shop", shopRoutes);
app.use("/api/cities", cityRoutes);

app.listen(PORT, () => {
    console.log(`Server running on http://localhost:${PORT}`);
});
