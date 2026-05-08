# IncTrak Ubuntu host preparation

This is the one-time Ubuntu host bootstrap checklist for the lab machine that will run the IncTrak API and
PostgreSQL stack in Docker.

Routine deployment and refresh steps belong in:

- [inctrak_production_runbook.md](inctrak_production_runbook.md)

## Goal

Prepare the Ubuntu host to run:

- the Dockerized `shared.inctrak.com` API
- a PostgreSQL instance that hosts:
  - the control-plane database
  - the feedback database
  - the real tenant template database
  - per-tenant databases created from the template
- `cloudflared` for the tunnel-backed API origin
- the rendered-config deployment wrapper used by the IncTrak Docker stack

Use `inctrak` as the server-side identity for directories, stack names, logs, and service names.

## 1. Confirm baseline host details

Run on the Ubuntu host:

```bash
uname -a
lsblk
free -h
ip addr
```

Recommended baseline:

- Ubuntu Server
- `x86_64` / `amd64`
- enough disk for PostgreSQL + tenant databases + Docker images
- enough RAM for PostgreSQL + the API + optional local AI model

If you intend to keep embedded AI enabled on the host, check GPU visibility too:

```bash
nvidia-smi
```

## 2. Install official Docker Engine

Run on the Ubuntu host:

```bash
sudo apt update
sudo apt install -y ca-certificates curl smartmontools

sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

sudo tee /etc/apt/sources.list.d/docker.sources >/dev/null <<EOF_DOCKER
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Architectures: $(dpkg --print-architecture)
Signed-By: /etc/apt/keyrings/docker.asc
EOF_DOCKER

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
sudo docker run hello-world
```

## 3. Allow the deployment user to run Docker

Run on the Ubuntu host:

```bash
sudo usermod -aG docker $USER
newgrp docker
docker version
docker compose version
systemctl status docker --no-pager
```

## 4. Set up SSH key access

If needed, create a key on the Windows workstation in PowerShell:

```powershell
$serverIp = "192.168.50.95"
$serverUser = "deploy"

ssh-keygen -t ed25519 -C "$serverUser@inctrak-lab"
```

Copy the public key to the Ubuntu host:

```powershell
type $env:USERPROFILE\.ssh\id_ed25519.pub | ssh ${serverUser}@${serverIp} "mkdir -p ~/.ssh && chmod 700 ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
```

Verify login:

```powershell
ssh ${serverUser}@${serverIp}
```

## 5. Harden SSH server settings

Edit:

```bash
sudo vi /etc/ssh/sshd_config
```

Recommended settings:

```text
PermitRootLogin no
PubkeyAuthentication yes
PasswordAuthentication no
KbdInteractiveAuthentication no
```

Reload SSH:

```bash
sudo systemctl restart ssh
sudo systemctl status ssh --no-pager
```

## 6. Enable firewall baseline

Run on the Ubuntu host:

```bash
sudo apt update
sudo apt install -y ufw
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status verbose
```

If the API will only be reached through Cloudflare Tunnel and no direct ingress will be used, you can tighten this
later.

## 7. Enable automatic updates

Run on the Ubuntu host:

```bash
sudo apt install -y unattended-upgrades
sudo dpkg-reconfigure -plow unattended-upgrades
```

## 8. Disable lid-close sleep behavior if this is laptop hardware

Edit:

```bash
sudo vi /etc/systemd/logind.conf
```

Recommended:

```text
HandleLidSwitch=ignore
HandleLidSwitchExternalPower=ignore
HandleLidSwitchDocked=ignore
```

Apply:

```bash
sudo systemctl restart systemd-logind
sudo systemd-analyze cat-config systemd/logind.conf | grep -E 'HandleLidSwitch|HandleLidSwitchExternalPower|HandleLidSwitchDocked'
```

## 9. Install cloudflared

If this host will publish the API through Cloudflare Tunnel, install `cloudflared`:

```bash
curl -fsSL https://pkg.cloudflare.com/cloudflare-main.gpg | sudo tee /usr/share/keyrings/cloudflare-main.gpg >/dev/null
echo 'deb [signed-by=/usr/share/keyrings/cloudflare-main.gpg] https://pkg.cloudflare.com/cloudflared any main' | sudo tee /etc/apt/sources.list.d/cloudflared.list
sudo apt update
sudo apt install -y cloudflared
cloudflared --version
```

## 10. Verify prepared host state

Run on the Ubuntu host:

```bash
docker version
docker compose version
systemctl status docker --no-pager
systemctl status ssh --no-pager
sudo ufw status verbose
cloudflared --version
```

## 11. Continue with the production runbook

The remaining IncTrak-specific deployment steps belong in:

- [inctrak_production_runbook.md](inctrak_production_runbook.md)

That runbook covers:

- server directory layout
- rendered-config binary placement
- internal runtime ports
- PostgreSQL database planning and bootstrap
- Docker stack bring-up
- Cloudflare wiring and validation

## Deployment direction

Recommended operating model:

- build the API image locally in WSL
- stage artifacts through `C:\transfer`
- copy artifacts and stack files to the Ubuntu host
- keep runtime secrets and server-local config on the host
- run PostgreSQL + API in Docker on the host
- run `cloudflared` on the host or as a host-managed service

That keeps the lab box as the runtime target without turning it into the primary build machine.
