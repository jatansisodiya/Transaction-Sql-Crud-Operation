# CI/CD setup for both projects (`web.dev.com` and `api.dev.com`)

This repository has two ASP.NET projects:
- `Qualification` (web app) -> `web.dev.com`
- `Transaction Sql Crud Operation` (Web API) -> `api.dev.com`

The GitHub Actions workflow:
1. Restores, builds, and tests the solution.
2. Publishes both apps as artifacts.
3. Deploys both apps to a Linux server via SSH/SCP.
4. Restarts two systemd services and reloads Nginx.

## 1) Prerequisites

- One Linux VM (Ubuntu recommended) with:
  - .NET runtime 10 installed
  - Nginx installed
  - systemd available
- DNS A records:
  - `web.dev.com` -> your VM public IP
  - `api.dev.com` -> your VM public IP
- SSL certificate (Let's Encrypt recommended)

## 2) Create GitHub repository secrets

In **GitHub -> Settings -> Secrets and variables -> Actions**, add:

- `DEPLOY_HOST`: VM IP or hostname
- `DEPLOY_USER`: SSH user (must have sudo rights)
- `DEPLOY_SSH_KEY`: private SSH key (multiline)

## 3) Prepare server folders

```bash
sudo mkdir -p /var/www/web.dev.com /var/www/api.dev.com
sudo chown -R $USER:$USER /var/www/web.dev.com /var/www/api.dev.com
```

## 4) Create systemd services

Create `/etc/systemd/system/qualification-web.service`:

```ini
[Unit]
Description=Qualification Web App
After=network.target

[Service]
WorkingDirectory=/var/www/web.dev.com
ExecStart=/usr/bin/dotnet /var/www/web.dev.com/Qualification.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=qualification-web
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5001

[Install]
WantedBy=multi-user.target
```

Create `/etc/systemd/system/transaction-api.service`:

```ini
[Unit]
Description=Transaction Sql Crud Operation API
After=network.target

[Service]
WorkingDirectory=/var/www/api.dev.com
ExecStart=/usr/bin/dotnet "/var/www/api.dev.com/Transaction Sql Crud Operation.dll"
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=transaction-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5002

[Install]
WantedBy=multi-user.target
```

Enable services:

```bash
sudo systemctl daemon-reload
sudo systemctl enable qualification-web.service
sudo systemctl enable transaction-api.service
sudo systemctl start qualification-web.service
sudo systemctl start transaction-api.service
```

## 5) Configure Nginx reverse proxy

Create `/etc/nginx/sites-available/web-and-api.conf`:

```nginx
server {
    listen 80;
    server_name web.dev.com;

    location / {
        proxy_pass http://127.0.0.1:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name api.dev.com;

    location / {
        proxy_pass http://127.0.0.1:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable and reload:

```bash
sudo ln -s /etc/nginx/sites-available/web-and-api.conf /etc/nginx/sites-enabled/web-and-api.conf
sudo nginx -t
sudo systemctl reload nginx
```

## 6) (Recommended) Add HTTPS certificates

```bash
sudo apt-get update
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d web.dev.com -d api.dev.com
```

## 7) Run the CI/CD pipeline

- Push code to `main` (or `master`), or run manually from **Actions** using `workflow_dispatch`.
- Pipeline path:
  - `build-test-publish` job builds/tests and creates artifacts.
  - `deploy` job copies artifacts and restarts services.

## 8) Verify deployment

```bash
curl -I http://web.dev.com
curl -I http://api.dev.com
sudo systemctl status qualification-web.service
sudo systemctl status transaction-api.service
```

If a deployment fails, check logs:

```bash
journalctl -u qualification-web.service -n 200 --no-pager
journalctl -u transaction-api.service -n 200 --no-pager
```
