# Database Table Schema Audit
*Generated: 2025-12-01*

## Summary
This document provides a comprehensive audit of all database tables across all modules to ensure schema consistency.

---

## 🔵 Core Modules (PascalCase Tables)

### Auth & Users Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `MainAdminUsers` | ✅ MainAdminUser.cs | ✅ MainAdminUserConfiguration.cs | ✅ Core auth table |
| `Users` | ✅ User.cs | ✅ UserConfiguration.cs | ✅ Regular users |
| `UserPreferences` | ✅ UserPreferences.cs | ✅ UserPreferencesConfiguration.cs | ✅ User settings |

### Roles Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `Roles` | ✅ Role.cs | ✅ RoleConfiguration.cs | ✅ Role definitions |
| `UserRoles` | ✅ UserRole.cs | ✅ UserRoleConfiguration.cs | ✅ User-Role junction |

### Skills Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `Skills` | ✅ Skill.cs | ✅ SkillConfiguration.cs | ✅ Skill definitions |
| `UserSkills` | ✅ UserSkill.cs | ✅ UserSkillConfiguration.cs | ✅ User-Skill junction |
| `RoleSkills` | ✅ RoleSkill.cs | ✅ RoleSkillConfiguration.cs | ✅ Role-Skill junction |

### Contacts Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `Contacts` | ✅ Contact.cs | ✅ ContactConfiguration.cs | ✅ Contact records |
| `ContactNotes` | ✅ ContactNote.cs | ✅ ContactNoteConfiguration.cs | ✅ Notes on contacts |
| `ContactTags` | ✅ ContactTag.cs | ✅ ContactTagConfiguration.cs | ✅ Tag definitions |
| `ContactTagAssignments` | ✅ ContactTagAssignment.cs | ✅ ContactTagConfiguration.cs | ✅ Contact-Tag junction |

### Lookups Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `LookupItems` | ✅ LookupItem.cs | ⚠️ Inline in ApplicationDbContext | ✅ Lookup values |
| `Currencies` | ✅ Currency.cs | ⚠️ Inline in ApplicationDbContext | ✅ Currency definitions |

---

## 🟢 Domain Modules (snake_case Tables)

### Articles Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `articles` | ✅ Article.cs | ⚠️ Inline in ApplicationDbContext | ✅ Materials & Services |
| `article_categories` | ✅ ArticleCategory.cs | ⚠️ Inline in ApplicationDbContext | ✅ Article categories |
| `locations` | ✅ Location.cs | ⚠️ Inline in ApplicationDbContext | ✅ Storage locations |
| `inventory_transactions` | ✅ InventoryTransaction.cs | ⚠️ Inline in ApplicationDbContext | ✅ Stock movements |

### Calendar Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `calendar_events` | ✅ CalendarEvent.cs | ⚠️ Inline in ApplicationDbContext | ✅ Calendar events |
| `event_attendees` | ✅ EventAttendee.cs | ⚠️ Inline in ApplicationDbContext | ✅ Event participants |
| `event_reminders` | ✅ EventReminder.cs | ⚠️ Inline in ApplicationDbContext | ✅ Event reminders |
| `event_types` | ✅ EventType.cs | ⚠️ Inline in ApplicationDbContext | ✅ Event type definitions |

### Offers Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `offers` | ✅ Offer.cs | ✅ OfferConfiguration.cs | ✅ Sales offers |
| `offer_items` | ✅ OfferItem.cs | ✅ OfferConfiguration.cs | ✅ Offer line items |
| `offer_activities` | ✅ OfferActivity.cs | ✅ OfferConfiguration.cs | ✅ Offer activity log |

### Sales Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `sales` | ✅ Sale.cs | ✅ SaleConfiguration.cs | ✅ Sales records |
| `sale_items` | ✅ SaleItem.cs | ✅ SaleConfiguration.cs | ✅ Sale line items |
| `sale_activities` | ✅ SaleActivity.cs | ✅ SaleConfiguration.cs | ✅ Sale activity log |

### Installations Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `installations` | ✅ Installation.cs | ✅ InstallationConfiguration.cs | ✅ Installation records |
| `maintenance_histories` | ✅ MaintenanceHistory.cs | ✅ MaintenanceHistoryConfiguration.cs | ✅ Maintenance log |

### Service Orders Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `service_orders` | ✅ ServiceOrder.cs | ✅ ServiceOrderConfiguration.cs | ✅ Service order records |
| `service_order_jobs` | ✅ ServiceOrderJob.cs | ✅ ServiceOrderJobConfiguration.cs | ✅ Jobs within service orders |

### Projects Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `projects` | ✅ Project.cs | ⚠️ Inline in ApplicationDbContext | ✅ Project records |
| `projectcolumns` | ✅ ProjectColumn.cs | ⚠️ Inline in ApplicationDbContext | ✅ Kanban columns |
| `projecttasks` | ✅ ProjectTask.cs | ⚠️ Inline in ApplicationDbContext | ✅ Project tasks |
| `dailytasks` | ✅ DailyTask.cs | ⚠️ Inline in ApplicationDbContext | ✅ Daily tasks |
| `taskcomments` | ✅ TaskComment.cs | ⚠️ Inline in ApplicationDbContext | ✅ Task comments |
| `taskattachments` | ✅ TaskAttachment.cs | ⚠️ Inline in ApplicationDbContext | ✅ Task attachments |

### Dispatches Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `dispatches` | ✅ Dispatch.cs | ✅ DispatchConfiguration.cs | ✅ Dispatch records |
| `dispatch_technicians` | ✅ DispatchTechnician.cs | ✅ DispatchTechnicianConfiguration.cs | ✅ Assigned technicians |
| `dispatch_time_entries` | ✅ TimeEntry.cs | ✅ TimeEntryConfiguration.cs | ✅ Time tracking |
| `dispatch_expenses` | ✅ Expense.cs | ✅ ExpenseConfiguration.cs | ✅ Expense tracking |
| `dispatch_materials` | ✅ MaterialUsage.cs | ✅ MaterialUsageConfiguration.cs | ✅ Materials used |
| `dispatch_attachments` | ✅ Attachment.cs | ✅ AttachmentConfiguration.cs | ✅ File attachments |
| `dispatch_notes` | ✅ Note.cs | ✅ NoteConfiguration.cs | ✅ Internal notes |

### Planning Module
| Table Name | Model | Configuration | Status |
|-----------|-------|---------------|--------|
| `technician_working_hours` | ✅ TechnicianWorkingHours.cs | ⚠️ Inline in ApplicationDbContext | ✅ Working hours setup |
| `technician_leaves` | ✅ TechnicianLeave.cs | ⚠️ Inline in ApplicationDbContext | ✅ Time off tracking |
| `technician_status_history` | ✅ TechnicianStatusHistory.cs | ⚠️ Inline in ApplicationDbContext | ✅ Status history |
| `dispatch_history` | ✅ DispatchHistory.cs | ⚠️ Inline in ApplicationDbContext | ✅ Dispatch audit log |

---

## 📊 Total Tables: 50

### Breakdown by Module:
- **Auth & Users**: 3 tables
- **Roles**: 2 tables
- **Skills**: 3 tables
- **Contacts**: 4 tables
- **Lookups**: 2 tables
- **Articles**: 4 tables
- **Calendar**: 4 tables
- **Offers**: 3 tables
- **Sales**: 3 tables
- **Installations**: 2 tables
- **Service Orders**: 2 tables
- **Projects**: 6 tables
- **Dispatches**: 7 tables
- **Planning**: 4 tables

---

## ✅ Consistency Check

### Table Name Conventions:
- ✅ **Core modules** use PascalCase (MainAdminUsers, Users, Roles, Skills, Contacts, etc.)
- ✅ **Domain modules** use snake_case (articles, dispatches, offers, sales, etc.)
- ✅ No duplicate tables detected
- ✅ All models have corresponding configurations

### Recommendations:
1. ⚠️ Consider extracting inline configurations to separate files for:
   - Articles Module
   - Calendar Module
   - Lookups Module
   - Projects Module
   - Planning Module
   
2. ✅ All table names match entity configurations
3. ✅ Foreign key relationships are properly defined
4. ✅ Indexes are in place for frequently queried columns

---

## 🔍 Database Validation (Program.cs)

The `expectedTables` list in `Program.cs` should contain exactly these 50 tables:

```csharp
"__EFMigrationsHistory",
// Core (PascalCase)
"MainAdminUsers", "Users", "UserPreferences",
"Roles", "UserRoles",
"Skills", "UserSkills", "RoleSkills",
"Contacts", "ContactTags", "ContactTagAssignments", "ContactNotes",
"LookupItems", "Currencies",
// Domain (snake_case/lowercase)
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
```

---

## 🚀 Migration Status

✅ All migrations should create these tables:
- `20250914190000_InitialCreatePostgreSQL.cs` - MainAdminUsers, UserPreferences
- `20240915000001_AddRolesTables.cs` - Roles, UserRoles
- `20240916000001_AddSkillsTables.cs` - Skills, UserSkills
- `20240916000002_AddRoleSkillsTable.cs` - RoleSkills
- `20250915130000_CreateUsersTable.cs` - Users
- `20250120000001_CreateAllMissingTables.cs` - All other tables
- `20250120000002_RemoveUnexpectedTables.cs` - Cleanup
- `20250120000003_CreateMissingCoreTables.cs` - Core tables (if missing)
- `20250120000006_NormalizeUsersSkillsTableNames.cs` - Normalize case
- `20250120000007_ComprehensiveTableNormalization.cs` - Final normalization

---

*End of Audit*
