# 🚀 Clean Database Deployment Instructions

## Problem Identified
Foreign key constraint `FK_UserSkills_Users_UserId` failed because migrations were conflicting and tables were created in wrong order with naming inconsistencies.

## Solution: Manual Database Setup

Since migrations are conflicting, we need to **manually run the SQL scripts** to set up the database correctly.

---

## 📋 Step-by-Step Instructions

### 1. **Connect to your PostgreSQL database**

Get your database connection string from Render/your hosting provider.

### 2. **Drop ALL existing tables** (CAUTION: This will delete all data!)

```sql
-- Drop all existing tables to start fresh
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS "' || r.tablename || '" CASCADE';
    END LOOP;
END $$;
```

### 3. **Run the SQL scripts in this exact order:**

Execute these SQL files from the `Neon/` folder:

```sql
-- 1. Core Auth & Users (MUST BE FIRST)
\i Neon/01_auth_and_users.sql

-- 2. Roles & Skills
\i Neon/02_roles_and_skills.sql

-- 3. Contacts Module
\i Neon/03_contacts.sql

-- 4. Articles Module
\i Neon/04_articles.sql

-- 5. Offers Module
\i Neon/05_offers.sql

-- 6. Sales Module
\i Neon/06_sales.sql

-- 7. Projects Module
\i Neon/07_projects.sql

-- 8. Calendar Module
\i Neon/08_calendar.sql

-- 9. Lookups Module
\i Neon/09_lookups.sql
```

**OR** if using a SQL client (like pgAdmin, DBeaver, or psql), copy-paste the content of each file in order.

### 4. **Create the EF Migrations History table**

```sql
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Mark all existing migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20240914190000_InitialCreatePostgreSQL', '8.0.0'),
('20240915000001_AddRolesTables', '8.0.0'),
('20240916000001_AddSkillsTables', '8.0.0'),
('20240916000002_AddRoleSkillsTable', '8.0.0'),
('20240918000001_AddContactsModule', '8.0.0'),
('20250115000001_RenamePreferencesToPreferencesJson', '8.0.0'),
('20250115000002_AddMissingTables', '8.0.0'),
('20250115000003_AddSalesAndOffersTables', '8.0.0'),
('20250115000004_AddInstallationsModule', '8.0.0'),
('20250115000005_AddServiceOrdersModule', '8.0.0'),
('20250915120000_AddOnboardingCompleted', '8.0.0'),
('20250915130000_CreateUsersTable', '8.0.0'),
('20250918090000_AddArticlesTable', '8.0.0'),
('20250918100000_AddCalendarTables', '8.0.0'),
('20250918110000_AddLookupsTable', '8.0.0'),
('20251029000001_CreateMissingTables', '8.0.0')
ON CONFLICT DO NOTHING;
```

### 5. **Verify the setup**

Run this query to ensure all tables exist:

```sql
SELECT COUNT(*) as table_count 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_type = 'BASE TABLE';
```

**Expected result: 51 tables** (50 app tables + `__EFMigrationsHistory`)

### 6. **Check for duplicates**

```sql
SELECT LOWER(table_name) as normalized_name, COUNT(*) 
FROM information_schema.tables 
WHERE table_schema = 'public' 
GROUP BY LOWER(table_name) 
HAVING COUNT(*) > 1;
```

**Expected result: 0 rows** (no duplicates)

---

## ✅ Expected Tables (50 total)

After running all scripts, you should have these tables:

**Core (PascalCase):**
- MainAdminUsers
- Users
- UserPreferences
- Roles
- UserRoles
- Skills
- UserSkills
- RoleSkills
- Contacts
- ContactTags
- ContactTagAssignments
- ContactNotes
- LookupItems
- Currencies

**Domain (snake_case):**
- articles
- article_categories
- locations
- inventory_transactions
- calendar_events
- event_attendees
- event_reminders
- event_types
- offers
- offer_items
- offer_activities
- sales
- sale_items
- sale_activities
- installations
- maintenance_histories
- service_orders
- service_order_jobs
- projects
- projectcolumns
- projecttasks
- taskcomments
- taskattachments
- dailytasks
- dispatches
- dispatch_technicians
- dispatch_time_entries
- dispatch_expenses
- dispatch_materials
- dispatch_attachments
- dispatch_notes
- technician_working_hours
- technician_leaves
- technician_status_history
- dispatch_history

---

## 🔄 After Manual Setup

Once the database is set up manually:

1. **Restart your API application**
2. **The app should start successfully** without migration errors
3. **All API endpoints should work**

---

## 🛠️ Alternative: Use complete_database_recreation.sql

You can also run the all-in-one script:

```bash
psql $DATABASE_URL -f Database/complete_database_recreation.sql
```

This script will:
1. Drop all existing tables
2. Recreate all 50 tables with correct structure
3. Create all indexes
4. Set up all foreign keys

Then mark migrations as applied (see step 4 above).

---

## ⚠️ Important Notes

1. **This will delete ALL existing data** - make sure you have backups if needed
2. **Run scripts in the exact order shown** - dependencies matter
3. **After manual setup, DO NOT run EF migrations** - the schema is complete
4. **The API will check for pending migrations on startup** - they should all show as applied

---

## 🆘 Troubleshooting

### If you still see migration errors:

Delete ALL migration files from `Migrations/` folder except:
- Keep: `ApplicationDbContextModelSnapshot.cs`
- Keep: `MigrationExtensions.cs`
- Delete: Everything else

Then the app won't try to apply any migrations.

### If foreign key errors persist:

Double-check that you ran the scripts in the correct order. The foreign keys require parent tables to exist first:
1. MainAdminUsers & Users MUST be created before anything references them
2. Roles & Skills MUST exist before UserRoles/UserSkills/RoleSkills
3. Contacts MUST exist before projects reference them

---

*Last Updated: 2025-12-01*
