using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            // Create Offers table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offers') THEN
                        CREATE TABLE offers (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            title character varying(255) NOT NULL,
                            description text,
                            contact_id integer NOT NULL,
                            amount numeric(15,2) NOT NULL,
                            currency character varying(3) NOT NULL,
                            taxes numeric(15,2),
                            discount numeric(15,2),
                            total_amount numeric(15,2) GENERATED ALWAYS AS (amount + COALESCE(taxes, 0) - COALESCE(discount, 0)) STORED,
                            status character varying(20) NOT NULL,
                            category character varying(50),
                            source character varying(50),
                            billing_address text,
                            billing_postal_code character varying(20),
                            billing_country character varying(100),
                            delivery_address text,
                            delivery_postal_code character varying(20),
                            delivery_country character varying(100),
                            valid_until timestamp with time zone,
                            assigned_to character varying(50),
                            assigned_to_name character varying(255),
                            tags text[],
                            created_at timestamp with time zone NOT NULL,
                            updated_at timestamp with time zone NOT NULL,
                            created_by character varying(50) NOT NULL,
                            last_activity timestamp with time zone,
                            converted_to_sale_id character varying(50),
                            converted_to_service_order_id character varying(50),
                            converted_at timestamp with time zone
                        );
                    END IF;
                END $$;
            ");

            // Create Offer Items table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offer_items') THEN
                        CREATE TABLE offer_items (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            offer_id character varying(50) NOT NULL,
                            type character varying(20) NOT NULL,
                            article_id character varying(50) NOT NULL,
                            item_name character varying(255) NOT NULL,
                            item_code character varying(100),
                            description text,
                            quantity numeric(10,2) NOT NULL,
                            unit_price numeric(15,2) NOT NULL,
                            discount numeric(15,2) NOT NULL,
                            discount_type character varying(20) NOT NULL,
                            total_price numeric(15,2) GENERATED ALWAYS AS ((quantity * unit_price) - discount) STORED,
                            installation_id character varying(50),
                            installation_name character varying(255),
                            CONSTRAINT FK_offer_items_offers_offer_id FOREIGN KEY (offer_id) REFERENCES offers(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Create Offer Activities table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offer_activities') THEN
                        CREATE TABLE offer_activities (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            offer_id character varying(50) NOT NULL,
                            type character varying(50) NOT NULL,
                            description text NOT NULL,
                            details text,
                            old_value character varying(255),
                            new_value character varying(255),
                            created_at timestamp with time zone NOT NULL,
                            created_by character varying(50) NOT NULL,
                            created_by_name character varying(255) NOT NULL,
                            CONSTRAINT FK_offer_activities_offers_offer_id FOREIGN KEY (offer_id) REFERENCES offers(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Create Sales table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sales') THEN
                        CREATE TABLE sales (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            title character varying(255) NOT NULL,
                            description text,
                            contact_id integer NOT NULL,
                            amount numeric(15,2) NOT NULL,
                            currency character varying(3) NOT NULL,
                            taxes numeric(15,2),
                            discount numeric(15,2),
                            total_amount numeric(15,2) GENERATED ALWAYS AS (amount + COALESCE(taxes, 0) - COALESCE(discount, 0)) STORED,
                            status character varying(20) NOT NULL,
                            stage character varying(50),
                            priority character varying(20),
                            category character varying(50),
                            source character varying(50),
                            billing_address text,
                            billing_postal_code character varying(20),
                            billing_country character varying(100),
                            delivery_address text,
                            delivery_postal_code character varying(20),
                            delivery_country character varying(100),
                            estimated_close_date timestamp with time zone,
                            actual_close_date timestamp with time zone,
                            valid_until timestamp with time zone,
                            assigned_to character varying(50),
                            assigned_to_name character varying(255),
                            tags text[],
                            created_at timestamp with time zone NOT NULL,
                            updated_at timestamp with time zone NOT NULL,
                            created_by character varying(50) NOT NULL,
                            last_activity timestamp with time zone,
                            offer_id character varying(50),
                            converted_from_offer_at timestamp with time zone,
                            lost_reason text,
                            materials_fulfillment character varying(20),
                            service_orders_status character varying(20)
                        );
                    END IF;
                END $$;
            ");

            // Create Sale Items table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sale_items') THEN
                        CREATE TABLE sale_items (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            sale_id character varying(50) NOT NULL,
                            type character varying(20) NOT NULL,
                            article_id character varying(50) NOT NULL,
                            item_name character varying(255) NOT NULL,
                            item_code character varying(100),
                            description text,
                            quantity numeric(10,2) NOT NULL,
                            unit_price numeric(15,2) NOT NULL,
                            discount numeric(15,2) NOT NULL,
                            discount_type character varying(20) NOT NULL,
                            total_price numeric(15,2) GENERATED ALWAYS AS ((quantity * unit_price) - discount) STORED,
                            installation_id character varying(50),
                            installation_name character varying(255),
                            requires_service_order boolean NOT NULL,
                            service_order_generated boolean NOT NULL,
                            service_order_id character varying(50),
                            fulfillment_status character varying(20),
                            CONSTRAINT FK_sale_items_sales_sale_id FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Create Sale Activities table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sale_activities') THEN
                        CREATE TABLE sale_activities (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            sale_id character varying(50) NOT NULL,
                            type character varying(50) NOT NULL,
                            description character varying(500) NOT NULL,
                            details text,
                            old_value text,
                            new_value text,
                            created_at timestamp with time zone NOT NULL,
                            created_by character varying(50) NOT NULL,
                            created_by_name character varying(255),
                            CONSTRAINT FK_sale_activities_sales_sale_id FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Create Article Categories table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'article_categories') THEN
                        CREATE TABLE article_categories (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            name character varying(100) NOT NULL,
                            type character varying(20) NOT NULL,
                            description character varying(500),
                            parent_id character varying(50),
                            ""order"" integer NOT NULL,
                            is_active boolean NOT NULL,
                            created_at timestamp with time zone NOT NULL
                        );
                    END IF;
                END $$;
            ");

            // Create Locations table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'locations') THEN
                        CREATE TABLE locations (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            name character varying(255) NOT NULL,
                            type character varying(50) NOT NULL,
                            address character varying(500),
                            assigned_technician character varying(50),
                            capacity integer,
                            is_active boolean NOT NULL,
                            created_at timestamp with time zone NOT NULL
                        );
                    END IF;
                END $$;
            ");

            // Create Inventory Transactions table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'inventory_transactions') THEN
                        CREATE TABLE inventory_transactions (
                            id character varying(50) NOT NULL PRIMARY KEY,
                            article_id character varying(50) NOT NULL,
                            type character varying(20) NOT NULL,
                            quantity integer NOT NULL,
                            from_location character varying(255),
                            to_location character varying(255),
                            reason character varying(500) NOT NULL,
                            reference character varying(100),
                            performed_by character varying(50) NOT NULL,
                            notes text,
                            created_at timestamp with time zone NOT NULL
                        );
                    END IF;
                END $$;
            ");

            // Create indexes
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_offer_items_offer_id') THEN
                        CREATE INDEX IX_offer_items_offer_id ON offer_items(offer_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_offer_activities_offer_id') THEN
                        CREATE INDEX IX_offer_activities_offer_id ON offer_activities(offer_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_sale_items_sale_id') THEN
                        CREATE INDEX IX_sale_items_sale_id ON sale_items(sale_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_sale_activities_sale_id') THEN
                        CREATE INDEX IX_sale_activities_sale_id ON sale_activities(sale_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_offers_contact_id') THEN
                        CREATE INDEX IX_offers_contact_id ON offers(contact_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_sales_contact_id') THEN
                        CREATE INDEX IX_sales_contact_id ON sales(contact_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'IX_inventory_transactions_article_id') THEN
                        CREATE INDEX IX_inventory_transactions_article_id ON inventory_transactions(article_id);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS offer_activities CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS offer_items CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS offers CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sale_activities CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sale_items CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sales CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS article_categories CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS locations CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS inventory_transactions CASCADE;");
        }
    }
}
