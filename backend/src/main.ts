import express, { Application } from "express";
import userRoutes from "./routes/userRoutes";

const app: Application = express();
const PORT = 3000;

// Middleware to parse JSON
app.use(express.json());

// Routes
app.use("/users", userRoutes);

app.listen(PORT, () => {
    console.log(`Server running on http://localhost:${PORT}`);
});
