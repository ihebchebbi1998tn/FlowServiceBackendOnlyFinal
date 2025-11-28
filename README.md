# Consolidated Portal API

This is a monolithic API that consolidates all the microservices from the Portal project for easier deployment on Render.

## Features

- **User Management**: Authentication, authorization, roles, and user profiles
- **Chat System**: Real-time messaging with SignalR
- **Notifications**: Push notifications and email alerts
- **Service Management**: Device management, service requests, orders, and invoices
- **File Storage**: MinIO integration for file uploads

## Database

Uses PostgreSQL (Neon) instead of SQL Server for better cloud compatibility.

## Environment Variables

Key environment variables (set in Render dashboard):

```
DATABASE_URL=postgresql://neondb_owner:npg_CAiFLbX85sIq@ep-summer-fire-adwac3xi-pooler.c-2.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require
JWT_SECRET=your-jwt-secret
STRIPE_SECRET_KEY=your-stripe-key
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=your-admin-password
```

## Deployment to Render

1. Connect your GitHub repository to Render
2. Create a new Web Service
3. Set build command: `dotnet publish -c Release -o out`
4. Set start command: `dotnet out/ConsolidatedApi.dll`
5. Add environment variables in Render dashboard
6. Deploy!

## Health Check

The API provides a health check endpoint at `/health`

## API Documentation

Swagger is available at `/swagger` in development mode.

## SignalR Hubs

- `/chat/hub` - Organization chat
- `/user-chat/hub` - User-to-user chat
- `/notification/hub` - Real-time notifications