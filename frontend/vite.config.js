import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

export default defineConfig(({ command }) => ({
  plugins: [react()],
  server: {
    fs: { deny: ['.env', '.env.*', '*.{crt,pem}', '**/.git/**', '**/ozel-veriler/**'] },
    host: 'localhost',
    port: 5173,
    strictPort: true,
    https: command === 'serve' ? {
      cert: readFileSync(fileURLToPath(new URL('./.certs/localhost.pem', import.meta.url))),
      key: readFileSync(fileURLToPath(new URL('./.certs/localhost.key', import.meta.url))),
    } : undefined,
  },
}));
