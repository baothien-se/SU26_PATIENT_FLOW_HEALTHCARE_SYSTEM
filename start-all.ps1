# ============================================
# Hospital Management System - Start All Services
# ============================================
# This script starts all required services and dependencies
# Usage: ./start-all.ps1

param(
    [ValidateSet('full', 'dependencies', 'services', 'gateway')]
    [string]$Mode = 'full'
)

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ErrorActionPreference = 'Stop'

# ============================================
# HELPER FUNCTIONS
# ============================================

function Get-ShellExe {
    $pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($pwsh) {
        return $pwsh.Source
    }

    $windowsPowerShell = Get-Command powershell -ErrorAction SilentlyContinue
    if ($windowsPowerShell) {
        return $windowsPowerShell.Source
    }

    throw 'Neither pwsh nor powershell was found on PATH.'
}

function Wait-ForPort($port, $timeoutSeconds = 30) {
    Write-Host "Waiting for port $port to be available (timeout: ${timeoutSeconds}s)..." -ForegroundColor Yellow
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    while ($stopwatch.Elapsed.TotalSeconds -lt $timeoutSeconds) {
        try {
            $client = New-Object System.Net.Sockets.TcpClient
            $async = $client.BeginConnect('127.0.0.1', $port, $null, $null)
            if ($async.AsyncWaitHandle.WaitOne(1000)) {
                $client.EndConnect($async)
                $client.Close()
                Write-Host "Port $port is ready!" -ForegroundColor Green
                return $true
            }
            $client.Close()
        }
        catch {
            # Continue waiting
        }
        Start-Sleep -Milliseconds 500
    }
    
    Write-Host "Port $port did not become available within ${timeoutSeconds}s" -ForegroundColor Red
    return $false
}

function Download-SwaggerAssets {
    Write-Host "`n" + ("="*50) -ForegroundColor Cyan
    Write-Host "Downloading Swagger UI Assets..." -ForegroundColor Cyan
    Write-Host ("="*50) -ForegroundColor Cyan
    
    $urls = @(
        'https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.32.8/swagger-ui.css',
        'https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.32.8/swagger-ui-bundle.js',
        'https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.32.8/swagger-ui-standalone-preset.js'
    )

    $dest = Join-Path $root 'src\Gateways\ApiGateway\wwwroot'

    if (-not (Test-Path $dest)) {
        New-Item -Path $dest -ItemType Directory -Force | Out-Null
        Write-Host "Created directory: $dest" -ForegroundColor Green
    }

    foreach ($url in $urls) {
        $fileName = Split-Path $url -Leaf
        $outPath = Join-Path $dest $fileName
        
        if (Test-Path $outPath) {
            Write-Host "✓ $fileName already exists, skipping..." -ForegroundColor Green
        }
        else {
            Write-Host "Downloading $fileName..." -ForegroundColor Yellow
            try {
                Invoke-WebRequest -Uri $url -UseBasicParsing -OutFile $outPath
                Write-Host "✓ Downloaded $fileName" -ForegroundColor Green
            }
            catch {
                Write-Host "✗ Failed to download $fileName`: $($_.Exception.Message)" -ForegroundColor Red
                return $false
            }
        }
    }
    
    return $true
}

function Start-Dependencies {
    Write-Host "`n" + ("="*50) -ForegroundColor Cyan
    Write-Host "Starting Dependencies (RabbitMQ, SQL Server)..." -ForegroundColor Cyan
    Write-Host ("="*50) -ForegroundColor Cyan
    
    # Check for docker
    $docker = Get-Command docker -ErrorAction SilentlyContinue
    
    if ($null -eq $docker) {
        Write-Host "⚠ Docker is not installed or not in PATH" -ForegroundColor Yellow
        Write-Host "Please ensure the following are running:" -ForegroundColor Yellow
        Write-Host "  - SQL Server (localhost, port 1433)" -ForegroundColor Yellow
        Write-Host "  - RabbitMQ (localhost, port 5672)" -ForegroundColor Yellow
        return $true
    }
    
    Write-Host "✓ Docker found, dependencies can be started in containers" -ForegroundColor Green
    Write-Host "To start dependencies with Docker, run: docker-compose up -d" -ForegroundColor Cyan
    
    return $true
}

function Start-Services {
    Write-Host "`n" + ("="*50) -ForegroundColor Cyan
    Write-Host "Starting Microservices..." -ForegroundColor Cyan
    Write-Host ("="*50) -ForegroundColor Cyan
    
    # Check .NET
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        Write-Host "✗ .NET is not installed or not in PATH" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✓ .NET found: $($dotnet.Version)" -ForegroundColor Green
    
    # Start IdentityService
    Write-Host "`nLaunching IdentityService on http://localhost:5010 (HTTPS: https://localhost:5011)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; dotnet run --project src/Services/IdentityService/IdentityService.Api/IdentityService.Api.csproj"
    Start-Sleep -Seconds 3

    # Start ProductService
    Write-Host "Launching ProductService on http://localhost:5020 (HTTPS: https://localhost:5021)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; dotnet run --project src/Services/ProductService/ProductService.Api/ProductService.Api.csproj"
    Start-Sleep -Seconds 3

    return $true
}

function Start-Gateway {
    Write-Host "`n" + ("="*50) -ForegroundColor Cyan
    Write-Host "Starting API Gateway..." -ForegroundColor Cyan
    Write-Host ("="*50) -ForegroundColor Cyan
    
    Write-Host "Launching API Gateway on http://localhost:5000 (HTTPS: https://localhost:5001)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root'; dotnet run --project src/Gateways/ApiGateway/ApiGateway.csproj"
    Start-Sleep -Seconds 2
    
    return $true
}

function Show-ServiceInfo {
    Write-Host "`n" + ("="*50) -ForegroundColor Green
    Write-Host "🚀 All Services Started Successfully!" -ForegroundColor Green
    Write-Host ("="*50) -ForegroundColor Green
    
    Write-Host "`nAPI Gateway Entry Point:" -ForegroundColor Yellow
    Write-Host "  HTTP:  http://localhost:5000" -ForegroundColor Cyan
    Write-Host "  HTTPS: https://localhost:5001" -ForegroundColor Cyan
    
    Write-Host "`nIdentity Service:" -ForegroundColor Yellow
    Write-Host "  HTTP:  http://localhost:5010" -ForegroundColor Cyan
    Write-Host "  HTTPS: https://localhost:5011" -ForegroundColor Cyan
    Write-Host "  Docs:  https://localhost:5011/scalar/v1" -ForegroundColor Cyan
    
    Write-Host "`nProduct Service:" -ForegroundColor Yellow
    Write-Host "  HTTP:  http://localhost:5020" -ForegroundColor Cyan
    Write-Host "  HTTPS: https://localhost:5021" -ForegroundColor Cyan
    Write-Host "  Docs:  https://localhost:5021/scalar/v1" -ForegroundColor Cyan
    
    Write-Host "`nGateway Swagger UI:" -ForegroundColor Yellow
    Write-Host "  http://localhost:5000/swagger-ui.html" -ForegroundColor Cyan
    
    Write-Host "`nDocumentation:" -ForegroundColor Yellow
    Write-Host "  System Design:  docs/SYSTEM_DESIGN.md" -ForegroundColor Cyan
    Write-Host "  Phase 1 Status: docs/PHASE_1_COMPLETION.md" -ForegroundColor Cyan
    Write-Host "  Developer Ref:  docs/DEVELOPER_QUICK_REFERENCE.md" -ForegroundColor Cyan
    Write-Host "  Workflows:      docs/WORKFLOW_EXPLANATION.md" -ForegroundColor Cyan
    
    Write-Host "`n📝 To stop services, close the PowerShell windows" -ForegroundColor Yellow
    Write-Host "⚠  Note: Press Ctrl+C to exit each service window" -ForegroundColor Yellow
}

# ============================================
# MAIN EXECUTION
# ============================================

Write-Host "`n" + ("="*50) -ForegroundColor Magenta
Write-Host "Hospital Management System - Start Script" -ForegroundColor Magenta
Write-Host ("="*50) -ForegroundColor Magenta
Write-Host "Mode: $Mode" -ForegroundColor Cyan

try {
    # Download Swagger assets for API Gateway
    if ($Mode -eq 'full' -or $Mode -eq 'gateway') {
        if (-not (Download-SwaggerAssets)) {
            Write-Host "Failed to download Swagger assets (non-critical)" -ForegroundColor Yellow
        }
    }
    
    # Start dependencies
    if ($Mode -eq 'full' -or $Mode -eq 'dependencies') {
        if (-not (Start-Dependencies)) {
            throw "Failed to start dependencies"
        }
    }
    
    # Start services
    if ($Mode -eq 'full' -or $Mode -eq 'services') {
        if (-not (Start-Services)) {
            throw "Failed to start services"
        }
    }
    
    # Start gateway
    if ($Mode -eq 'full' -or $Mode -eq 'gateway') {
        if (-not (Start-Gateway)) {
            throw "Failed to start gateway"
        }
        
        # Wait for ports to be ready
        Wait-ForPort 5011 30 | Out-Null
        Wait-ForPort 5021 30 | Out-Null
        Wait-ForPort 5001 30 | Out-Null
    }
    
    Show-ServiceInfo
}
catch {
    Write-Host "`n✗ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n"
