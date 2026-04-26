# IncTrak local VS Code launch

Use this flow when you want `shared.inctrak.com` to run locally with the same `AppSettings__...` environment shape
used by Cloudflare and deployment automation.

## Local config file

The local source of truth is:

```text
scripts/inctrak/config.local.yaml
```

Start from:

```text
scripts/inctrak/config.example.yaml
```

`config.local.yaml` is gitignored. Fill in real values directly or export the referenced environment variables before
launch.

## How VS Code launch works

The local launch entries in `.vscode/launch.json` use the `backend: prepare local launch` task.

That task:

1. renders `scripts/inctrak/config.local.yaml`
2. writes the flattened environment variables to `.vscode/shared.inctrak-api.env`
3. builds `shared.inctrak.com`
4. launches the API with `.vscode/shared.inctrak-api.env` as the environment file

The renderer preserves the same config naming used by deployment:

- `AppSettings.IncTrakConnection` becomes `AppSettings__IncTrakConnection`
- `AppSettings.AllowedOrigins[0]` becomes `AppSettings__AllowedOrigins__0`
- `AppSettings.GatewaySecretHeaderName` becomes `AppSettings__GatewaySecretHeaderName`

## Frontend local run

The main debug option in VS Code is:

```text
Local: frontend + backend
```

This matches the MMA launch style:

- hidden backend launch: `Backend: shared.inctrak.com (no browser)`
- hidden frontend launch: `Frontend: Vite`
- one visible compound launch: `Local: frontend + backend`

That visible launch:

- renders the backend env file
- builds the API
- launches the API on `http://localhost:5000` and `https://localhost:5001`
- starts the Vite dev server
- opens the frontend in Chrome at `http://localhost:5173`

You can still run the frontend manually with:

```bash
cd frontend
npm run dev
```

By default Vite proxies `/api/*` to `http://localhost:5000`.

Override the backend target if needed:

```bash
VITE_API_PROXY_TARGET=http://localhost:5000 npm run dev
```
