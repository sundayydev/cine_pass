using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_CinePass.API.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_member_points_user_id",
                table: "member_points");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "point_history",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                table: "point_history",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                table: "point_history",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voucher_id",
                table: "point_history",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "final_amount",
                table: "orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "user_voucher_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lifetime_points",
                table: "member_points",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_expiry_date",
                table: "member_points",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "points_to_expire",
                table: "member_points",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "tier",
                table: "member_points",
                type: "text",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "member_tier_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier = table.Column<string>(type: "text", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    min_points = table.Column<int>(type: "integer", nullable: false),
                    max_points = table.Column<int>(type: "integer", nullable: true),
                    point_multiplier = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    color = table.Column<string>(type: "varchar(20)", nullable: true),
                    icon_url = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    benefits = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_tier_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    max_discount_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    min_order_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    points_required = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    quantity_redeemed = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", maxLength: 20, nullable: false),
                    min_tier = table.Column<string>(type: "text", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vouchers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_vouchers", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_vouchers_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_vouchers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_vouchers_vouchers_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "vouchers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_point_history_created_at",
                table: "point_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_point_history_order_id",
                table: "point_history",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_history_voucher_id",
                table: "point_history",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_voucher_id",
                table: "orders",
                column: "user_voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_points_user_id",
                table: "member_points",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_member_tier_configs_tier",
                table: "member_tier_configs",
                column: "tier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_vouchers_order_id",
                table: "user_vouchers",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_vouchers_user_id",
                table: "user_vouchers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_vouchers_user_id_is_used",
                table: "user_vouchers",
                columns: new[] { "user_id", "is_used" });

            migrationBuilder.CreateIndex(
                name: "IX_user_vouchers_voucher_id",
                table: "user_vouchers",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_vouchers_code",
                table: "vouchers",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_user_vouchers_user_voucher_id",
                table: "orders",
                column: "user_voucher_id",
                principalTable: "user_vouchers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_point_history_orders_order_id",
                table: "point_history",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_point_history_vouchers_voucher_id",
                table: "point_history",
                column: "voucher_id",
                principalTable: "vouchers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_user_vouchers_user_voucher_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_point_history_orders_order_id",
                table: "point_history");

            migrationBuilder.DropForeignKey(
                name: "FK_point_history_vouchers_voucher_id",
                table: "point_history");

            migrationBuilder.DropTable(
                name: "member_tier_configs");

            migrationBuilder.DropTable(
                name: "user_vouchers");

            migrationBuilder.DropTable(
                name: "vouchers");

            migrationBuilder.DropIndex(
                name: "IX_point_history_created_at",
                table: "point_history");

            migrationBuilder.DropIndex(
                name: "IX_point_history_order_id",
                table: "point_history");

            migrationBuilder.DropIndex(
                name: "IX_point_history_voucher_id",
                table: "point_history");

            migrationBuilder.DropIndex(
                name: "IX_orders_user_voucher_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_member_points_user_id",
                table: "member_points");

            migrationBuilder.DropColumn(
                name: "description",
                table: "point_history");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "point_history");

            migrationBuilder.DropColumn(
                name: "order_id",
                table: "point_history");

            migrationBuilder.DropColumn(
                name: "voucher_id",
                table: "point_history");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "final_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_voucher_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "lifetime_points",
                table: "member_points");

            migrationBuilder.DropColumn(
                name: "next_expiry_date",
                table: "member_points");

            migrationBuilder.DropColumn(
                name: "points_to_expire",
                table: "member_points");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "member_points");

            migrationBuilder.CreateIndex(
                name: "IX_member_points_user_id",
                table: "member_points",
                column: "user_id");
        }
    }
}
