# Render Deployment Configuration

## Environment Variables to Set in Render Dashboard:

```bash
# Database
DATABASE_URL=postgresql://neondb_owner:npg_CAiFLbX85sIq@ep-summer-fire-adwac3xi-pooler.c-2.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require

# JWT Configuration
JWT_SECRET=98f7h1F9C8WzQl2kxHa7Xt5Tz0Yv93Lm
JWT_ISSUER=ConsolidatedApi
JWT_AUDIENCE=ConsolidatedApi
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7

# Stripe
STRIPE_SECRET_KEY=your_stripe_secret_key_here

# Frontend URL (set to your Vercel URL)
FRONTEND_URL=https://your-frontend-app.vercel.app

# Admin User
ADMIN_EMAIL=elfadanes@gmail.com
ADMIN_PASSWORD=SuperSecret123!
ADMIN_FIRSTNAME=L-Mobile
ADMIN_LASTNAME=Admin

# File Storage (optional - update with your MinIO/S3 settings)
MINIO_ENDPOINT=files.example.com
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin

# API Keys
API_TOKEN_GEMINI=AIzaSyCiV8CSl_tDHZTv3c0lZXvmWia068p0GRQ
API_TOKEN_APYHUB=APY0LohLftAzHPlX5CNriQErLATRVmXS6wJVVb7vtWfZPnbtUU9laUquEvNQ4JBLn4qh2buC4CJ
ACCESS_TOKEN_SQUARE=EAAAl0VqrxqWUbKs69-8Ok6DlR-xHlBW2iy6nW_ANYnKt6gWbpulx7vkcqmHfcon
```

## Render Service Configuration:

1. **Build Command**: `dotnet publish -c Release -o out`
2. **Start Command**: `dotnet out/ConsolidatedApi.dll`
3. **Environment**: `.NET 9`
4. **Instance Type**: Free tier should work for testing

## Frontend Environment Variables (Vercel):

Update `Portal/client-portalFront/.env` to:
```bash
VITE_APP_API_URL=https://your-backend-service.onrender.com
VITE_SQUARE_APP_ID=sandbox-sq0idb-mUBQ0Oab5HUAcPb52VR0HA
VITE_SQUARE_LOCATION_ID=LW2EVVW99H1P0
```

## Health Check URL:
`https://your-backend-service.onrender.com/health`