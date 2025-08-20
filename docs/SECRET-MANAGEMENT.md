# Secret Management & Cloud Migration Guide

This document explains how to manage secrets locally and migrate to cloud environments.

## Architecture Overview

Our secret management strategy supports multiple sources in order of priority:

1. **User Secrets** (Development only)
2. **Environment Variables** (All environments)
3. **Configuration Files** (Non-sensitive settings)
4. **Cloud Secret Stores** (Production)

## Local Development

### Option 1: User Secrets (Recommended)
```bash
# Set secrets (already done by setup script)
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://user:pass@localhost:27017/db"
```

### Option 2: Environment Variables
```bash
# Windows PowerShell
$env:MongoDb__ConnectionString = "mongodb://user:pass@localhost:27017/db"

# Windows CMD
set MongoDb__ConnectionString=mongodb://user:pass@localhost:27017/db

# Linux/macOS
export MongoDb__ConnectionString="mongodb://user:pass@localhost:27017/db"
```

### Option 3: Docker Environment File
Use the `database/.env` file for Docker Compose variables.

## Cloud Migration Options

### 🔵 Azure Cloud

#### 1. Azure Key Vault (Recommended)

**Setup:**
```csharp
// Add to Program.cs for production
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}
```

**Required NuGet packages:**
```xml
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.2.2" />
<PackageReference Include="Azure.Identity" Version="1.10.4" />
```

**Azure Setup:**
```bash
# Create Key Vault
az keyvault create --name "minisocial-kv" --resource-group "minisocial-rg" --location "eastus"

# Add secret
az keyvault secret set --vault-name "minisocial-kv" --name "MongoDb--ConnectionString" --value "your-connection-string"

# Grant access to App Service
az webapp identity assign --name "minisocial-app" --resource-group "minisocial-rg"
az keyvault set-policy --name "minisocial-kv" --object-id "app-identity-id" --secret-permissions get list
```

#### 2. Azure App Service Application Settings
```bash
# Set via Azure CLI
az webapp config appsettings set --name "minisocial-app" --resource-group "minisocial-rg" \
  --settings MongoDb__ConnectionString="your-connection-string"
```

### 🟠 AWS Cloud

#### 1. AWS Secrets Manager (Recommended)

**Setup:**
```csharp
// Add to Program.cs for production
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddSecretsManager(configurator: options =>
    {
        options.SecretFilter = entry => entry.Name.StartsWith("minisocial/");
        options.KeyGenerator = (entry, key) => key.Replace("minisocial/", "").Replace("/", ":");
    });
}
```

**Required NuGet packages:**
```xml
<PackageReference Include="Kralizek.Extensions.Configuration.AWSSecretsManager" Version="3.0.0" />
```

**AWS Setup:**
```bash
# Create secret
aws secretsmanager create-secret --name "minisocial/MongoDb/ConnectionString" \
  --secret-string "your-connection-string"

# Grant access to ECS task role
aws iam attach-role-policy --role-name "minisocial-task-role" \
  --policy-arn "arn:aws:iam::aws:policy/SecretsManagerReadWrite"
```

#### 2. AWS Systems Manager Parameter Store
```bash
# Set parameter
aws ssm put-parameter --name "/minisocial/MongoDb/ConnectionString" \
  --value "your-connection-string" --type "SecureString"
```

### 🟢 Google Cloud

#### 1. Google Secret Manager

**Setup:**
```csharp
// Add to Program.cs for production
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddGoogleSecretManager(projectId: "your-project-id");
}
```

**Required NuGet packages:**
```xml
<PackageReference Include="Google.Cloud.SecretManager.V1" Version="2.4.0" />
<PackageReference Include="Google.Extensions.AspNetCore.Configuration.SecretManager" Version="1.0.0" />
```

### 🔶 HashiCorp Vault (Cloud Agnostic)

**Setup:**
```csharp
// Add to Program.cs
if (builder.Environment.IsProduction())
{
    var vaultOptions = new VaultOptions
    {
        VaultServerUriWithPort = "https://vault.company.com:8200",
        VaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN")
    };
    
    builder.Configuration.AddVault(vaultOptions, "secret/minisocial");
}
```

## Environment-Specific Configuration

### Development
- User Secrets for sensitive data
- `appsettings.Development.json` for non-sensitive overrides
- Local Docker containers

### Staging/Production
- Cloud secret managers for sensitive data
- Environment variables for deployment-specific settings
- Managed database services

## Best Practices

### 1. Secret Hierarchy
```
Priority (highest to lowest):
1. Cloud Secret Store (production)
2. Environment Variables (containers)
3. User Secrets (development)
4. Configuration Files (defaults)
```

### 2. Naming Conventions
```
Configuration Key: MongoDb:ConnectionString
Environment Var:   MongoDb__ConnectionString  (double underscore)
User Secret:       MongoDb:ConnectionString   (colon)
Azure Key Vault:   MongoDb--ConnectionString  (double dash)
```

### 3. Security Checklist
- ✅ Never commit secrets to source control
- ✅ Use different secrets per environment
- ✅ Rotate secrets regularly
- ✅ Use least-privilege access
- ✅ Monitor secret access logs
- ✅ Use managed identities when possible

### 4. Connection String Formats
```bash
# Local Development
mongodb://user:password@localhost:27017/database

# MongoDB Atlas
mongodb+srv://user:password@cluster.mongodb.net/database?retryWrites=true&w=majority

# Azure Cosmos DB (MongoDB API)
mongodb://account:key@account.mongo.cosmos.azure.com:10255/database?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@account@

# AWS DocumentDB
mongodb://user:password@cluster.cluster-id.region.docdb.amazonaws.com:27017/database?ssl=true&replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false
```

## Migration Checklist

### Pre-Migration
- [ ] Document current local setup
- [ ] Identify all secrets and configuration
- [ ] Choose cloud secret management service
- [ ] Plan secret rotation strategy

### Migration Steps
- [ ] Create cloud secret store
- [ ] Migrate secrets to cloud store
- [ ] Update application configuration
- [ ] Test in staging environment
- [ ] Deploy to production
- [ ] Verify connectivity and functionality

### Post-Migration
- [ ] Remove local secrets from old locations
- [ ] Update documentation
- [ ] Set up monitoring and alerts
- [ ] Plan regular secret rotation
