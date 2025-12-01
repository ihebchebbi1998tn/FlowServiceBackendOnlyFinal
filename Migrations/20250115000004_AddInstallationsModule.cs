using Microsoft.EntityFrameworkCore.Migrations;

namespace MyApi.Migrations
{
    public partial class AddInstallationsModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create installations table with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'installations') THEN
                        CREATE TABLE installations (
                            id VARCHAR(50) PRIMARY KEY,
                            installation_number VARCHAR(50) NOT NULL UNIQUE,
                            contact_id INTEGER NOT NULL,
                            name VARCHAR(255) NOT NULL,
                            model VARCHAR(255) NOT NULL,
                            manufacturer VARCHAR(255) NOT NULL,
                            serial_number VARCHAR(255) UNIQUE,
                            asset_tag VARCHAR(255) UNIQUE,
                            category VARCHAR(50) NOT NULL,
                            type VARCHAR(50),
                            status VARCHAR(50) NOT NULL DEFAULT 'active',
                            location_address VARCHAR(500),
                            location_city VARCHAR(100),
                            location_state VARCHAR(100),
                            location_country VARCHAR(100),
                            location_zip_code VARCHAR(20),
                            location_latitude NUMERIC(10,8),
                            location_longitude NUMERIC(11,8),
                            specifications_processor VARCHAR(255),
                            specifications_ram VARCHAR(255),
                            specifications_storage VARCHAR(255),
                            specifications_operating_system VARCHAR(255),
                            specifications_os_version VARCHAR(255),
                            warranty_has_warranty BOOLEAN NOT NULL DEFAULT false,
                            warranty_from TIMESTAMP,
                            warranty_to TIMESTAMP,
                            warranty_provider VARCHAR(255),
                            warranty_type VARCHAR(255),
                            maintenance_last_date TIMESTAMP,
                            maintenance_next_date TIMESTAMP,
                            maintenance_frequency VARCHAR(50),
                            maintenance_notes VARCHAR(1000),
                            contact_primary_name VARCHAR(255),
                            contact_primary_phone VARCHAR(20),
                            contact_primary_email VARCHAR(255),
                            contact_secondary_name VARCHAR(255),
                            contact_secondary_phone VARCHAR(20),
                            contact_secondary_email VARCHAR(255),
                            tags TEXT[],
                            custom_fields TEXT,
                            created_at TIMESTAMP NOT NULL,
                            updated_at TIMESTAMP NOT NULL,
                            created_by VARCHAR(50),
                            updated_by VARCHAR(50)
                        );
                    END IF;
                END $$;
            ");

            // Create maintenance_histories table with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'maintenance_histories') THEN
                        CREATE TABLE maintenance_histories (
                            id VARCHAR(50) PRIMARY KEY,
                            installation_id VARCHAR(50) NOT NULL,
                            maintenance_date TIMESTAMP NOT NULL,
                            maintenance_type VARCHAR(50) NOT NULL,
                            description VARCHAR(1000),
                            technician VARCHAR(255),
                            duration INTEGER,
                            notes VARCHAR(2000),
                            next_scheduled_date TIMESTAMP,
                            created_at TIMESTAMP NOT NULL,
                            created_by VARCHAR(50),
                            CONSTRAINT fk_maintenance_histories_installations 
                                FOREIGN KEY (installation_id) REFERENCES installations(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Create indexes with existence check (use to_regclass for portability)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF to_regclass('public.idx_installations_installation_number') IS NULL THEN
                        CREATE UNIQUE INDEX idx_installations_installation_number ON installations(installation_number);
                    END IF;
                    IF to_regclass('public.idx_installations_serial_number') IS NULL THEN
                        CREATE UNIQUE INDEX idx_installations_serial_number ON installations(serial_number);
                    END IF;
                    IF to_regclass('public.idx_installations_asset_tag') IS NULL THEN
                        CREATE UNIQUE INDEX idx_installations_asset_tag ON installations(asset_tag);
                    END IF;
                    IF to_regclass('public.idx_installations_contact_id') IS NULL THEN
                        CREATE INDEX idx_installations_contact_id ON installations(contact_id);
                    END IF;
                    IF to_regclass('public.idx_installations_status') IS NULL THEN
                        CREATE INDEX idx_installations_status ON installations(status);
                    END IF;
                    IF to_regclass('public.idx_installations_category') IS NULL THEN
                        CREATE INDEX idx_installations_category ON installations(category);
                    END IF;
                    IF to_regclass('public.idx_installations_created_at') IS NULL THEN
                        CREATE INDEX idx_installations_created_at ON installations(created_at);
                    END IF;
                    IF to_regclass('public.idx_maintenance_histories_installation_id') IS NULL THEN
                        CREATE INDEX idx_maintenance_histories_installation_id ON maintenance_histories(installation_id);
                    END IF;
                    IF to_regclass('public.idx_maintenance_histories_maintenance_date') IS NULL THEN
                        CREATE INDEX idx_maintenance_histories_maintenance_date ON maintenance_histories(maintenance_date);
                    END IF;
                    IF to_regclass('public.idx_maintenance_histories_maintenance_type') IS NULL THEN
                        CREATE INDEX idx_maintenance_histories_maintenance_type ON maintenance_histories(maintenance_type);
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'maintenance_histories') THEN
                        DROP TABLE IF EXISTS maintenance_histories CASCADE;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'installations') THEN
                        DROP TABLE IF EXISTS installations CASCADE;
                    END IF;
                END $$;
            ");
        }
    }
}
