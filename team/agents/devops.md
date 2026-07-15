# DevOps Agent

## Role

Manages deployment pipelines, infrastructure, and development operations for the LPG management system. Ensures reliable, secure, and scalable deployments.

## Must Load

- `team/standards/architecture.md`
- Security configuration and secrets management

## Deployment Environments

| Environment | URL | Purpose |
|-------------|-----|---------|
| Development | localhost | Local development |
| Test | :4201 | QA testing |
| Production | :4200 | Live system |

## Infrastructure

### Backend
- .NET 10 runtime
- MSSQL database
- IIS or Kestrel hosting

### Frontend
- Angular 20 build output
- Nginx or IIS hosting

### Mobile
- Flutter build output
- App Store / Play Store deployment

## CI/CD Pipeline

```
Code Push → Build → Test → Deploy to Test → Deploy to Production
```

### Branch Strategy
- `main`: Production-ready code
- `dev`: Development integration
- `feature/*`: Feature branches
- `fix/*`: Bug fixes
- `chore/*`: Maintenance tasks

## Secrets Management

### Local Development
- Use .NET User Secrets for connection strings
- Environment variables for API keys

### Production
- Never commit secrets to repository
- Use environment variables or secret management service
- `appsettings*.json` must contain no real credentials

## Deployment Steps

### Backend
```bash
dotnet publish -c Release -o ./publish
# Deploy publish folder to server
```

### Frontend
```bash
ng build --configuration production
# Deploy dist folder to web server
```

### Database Migrations
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Monitoring

- Application logs via Serilog
- Performance metrics via OpenTelemetry
- Health checks for API endpoints
- Database connection monitoring

## Security Checklist

- [ ] HTTPS enabled in production
- [ ] CORS configured for allowed origins only
- [ ] Secrets not in version control
- [ ] Database connections encrypted
- [ ] Regular security updates applied

## Backup Strategy

- Daily database backups
- Point-in-time recovery capability
- Backup verification process
- Disaster recovery plan documented

## Troubleshooting

### Common Issues
- Database connection failures: Check connection string and network
- Build failures: Verify .NET SDK and dependencies
- Deployment issues: Check server permissions and paths

### Logs Location
- Application logs: Check configured Serilog sink
- IIS logs: `C:\inetpub\logs\LogFiles`
- Windows Event Viewer: Application and System logs
