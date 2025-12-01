using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesAndOffersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offers') THEN
                        CREATE TABLE offers (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            title varchar(255) NOT NULL,
                            contact_id uuid NOT NULL,
                            status varchar(20) NOT NULL,
                            category varchar(50),
                            currency varchar(3) NOT NULL,
                            amount decimal(15,2) NOT NULL,
                            taxes decimal(15,2),
                            discount decimal(15,2),
                            total_amount decimal(15,2) GENERATED ALWAYS AS (amount + COALESCE(taxes, 0) - COALESCE(discount, 0)) STORED,
                            valid_until timestamp with time zone,
                            notes text,
                            assigned_to uuid,
                            created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                            updated_at timestamp with time zone NOT NULL DEFAULT NOW()
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offer_items') THEN
                        CREATE TABLE offer_items (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            offer_id uuid NOT NULL,
                            item_name varchar(255) NOT NULL,
                            type varchar(20) NOT NULL,
                            article_id uuid,
                            quantity decimal(10,2) NOT NULL,
                            unit varchar(50),
                            unit_price decimal(15,2) NOT NULL,
                            discount decimal(15,2),
                            discount_type varchar(20),
                            total_price decimal(15,2) GENERATED ALWAYS AS (
                                quantity * unit_price - COALESCE(
                                    CASE 
                                        WHEN discount_type = 'percentage' THEN (quantity * unit_price * discount / 100)
                                        WHEN discount_type = 'fixed' THEN discount
                                        ELSE 0
                                    END, 0
                                )
                            ) STORED,
                            description text,
                            CONSTRAINT fk_offer_items_offer FOREIGN KEY (offer_id) REFERENCES offers(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'offer_activities') THEN
                        CREATE TABLE offer_activities (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            offer_id uuid NOT NULL,
                            type varchar(50) NOT NULL,
                            description text NOT NULL,
                            created_by uuid,
                            created_by_name varchar(255) NOT NULL,
                            created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                            CONSTRAINT fk_offer_activities_offer FOREIGN KEY (offer_id) REFERENCES offers(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sales') THEN
                        CREATE TABLE sales (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            title varchar(255) NOT NULL,
                            contact_id uuid NOT NULL,
                            offer_id uuid,
                            status varchar(20) NOT NULL,
                            stage varchar(20) NOT NULL,
                            currency varchar(3) NOT NULL,
                            amount decimal(15,2) NOT NULL,
                            taxes decimal(15,2),
                            discount decimal(15,2),
                            total_amount decimal(15,2) GENERATED ALWAYS AS (amount + COALESCE(taxes, 0) - COALESCE(discount, 0)) STORED,
                            expected_close_date timestamp with time zone,
                            actual_close_date timestamp with time zone,
                            probability integer,
                            notes text,
                            assigned_to uuid,
                            created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                            updated_at timestamp with time zone NOT NULL DEFAULT NOW()
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sale_items') THEN
                        CREATE TABLE sale_items (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            sale_id uuid NOT NULL,
                            item_name varchar(255) NOT NULL,
                            type varchar(20) NOT NULL,
                            article_id uuid,
                            quantity decimal(10,2) NOT NULL,
                            unit varchar(50),
                            unit_price decimal(15,2) NOT NULL,
                            discount decimal(15,2),
                            discount_type varchar(20),
                            total_price decimal(15,2) GENERATED ALWAYS AS (
                                quantity * unit_price - COALESCE(
                                    CASE 
                                        WHEN discount_type = 'percentage' THEN (quantity * unit_price * discount / 100)
                                        WHEN discount_type = 'fixed' THEN discount
                                        ELSE 0
                                    END, 0
                                )
                            ) STORED,
                            description text,
                            CONSTRAINT fk_sale_items_sale FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'sale_activities') THEN
                        CREATE TABLE sale_activities (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            sale_id uuid NOT NULL,
                            type varchar(50) NOT NULL,
                            description text NOT NULL,
                            created_by uuid,
                            created_by_name varchar(255) NOT NULL,
                            created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                            CONSTRAINT fk_sale_activities_sale FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_contact_id') THEN
                        CREATE INDEX idx_offers_contact_id ON offers(contact_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_status') THEN
                        CREATE INDEX idx_offers_status ON offers(status);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_category') THEN
                        CREATE INDEX idx_offers_category ON offers(category);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_created_at') THEN
                        CREATE INDEX idx_offers_created_at ON offers(created_at);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_updated_at') THEN
                        CREATE INDEX idx_offers_updated_at ON offers(updated_at);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offers_assigned_to') THEN
                        CREATE INDEX idx_offers_assigned_to ON offers(assigned_to);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_items_offer_id') THEN
                        CREATE INDEX idx_offer_items_offer_id ON offer_items(offer_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_items_type') THEN
                        CREATE INDEX idx_offer_items_type ON offer_items(type);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_items_item_id') THEN
                        CREATE INDEX idx_offer_items_item_id ON offer_items(article_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_activities_offer_id') THEN
                        CREATE INDEX idx_offer_activities_offer_id ON offer_activities(offer_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_activities_type') THEN
                        CREATE INDEX idx_offer_activities_type ON offer_activities(type);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_offer_activities_created_at') THEN
                        CREATE INDEX idx_offer_activities_created_at ON offer_activities(created_at);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_contact_id') THEN
                        CREATE INDEX idx_sales_contact_id ON sales(contact_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_status') THEN
                        CREATE INDEX idx_sales_status ON sales(status);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_stage') THEN
                        CREATE INDEX idx_sales_stage ON sales(stage);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_created_at') THEN
                        CREATE INDEX idx_sales_created_at ON sales(created_at);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_updated_at') THEN
                        CREATE INDEX idx_sales_updated_at ON sales(updated_at);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_offer_id') THEN
                        CREATE INDEX idx_sales_offer_id ON sales(offer_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sales_assigned_to') THEN
                        CREATE INDEX idx_sales_assigned_to ON sales(assigned_to);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_items_sale_id') THEN
                        CREATE INDEX idx_sale_items_sale_id ON sale_items(sale_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_items_type') THEN
                        CREATE INDEX idx_sale_items_type ON sale_items(type);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_items_article_id') THEN
                        CREATE INDEX idx_sale_items_article_id ON sale_items(article_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_activities_sale_id') THEN
                        CREATE INDEX idx_sale_activities_sale_id ON sale_activities(sale_id);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_activities_type') THEN
                        CREATE INDEX idx_sale_activities_type ON sale_activities(type);
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_indexes WHERE schemaname = 'public' AND indexname = 'idx_sale_activities_created_at') THEN
                        CREATE INDEX idx_sale_activities_created_at ON sale_activities(created_at);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS sale_activities CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sale_items CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS sales CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS offer_activities CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS offer_items CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS offers CASCADE;");
        }
    }
}
