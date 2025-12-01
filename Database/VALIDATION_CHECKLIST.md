# Database Validation Checklist ✅

## Pre-Deployment Validation

### 1. Table Definitions ✅
- [x] All 50 tables defined in models
- [x] All configurations match model expectations
- [x] No conflicting table name definitions
- [x] Consistent naming conventions applied

### 2. Migration Files ✅
- [x] `20250914190000_InitialCreatePostgreSQL.cs` - MainAdminUsers, UserPreferences
- [x] `20240915000001_AddRolesTables.cs` - Roles, UserRoles  
- [x] `20240916000001_AddSkillsTables.cs` - Skills, UserSkills
- [x] `20240916000002_AddRoleSkillsTable.cs` - RoleSkills
- [x] `20250915130000_CreateUsersTable.cs` - Users
- [x] `20250120000001_CreateAllMissingTables.cs` - Contacts, Articles, Calendar, etc.
- [x] `20250120000002_RemoveUnexpectedTables.cs` - Cleanup duplicates
- [x] `20250120000003_CreateMissingCoreTables.cs` - Core tables fallback
- [x] `20250120000006_NormalizeUsersSkillsTableNames.cs` - Case normalization
- [x] `20250120000007_ComprehensiveTableNormalization.cs` - Full normalization

### 3. Foreign Key Relationships ✅

#### Core Relationships
- [x] `UserPreferences.UserId` → `MainAdminUsers.Id`
- [x] `UserRoles.UserId` → `MainAdminUsers.Id`
- [x] `UserRoles.RoleId` → `Roles.Id`
- [x] `UserSkills.UserId` → `Users.Id`
- [x] `UserSkills.SkillId` → `Skills.Id`
- [x] `RoleSkills.RoleId` → `Roles.Id`
- [x] `RoleSkills.SkillId` → `Skills.Id`

#### Contacts Relationships
- [x] `ContactNotes.ContactId` → `Contacts.Id`
- [x] `ContactTagAssignments.ContactId` → `Contacts.Id`
- [x] `ContactTagAssignments.TagId` → `ContactTags.Id`

#### Projects Relationships
- [x] `projects.ContactId` → `Contacts.Id`
- [x] `projectcolumns.ProjectId` → `projects.Id`
- [x] `projecttasks.ProjectId` → `projects.Id`
- [x] `projecttasks.ColumnId` → `projectcolumns.Id`
- [x] `projecttasks.ContactId` → `Contacts.Id`
- [x] `projecttasks.ParentTaskId` → `projecttasks.Id`
- [x] `taskcomments.ProjectTaskId` → `projecttasks.Id`
- [x] `taskcomments.DailyTaskId` → `dailytasks.Id`
- [x] `taskattachments.ProjectTaskId` → `projecttasks.Id`
- [x] `taskattachments.DailyTaskId` → `dailytasks.Id`

#### Offers & Sales Relationships
- [x] `offer_items.OfferId` → `offers.Id`
- [x] `offer_activities.OfferId` → `offers.Id`
- [x] `sale_items.SaleId` → `sales.Id`
- [x] `sale_activities.SaleId` → `sales.Id`

#### Service Orders & Installations
- [x] `service_order_jobs.ServiceOrderId` → `service_orders.Id`
- [x] `maintenance_histories.InstallationId` → `installations.Id`

#### Dispatches Relationships
- [x] `dispatch_technicians.DispatchId` → `dispatches.Id`
- [x] `dispatch_time_entries.DispatchId` → `dispatches.Id`
- [x] `dispatch_expenses.DispatchId` → `dispatches.Id`
- [x] `dispatch_materials.DispatchId` → `dispatches.Id`
- [x] `dispatch_attachments.DispatchId` → `dispatches.Id`
- [x] `dispatch_notes.DispatchId` → `dispatches.Id`

#### Calendar Relationships
- [x] `calendar_events.EventTypeId` → `event_types.Id`
- [x] `event_attendees.EventId` → `calendar_events.Id`
- [x] `event_reminders.EventId` → `calendar_events.Id`

### 4. Indexes ✅

#### Unique Indexes
- [x] `MainAdminUsers.Email`
- [x] `Users.Email`
- [x] `UserPreferences.UserId`
- [x] `Roles.Name`
- [x] `Skills.Name`

#### Performance Indexes
- [x] `Contacts.Email`
- [x] `Contacts.Name`
- [x] `Contacts.Status`
- [x] `ContactNotes.ContactId`
- [x] `ContactTagAssignments.ContactId`
- [x] `ContactTagAssignments.TagId`
- [x] `UserSkills.UserId, SkillId`
- [x] `RoleSkills.RoleId, SkillId`
- [x] `projecttasks.ProjectId`
- [x] `projecttasks.ColumnId`

### 5. Expected Tables List (Program.cs) ✅

```csharp
var expectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "__EFMigrationsHistory",
    
    // Core Modules (PascalCase)
    "MainAdminUsers", "Users", "UserPreferences",
    "Roles", "UserRoles",
    "Skills", "UserSkills", "RoleSkills",
    "Contacts", "ContactTags", "ContactTagAssignments", "ContactNotes",
    "LookupItems", "Currencies",
    
    // Domain Modules (snake_case/lowercase)
    "articles", "article_categories", "locations", "inventory_transactions",
    "calendar_events", "event_attendees", "event_reminders", "event_types",
    "offers", "offer_items", "offer_activities",
    "sales", "sale_items", "sale_activities",
    "installations", "maintenance_histories",
    "service_orders", "service_order_jobs",
    "projects", "projectcolumns", "projecttasks", "taskcomments", "taskattachments", "dailytasks",
    "dispatches", "dispatch_technicians", "dispatch_time_entries", "dispatch_expenses", 
    "dispatch_materials", "dispatch_attachments", "dispatch_notes",
    "technician_working_hours", "technician_leaves", "technician_status_history", "dispatch_history"
};
```

---

## Post-Deployment Validation

### Run These Queries on Production DB:

```sql
-- 1. Count all tables (should be 51 including __EFMigrationsHistory)
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

-- 2. List all table names
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
ORDER BY table_name;

-- 3. Check for duplicate table names (case-insensitive)
SELECT LOWER(table_name) as normalized_name, COUNT(*) as count
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
GROUP BY LOWER(table_name)
HAVING COUNT(*) > 1;

-- 4. Verify foreign keys exist
SELECT 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
  ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
  ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
ORDER BY tc.table_name, kcu.column_name;

-- 5. Check for missing indexes
SELECT 
    t.tablename,
    COUNT(i.indexname) as index_count
FROM pg_tables t
LEFT JOIN pg_indexes i ON t.tablename = i.tablename
WHERE t.schemaname = 'public'
GROUP BY t.tablename
ORDER BY index_count ASC;
```

---

## Expected Results After Deployment:

### ✅ Success Indicators:
1. **Total tables**: 51 (50 app tables + `__EFMigrationsHistory`)
2. **No duplicate tables**: Query #3 returns 0 rows
3. **All foreign keys present**: Query #4 returns ~50+ rows
4. **All expected tables exist**: All tables from the list above present
5. **No unexpected tables**: Only tables from the list above exist
6. **Application starts**: No EF migration errors in logs
7. **API endpoints respond**: All controllers return proper responses

### ⚠️ Warning Signs:
- Any table name appears twice with different casing
- Missing tables from the expected list
- Unexpected tables not in the schema
- Foreign key constraint errors
- Migration failures in logs

### ❌ Failure Indicators:
- Less than 50 application tables
- Duplicate tables (e.g., `Users` AND `users`)
- Foreign key violations
- Application won't start due to DB errors
- Controllers return 500 errors

---

## Testing Checklist

### API Endpoint Tests:
- [ ] `GET /api/auth/login` - Authentication works
- [ ] `GET /api/users` - Users module operational
- [ ] `GET /api/contacts` - Contacts module operational
- [ ] `GET /api/roles` - Roles module operational
- [ ] `GET /api/skills` - Skills module operational
- [ ] `GET /api/articles` - Articles module operational
- [ ] `GET /api/calendar` - Calendar module operational
- [ ] `GET /api/offers` - Offers module operational
- [ ] `GET /api/sales` - Sales module operational
- [ ] `GET /api/installations` - Installations module operational
- [ ] `GET /api/serviceorders` - Service Orders module operational
- [ ] `GET /api/projects` - Projects module operational
- [ ] `GET /api/dispatches` - Dispatches module operational
- [ ] `GET /api/planning` - Planning module operational
- [ ] `GET /api/lookups` - Lookups module operational

---

*Last Updated: 2025-12-01*
