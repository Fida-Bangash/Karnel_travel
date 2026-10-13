using System;
using Karnel_Travel.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karnel_Travel.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(Karnel_TravelContext))]
    [Migration("20261013120000_AddPaymentToBookings")]
    public partial class AddPaymentToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidOn",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentLast4",
                table: "Bookings",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Bookings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PaidOn", table: "Bookings");
            migrationBuilder.DropColumn(name: "PaymentLast4", table: "Bookings");
            migrationBuilder.DropColumn(name: "PaymentMethod", table: "Bookings");
            migrationBuilder.DropColumn(name: "PaymentReference", table: "Bookings");
            migrationBuilder.DropColumn(name: "PaymentStatus", table: "Bookings");
        }
    }
}
