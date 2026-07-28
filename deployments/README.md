# Deployment

Three environments: **local** (your machine), **test**, and **production**. Test and prod
are Docker Compose stacks deployed to a VPS by GitHub Actions.

| Environment | Where | Branch | Compose file | Env file | Web port | Database | Seed data |
|---|---|---|---|---|---|---|---|
| **Local** | your machine | any | — (`dotnet run` + `ng serve`) | `appsettings.Development.json` | 4200 | `LpgErp` on local SQL | yes |
| **Test** | VPS `~/LpgErp-test` | `test` | `docker-compose.test.yml` | `.env.test` | 8080 | `LpgErpTest` (own volume) | yes |
| **Production** | VPS `~/LpgErp-prod` | `main` | `docker-compose.yml` | `.env.prod` | 80 | `LpgErp` (own volume) | no |

Architecture per deployed stack: `nginx (Angular SPA, proxies /api) → API (.NET 10, auto-migrates on start) → SQL Server 2022`.
Only the web port is published; API and DB stay on the stack's internal network. The SPA calls
`/api/v1` same-origin, so no CORS is involved in test or prod.

The two stacks are fully isolated: separate compose project names, volumes, databases, checkouts,
env files, **and JWT signing keys** — a token minted by test is rejected by production.

## Local development

Local is not containerised — run the API and the Angular dev server directly:

```bash
dotnet run --project src/LpgErp.Api          # http://localhost:5000
cd src/LpgErp.WebApp && ng serve             # http://localhost:4200
```

`ASPNETCORE_ENVIRONMENT=Development` (the default from the SDK) gives you Swagger at `/swagger`,
demo seed data on startup, and the placeholder JWT key from `appsettings.json`. Any loopback
origin is allowed by CORS, so a non-default `ng serve --port` still works — but
`src/environments/environment.ts` has the API URL hard-coded, so change it there if you run the
API on a port other than 5000.

Local needs a SQL Server on `localhost:1433` matching the connection string in
`appsettings.Development.json`. Migrations apply automatically on startup.

## CI/CD

| Event | Workflow | Result |
|---|---|---|
| PR into `main` or `test` | `ci.yml` | Build + test gate only |
| Push to `test` | `deploy-test.yml` | Gate, then rebuild + restart the test stack |
| Push to `main` | `deploy-prod.yml` | Gate, then rebuild + restart the prod stack |

Both deploy workflows run the shared `verify.yml` gate first (`dotnet build`, `dotnet test`,
`ng build --configuration production`). **A commit that does not build never reaches the VPS.**

On the server the deploy builds the new images *before* stopping anything, so a build failure
leaves the running stack untouched instead of causing an outage. After the swap it smoke-tests
the published port and fails the workflow (dumping container logs) if the web or API does not
respond. A `concurrency` group prevents two deploys of the same environment from interleaving.

Deploys only trigger when `src/`, `deployments/`, or the workflow file itself changes. Both
support manual runs via **workflow_dispatch** in the Actions UI.

### Required GitHub configuration

Secrets live in repo → **Settings → Secrets and variables → Actions**, under two
**environments** named `test` and `production`:

| Secret | Value |
|---|---|
| `VPS_HOST` | VPS IP or hostname |
| `VPS_USER` | SSH username |
| `VPS_PASSWORD` | SSH password |

Because the secrets are environment-scoped, pointing `test` and `production` at *different*
`VPS_HOST` values splits them onto separate servers with no other changes. Same value = both
stacks share one server, which is the current setup.

## First-time VPS setup

Prerequisites: docker + the compose plugin, and ~4 GB RAM if both stacks run on one box
(SQL Server wants ~2 GB per instance).

```bash
# Production
git clone https://github.com/roy-subrata/LpgErp.git ~/LpgErp-prod
cd ~/LpgErp-prod && git checkout main
cp deployments/.env.example deployments/.env.prod
nano deployments/.env.prod        # strong SA_PASSWORD, JWT_SECRET, WEB_PORT=80
docker compose -p lpgerp-prod --env-file deployments/.env.prod \
  -f deployments/docker-compose.yml up -d --build

# Test
git clone https://github.com/roy-subrata/LpgErp.git ~/LpgErp-test
cd ~/LpgErp-test && git checkout test
cp deployments/.env.example deployments/.env.test
nano deployments/.env.test        # DIFFERENT SA_PASSWORD and JWT_SECRET, TEST_WEB_PORT=8080
docker compose -p lpgerp-test --env-file deployments/.env.test \
  -f deployments/docker-compose.test.yml up -d --build
```

Generate each `JWT_SECRET` with `openssl rand -base64 48`. The API refuses to start outside
Development if the key is missing, still the committed placeholder, or under 32 bytes.

### Migrating from the older single-directory layout

Earlier deploys used one `~/LpgErp` checkout and one `deployments/.env` for both stacks. Run
this once, then the workflows take over:

```bash
mv ~/LpgErp ~/LpgErp-prod
cd ~/LpgErp-prod
git checkout main
mv deployments/.env deployments/.env.prod
echo "JWT_SECRET=$(openssl rand -base64 48)" >> deployments/.env.prod

git clone https://github.com/roy-subrata/LpgErp.git ~/LpgErp-test
cd ~/LpgErp-test && git checkout test
cp ~/LpgErp-prod/deployments/.env.prod deployments/.env.test
# then edit .env.test: a different SA_PASSWORD and a freshly generated JWT_SECRET
```

Existing prod data is safe as long as the compose **project name** stays `lpgerp-prod` — volumes
belong to the project, not the directory, so moving the checkout does not touch them. The old
workflow already used `-p lpgerp-prod`, but the old manual instructions used a bare
`docker compose up` (project name `deployments`). Confirm which one is live before migrating:

```bash
docker volume ls | grep mssql-data     # expect lpgerp-prod_lpg-mssql-data
```

If it shows `deployments_lpg-mssql-data` instead, the running stack was started manually — back
up the database first (see below), then restore into the `lpgerp-prod` stack after the switch.

**Everyone with a session issued under the old shared key is logged out after this migration,**
since the signing key changes. That is the point of the change.

## Useful commands

```bash
cd ~/LpgErp-prod   # or ~/LpgErp-test
C="docker compose -p lpgerp-prod --env-file deployments/.env.prod -f deployments/docker-compose.yml"

$C ps                      # status
$C logs -f api             # API logs (also in the lpg-api-logs volume)
$C down                    # stop (volumes kept)
$C down -v                 # stop AND delete data (careful!)
```

## Backups

```bash
cd ~/LpgErp-prod
C="docker compose -p lpgerp-prod --env-file deployments/.env.prod -f deployments/docker-compose.yml"
$C exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C \
  -Q "BACKUP DATABASE LpgErp TO DISK='/var/opt/mssql/data/LpgErp.bak' WITH INIT"
$C cp db:/var/opt/mssql/data/LpgErp.bak ./LpgErp-$(date +%F).bak
```

Cron a daily copy of the `.bak` off the VPS.

## HTTPS

Put a host-level reverse proxy in front of the web containers (Caddy handles Let's Encrypt
automatically):

```
# /etc/caddy/Caddyfile
erp.example.com      { reverse_proxy localhost:8081 }
test-erp.example.com { reverse_proxy localhost:8080 }
```

Set `WEB_PORT=8081` in `.env.prod` so Caddy owns 80/443 and forwards to the container. The API
already runs with `UseHttpsRedirect=false` because TLS terminates at the proxy.

## Notes

- The app has JWT authentication with role-based authorisation. Still keep the **test** stack off
  the public internet (firewall or proxy auth) — it carries demo data and permissive seed accounts.
- Test runs with `ASPNETCORE_ENVIRONMENT=Production` deliberately, so it exercises the same code
  paths as prod. Consequence: no Swagger UI in test. Demo data comes from the `SeedData=true`
  flag instead, and the seeder is idempotent, so redeploys don't duplicate rows.
- SQL Server runs as `MSSQL_PID: Express` (free, 10 GB per database) — fine for this workload.
