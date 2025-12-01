using Microsoft.EntityFrameworkCore.Migrations;

namespace MyApi.Migrations
{
    public partial class AddServiceOrdersModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'service_orders') THEN
                        CREATE TABLE service_orders (
                            id VARCHAR(50) PRIMARY KEY,
                            order_number VARCHAR(50) NOT NULL UNIQUE,
                            sale_id VARCHAR(50) NOT NULL,
                            offer_id VARCHAR(50) NOT NULL,
                            contact_id INTEGER NOT NULL,
                            status VARCHAR(20) NOT NULL DEFAULT 'draft',
                            priority VARCHAR(20),
                            description TEXT,
                            notes TEXT,
                            start_date TIMESTAMP,
                            target_completion_date TIMESTAMP,
                            actual_start_date TIMESTAMP,
                            actual_completion_date TIMESTAMP,
                            estimated_duration INTEGER,
                            actual_duration INTEGER,
                            estimated_cost DECIMAL(15,2) DEFAULT 0,
                            actual_cost DECIMAL(15,2) DEFAULT 0,
                            discount DECIMAL(15,2) DEFAULT 0,
                            discount_percentage DECIMAL(5,2) DEFAULT 0,
                            tax DECIMAL(15,2) DEFAULT 0,
                            total_amount DECIMAL(15,2) DEFAULT 0,
                            payment_status VARCHAR(20) DEFAULT 'pending',
                            payment_terms VARCHAR(20) DEFAULT 'net30',
                            invoice_number VARCHAR(50),
                            invoice_date TIMESTAMP,
                            completion_percentage INTEGER DEFAULT 0,
                            requires_approval BOOLEAN DEFAULT FALSE,
                            approved_by VARCHAR(50),
                            approval_date TIMESTAMP,
                            cancellation_reason TEXT,
                            cancellation_notes TEXT,
                            tags TEXT[],
                            custom_fields JSONB,
                            created_by VARCHAR(50) NOT NULL,
                            created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_by VARCHAR(50),
                            updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'service_order_jobs') THEN
                        CREATE TABLE service_order_jobs (
                            id VARCHAR(50) PRIMARY KEY,
                            service_order_id VARCHAR(50) NOT NULL,
                            title VARCHAR(255) NOT NULL,
                            description TEXT,
                            status VARCHAR(20) NOT NULL DEFAULT 'unscheduled',
                            installation_id VARCHAR(50),
                            work_type VARCHAR(50),
                            estimated_duration INTEGER,
                            estimated_cost DECIMAL(15,2) DEFAULT 0,
                            completion_percentage INTEGER DEFAULT 0,
                            assigned_technician_ids TEXT[],
                            created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            CONSTRAINT fk_service_order_jobs_service_order FOREIGN KEY (service_order_id) REFERENCES service_orders(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_order_number') THEN
                        CREATE UNIQUE INDEX idx_service_orders_order_number ON service_orders(order_number);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_sale_id') THEN
                        CREATE INDEX idx_service_orders_sale_id ON service_orders(sale_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_offer_id') THEN
                        CREATE INDEX idx_service_orders_offer_id ON service_orders(offer_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_contact_id') THEN
                        CREATE INDEX idx_service_orders_contact_id ON service_orders(contact_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_status') THEN
                        CREATE INDEX idx_service_orders_status ON service_orders(status);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_priority') THEN
                        CREATE INDEX idx_service_orders_priority ON service_orders(priority);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_created_at') THEN
                        CREATE INDEX idx_service_orders_created_at ON service_orders(created_at);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_orders_start_date') THEN
                        CREATE INDEX idx_service_orders_start_date ON service_orders(start_date);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_order_jobs_service_order_id') THEN
                        CREATE INDEX idx_service_order_jobs_service_order_id ON service_order_jobs(service_order_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_order_jobs_status') THEN
                        CREATE INDEX idx_service_order_jobs_status ON service_order_jobs(status);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_service_order_jobs_installation_id') THEN
                        CREATE INDEX idx_service_order_jobs_installation_id ON service_order_jobs(installation_id);
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS service_order_jobs CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS service_orders CASCADE;");
        }
    }
}
