# Caddy host setup

This document is the host-installed Caddy setup guide for the IncTrak production host.

Long-term operational reference belongs in:

- [inctrak_ubuntu_host_preparation.md](./inctrak_ubuntu_host_preparation.md) for one-time host setup
- [inctrak_production_runbook.md](./inctrak_production_runbook.md) for deploy, validation, and rollback flow

## Goal

Run Caddy on the Ubuntu host as the local reverse proxy behind Cloudflare Tunnel.

Recommended request path:

- browser -> Cloudflare Pages
- `/api/*` -> Cloudflare Pages Functions
- Pages Functions -> Cloudflare Tunnel hostname for the API origin
- Cloudflare Tunnel -> host-installed Caddy on the Ubuntu host
- Caddy -> `inctrak-api`

The same Caddy instance can route future hostnames to other local origins without rebuilding the IncTrak API image.

## Recommended server layout

```text
/srv/logs/caddy
  caddy.log

/etc/caddy
  Caddyfile
```

Prepare the host log directory:

```bash
sudo mkdir -p /srv/logs/caddy
sudo chown -R caddy:caddy /srv/logs/caddy
sudo chmod 755 /srv/logs/caddy
```

The repo includes a matching host logrotate policy at:

```text
scripts/caddy/caddy.logrotate
```

## Install Caddy

Run on the Ubuntu host:

```bash
sudo apt update
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https curl
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update
sudo apt install -y caddy
```

For the Cloudflare Tunnel pattern used here, Caddy only needs to listen on the host. Public access should arrive
through Cloudflare Tunnel.

## Shared Host Caddyfile

The authoritative Caddyfile is host-owned, not repo-owned. Keep the deployable
`/etc/caddy/Caddyfile` on the server and use the shared workbench reference as the
starting point:

```text
CalypsoSys operations workbench:
  docs/caddy.md
  templates/caddy/calypsosys-host.Caddyfile.example
```

Recommended routing pattern:

- `api.inctrak.com` -> `127.0.0.1:8082`
- future local origins -> their own hostnames and upstream ports

## Start and verify Caddy

Run:

```bash
sudo cp /etc/caddy/Caddyfile /etc/caddy/Caddyfile.dist
sudo vi /etc/caddy/Caddyfile
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl enable --now caddy
sudo systemctl restart caddy
sudo systemctl status caddy --no-pager
```

Confirm the host log file is being written:

```bash
ls -l /srv/logs/caddy
sudo tail -n 50 /srv/logs/caddy/caddy.log
```

Set `INCTRAK_REPO_ROOT` in your shell environment first, preferably in `~/.profile`, for example:

```bash
export INCTRAK_REPO_ROOT=/absolute/path/to/your/inctrak_options/checkout
```

Install the host logrotate policy:

```bash
sudo cp "$INCTRAK_REPO_ROOT/scripts/caddy/caddy.logrotate" /etc/logrotate.d/caddy
sudo chmod 644 /etc/logrotate.d/caddy
sudo logrotate -d /etc/logrotate.d/caddy
```

Check the local ingress path:

```bash
curl -i -H "Host: api.inctrak.com" http://127.0.0.1:80/
```

For API route validation:

```bash
curl -i -H "Host: api.inctrak.com" http://127.0.0.1:80/api/optionee/quick/
```

If gateway-secret enforcement is enabled, requests without `X-Internal-Api-Key` should return `401`.

## Cloudflare Tunnel relationship

In the recommended steady state:

- Cloudflare Tunnel is the public-facing ingress
- Caddy stays private on the Ubuntu host
- Caddy selects the local origin based on the `Host` header

Tunnel ingress should point at the local Caddy listener:

```yaml
ingress:
  - hostname: api.inctrak.com
    service: http://127.0.0.1:80
  - service: http_status:404
```

Caddy then proxies that hostname to `127.0.0.1:8082`.

## Reload and maintenance

After editing `/etc/caddy/Caddyfile`:

```bash
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl reload caddy
```

If needed:

```bash
sudo systemctl restart caddy
sudo journalctl -u caddy -n 100 --no-pager
```

## Validation checklist

Before considering host ingress healthy, confirm:

1. `systemctl status caddy` shows the service running.
2. `caddy validate` succeeds.
3. `/srv/logs/caddy/caddy.log` is created and writable.
4. a localhost request with the expected `Host` header reaches the API upstream.
5. Cloudflare Tunnel points at `http://127.0.0.1:80`.
6. the API still responds directly on `127.0.0.1:8082` for local diagnostics.
