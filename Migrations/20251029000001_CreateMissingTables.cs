using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    public partial class CreateMissingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up old migration history entries that might be blocking
            migrationBuilder.Sql(@"
                DELETE FROM ""__EFMigrationsHistory"" 
                WHERE ""MigrationId"" IN (
                    '20250129000001_AddPlanningTables',
                    '20251024000001_AddDispatchesModule',
                    '20251025000001_AddPlanningTables'
                );
            ");

            // Create dispatches table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatches (
                    id character varying(50) PRIMARY KEY,
                    dispatch_number character varying(100) NOT NULL,
                    service_order_id character varying(50),
                    job_id character varying(50),
                    status character varying(50) NOT NULL DEFAULT 'pending',
                    priority character varying(20) NOT NULL DEFAULT 'medium',
                    required_skills text[],
                    scheduled_date date,
                    scheduled_start_time time,
                    scheduled_end_time time,
                    estimated_duration integer,
                    actual_start_time timestamp with time zone,
                    actual_end_time timestamp with time zone,
                    actual_duration integer,
                    work_location jsonb,
                    completion_percentage integer NOT NULL DEFAULT 0,
                    dispatched_by character varying(50),
                    dispatched_at timestamp with time zone,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    is_deleted boolean NOT NULL DEFAULT false
                );
                
                CREATE UNIQUE INDEX IF NOT EXISTS idx_dispatches_dispatch_number ON dispatches(dispatch_number);
                CREATE INDEX IF NOT EXISTS idx_dispatches_status ON dispatches(status);
                CREATE INDEX IF NOT EXISTS idx_dispatches_scheduled_date ON dispatches(scheduled_date);
            ");

            // Create dispatch_technicians table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_technicians (
                    dispatch_id character varying(50) NOT NULL,
                    technician_id character varying(50) NOT NULL,
                    name character varying(255),
                    email character varying(255),
                    phone character varying(50),
                    assigned_at timestamp with time zone,
                    PRIMARY KEY (dispatch_id, technician_id),
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
            ");

            // Create dispatch_time_entries table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_time_entries (
                    id character varying(50) PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    technician_id character varying(50) NOT NULL,
                    work_type character varying(50),
                    start_time timestamp with time zone NOT NULL,
                    end_time timestamp with time zone NOT NULL,
                    duration integer,
                    description character varying(2000),
                    billable boolean,
                    hourly_rate numeric(10,2),
                    total_cost numeric(10,2),
                    status character varying(50) DEFAULT 'pending',
                    created_at timestamp with time zone NOT NULL,
                    approved_by character varying(50),
                    approved_at timestamp with time zone,
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS idx_time_entries_dispatch_id ON dispatch_time_entries(dispatch_id);
            ");

            // Create dispatch_expenses table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_expenses (
                    id character varying(50) PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    technician_id character varying(50) NOT NULL,
                    type character varying(100),
                    amount numeric(10,2) NOT NULL,
                    currency character varying(10),
                    description character varying(2000),
                    date date,
                    status character varying(50) DEFAULT 'pending',
                    created_at timestamp with time zone NOT NULL,
                    approved_by character varying(50),
                    approved_at timestamp with time zone,
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS idx_expenses_dispatch_id ON dispatch_expenses(dispatch_id);
            ");

            // Create dispatch_materials table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_materials (
                    id character varying(50) PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    article_id character varying(50) NOT NULL,
                    article_name character varying(255),
                    sku character varying(100),
                    quantity integer,
                    unit_price numeric(10,2),
                    total_price numeric(10,2),
                    used_by character varying(50),
                    used_at timestamp with time zone,
                    status character varying(50) DEFAULT 'pending',
                    created_at timestamp with time zone NOT NULL,
                    approved_by character varying(50),
                    approved_at timestamp with time zone,
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
            ");

            // Create dispatch_attachments table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_attachments (
                    id character varying(50) PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    file_name character varying(255) NOT NULL,
                    file_type character varying(100),
                    file_size_mb numeric(10,2),
                    category character varying(100),
                    uploaded_by character varying(50),
                    uploaded_at timestamp with time zone NOT NULL,
                    storage_path character varying(1000),
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
            ");

            // Create dispatch_notes table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_notes (
                    id character varying(50) PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    content character varying(2000) NOT NULL,
                    category character varying(100),
                    priority character varying(50),
                    created_by character varying(50),
                    created_at timestamp with time zone NOT NULL,
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
            ");

            // Create technician_working_hours table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS technician_working_hours (
                    id SERIAL PRIMARY KEY,
                    technician_id integer NOT NULL,
                    day_of_week integer NOT NULL CHECK (day_of_week >= 0 AND day_of_week <= 6),
                    start_time time NOT NULL,
                    end_time time NOT NULL,
                    is_active boolean NOT NULL DEFAULT true,
                    effective_from date,
                    effective_until date,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    FOREIGN KEY (technician_id) REFERENCES users(""Id"") ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS ""IX_technician_working_hours_technician_id"" ON technician_working_hours(technician_id);
                CREATE INDEX IF NOT EXISTS ""IX_technician_working_hours_dates"" ON technician_working_hours(effective_from, effective_until);
            ");

            // Create technician_leaves table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS technician_leaves (
                    id SERIAL PRIMARY KEY,
                    technician_id integer NOT NULL,
                    leave_type character varying(50) NOT NULL CHECK (leave_type IN ('vacation', 'sick', 'personal', 'training', 'other')),
                    start_date date NOT NULL,
                    end_date date NOT NULL CHECK (end_date >= start_date),
                    start_time time,
                    end_time time,
                    status character varying(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'approved', 'rejected', 'cancelled')),
                    reason text,
                    approved_by integer,
                    approved_at timestamp with time zone,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    FOREIGN KEY (technician_id) REFERENCES users(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (approved_by) REFERENCES users(""Id"") ON DELETE SET NULL
                );
                
                CREATE INDEX IF NOT EXISTS ""IX_technician_leaves_technician_id"" ON technician_leaves(technician_id);
                CREATE INDEX IF NOT EXISTS ""IX_technician_leaves_dates"" ON technician_leaves(start_date, end_date);
                CREATE INDEX IF NOT EXISTS ""IX_technician_leaves_status"" ON technician_leaves(status);
            ");

            // Create technician_status_history table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS technician_status_history (
                    id SERIAL PRIMARY KEY,
                    technician_id integer NOT NULL,
                    status character varying(50) NOT NULL CHECK (status IN ('available', 'busy', 'offline', 'on_leave', 'not_working', 'over_capacity')),
                    changed_from character varying(50),
                    changed_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    changed_by integer,
                    reason text,
                    metadata jsonb,
                    FOREIGN KEY (technician_id) REFERENCES users(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (changed_by) REFERENCES users(""Id"") ON DELETE SET NULL
                );
                
                CREATE INDEX IF NOT EXISTS ""IX_technician_status_history_technician_id"" ON technician_status_history(technician_id);
                CREATE INDEX IF NOT EXISTS ""IX_technician_status_history_changed_at"" ON technician_status_history(changed_at);
            ");

            // Create dispatch_history table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS dispatch_history (
                    id SERIAL PRIMARY KEY,
                    dispatch_id character varying(50) NOT NULL,
                    action character varying(50) NOT NULL CHECK (action IN ('created', 'assigned', 'rescheduled', 'reassigned', 'status_changed', 'updated', 'cancelled', 'deleted')),
                    old_value text,
                    new_value text,
                    changed_by character varying(50),
                    changed_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    metadata jsonb,
                    FOREIGN KEY (dispatch_id) REFERENCES dispatches(id) ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS ""IX_dispatch_history_dispatch_id"" ON dispatch_history(dispatch_id);
                CREATE INDEX IF NOT EXISTS ""IX_dispatch_history_changed_at"" ON dispatch_history(changed_at);
                CREATE INDEX IF NOT EXISTS ""IX_dispatch_history_action"" ON dispatch_history(action);
            ");

            // Add columns to service_order_jobs table if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'required_skills') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN required_skills text[];
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'priority') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN priority character varying(20) DEFAULT 'medium';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'scheduled_date') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN scheduled_date date;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'scheduled_start_time') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN scheduled_start_time time;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'scheduled_end_time') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN scheduled_end_time time;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'location_json') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN location_json jsonb;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'customer_name') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN customer_name character varying(255);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'service_order_jobs' AND column_name = 'customer_phone') THEN
                        ALTER TABLE service_order_jobs ADD COLUMN customer_phone character varying(50);
                    END IF;
                END $$;
                
                CREATE INDEX IF NOT EXISTS ""IX_service_order_jobs_status"" ON service_order_jobs(""Status"");
                CREATE INDEX IF NOT EXISTS ""IX_service_order_jobs_scheduled_date"" ON service_order_jobs(scheduled_date);
                CREATE INDEX IF NOT EXISTS ""IX_service_order_jobs_priority"" ON service_order_jobs(priority);
            ");

            // Add columns to users table if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'Skills') THEN
                        ALTER TABLE users ADD COLUMN ""Skills"" text[];
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'CurrentStatus') THEN
                        ALTER TABLE users ADD COLUMN ""CurrentStatus"" character varying(50) DEFAULT 'offline';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'LocationJson') THEN
                        ALTER TABLE users ADD COLUMN ""LocationJson"" jsonb;
                    END IF;
                END $$;
                
                CREATE INDEX IF NOT EXISTS ""IX_users_Role"" ON users(""Role"");
                CREATE INDEX IF NOT EXISTS ""IX_users_CurrentStatus"" ON users(""CurrentStatus"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_history CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS technician_status_history CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS technician_leaves CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS technician_working_hours CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_notes CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_attachments CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_materials CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_expenses CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_time_entries CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatch_technicians CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dispatches CASCADE;");
        }
    }
}
