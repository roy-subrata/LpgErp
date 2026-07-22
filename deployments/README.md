# Deployment

Docker Compose files, environment config, and CI/CD workflows for deploying to a VPS.

## Stack overview

| Stack | Compose file | Web port | Database | Seed data |
|---|---|---|---|---|
| **Production** | `docker-compose.yml` | 80 | `LpgErp` | no |
| **Test** | `docker-compose.test.yml` | 8080 | `LpgErpTest` (own volume) | yes (demo dataset) |

Architecture per stack: `nginx (Angular SPA, proxies /api) → API (.NET 10, auto-migrates on start) → SQL Server 2022`.
Only the web port is exposed; API and DB stay on the internal network. The SPA calls `/api/v1` same-origin, so no CORS in production.

## CI/CD (GitHub Actions)

Deploys are automatic on push when relevant files change:

| Branch | Environment | Action |
|---|---|---|
| `main` | Production | Rebuilds and restarts the prod stack |
| `test` | Test/Staging | Rebuilds and restarts the test stack |

**Triggers:** Only fires when code or deployment files change (path filters on `src/`, `deployments/`, and the workflow file itself). Also supports manual trigger via `workflow_dispatch` in the Actions UI.

### Required GitHub Secrets

Set these in your repo → **Settings → Secrets and variables → Actions**:

| Secret | Value |
|---|---|
| `VPS_HOST` | VPS IP address or hostname |
| `VPS_USER` | SSH username (e.g., `root`) |
| `VPS_PASSWORD` | VPS SSH password |

### How it works

1. You push/merge to `main` or `test` (or trigger manually from Actions UI)
2. GitHub Actions SSHs into the VPS
3. Fetches latest code with `git reset --hard origin/<branch>`
4. Stops and removes old containers
5. Rebuilds images and starts new containers (`--force-recreate`)
6. Prunes unused Docker images to free disk space
7. Migrations apply automatically on API startup

## Manual first-time VPS setup

```bash
# prerequisites: docker + docker compose plugin; ~2 GB free RAM for SQL Server
git clone https://github.com/roy-subrata/LpgErp.git && cd LpgErp/deployments
cp .env.example .env
nano .env                      # set a strong SA_PASSWORD

docker compose up -d --build   # production  -> http://<vps-ip>/
```

## Manual updates (if needed)

```bash
cd deployments
git pull
docker compose up -d --build                                        # prod
docker compose -f docker-compose.test.yml -p lpgerp-test up -d --build  # test
```

## Test environment

```bash
cd deployments
docker compose -f docker-compose.test.yml -p lpgerp-test up -d --build
# -> http://<vps-ip>:8080/  with seeded demo data
```

## Useful commands

```bash
cd deployments
docker compose ps                          # status
docker compose logs -f api                 # API logs (also in the lpg-api-logs volume)
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d LpgErp -Q "SELECT COUNT(*) FROM SalesOrders"
docker compose down                        # stop (volumes kept)
docker compose down -v                     # stop AND delete data (careful!)
```

## Backups

```bash
cd deployments
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C \
  -Q "BACKUP DATABASE LpgErp TO DISK='/var/opt/mssql/data/LpgErp.bak' WITH INIT"
docker compose cp db:/var/opt/mssql/data/LpgErp.bak ./LpgErp-$(date +%F).bak
```
Cron a daily copy of the `.bak` off the VPS.

## HTTPS (recommended)

Put a host-level reverse proxy (Caddy is easiest) in front of the web container:

```
# /etc/caddy/Caddyfile — automatic Let's Encrypt certificates
erp.example.com { reverse_proxy localhost:8081 }
test-erp.example.com { reverse_proxy localhost:8080 }
```
Set `WEB_PORT=8081` in `.env` so Caddy owns port 80/443 and forwards to the container.

## Notes

- **No authentication yet** — do not expose this to the public internet without
  at least basic auth at the proxy or a VPN/firewall rule restricting access.
- SQL Server runs as `MSSQL_PID: Express` (free tier, 10 GB DB limit) — fine for
  this workload; change the value if you own a license.
