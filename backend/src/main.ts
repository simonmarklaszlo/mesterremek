import express, { Application } from "express";
import userRoutes from "./routes/userRoutes";
import shopRoutes from "./routes/shopRoutes";
import suggestionRoutes from "./routes/suggestionRoutes";
import cors from 'cors';
import {config} from "./config/config";


const app: Application = express();
const HOST = config.server.address;
const PORT = config.server.port;

// Middleware to parse JSON
app.use(express.json());
app.use(cors());

// Routes
app.use("/api/auth", userRoutes);
app.use("/api/shops", shopRoutes);
app.use("/api/suggestions", suggestionRoutes);

app.listen(PORT, HOST, () => {
    console.log(`Server running on http://localhost:${PORT}`);
});
