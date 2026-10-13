using System;
using Karnel_Travel.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karnel_Travel.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(Karnel_TravelContext))]
    [Migration("20261013130000_AddBookingReviews")]
    public partial class AddBookingReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "BookingId", table: "Feedbacks", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ItemName", table: "Feedbacks", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<int>(name: "ItemType", table: "Feedbacks", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "Rating", table: "Feedbacks", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "UserId", table: "Feedbacks", type: "nvarchar(450)", maxLength: 450, nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_BookingId",
                table: "Feedbacks",
                column: "BookingId",
                unique: true,
                filter: "[BookingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CreatedOn",
                table: "Feedbacks",
                column: "CreatedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Feedbacks_BookingId", table: "Feedbacks");
            migrationBuilder.DropIndex(name: "IX_Feedbacks_CreatedOn", table: "Feedbacks");
            migrationBuilder.DropColumn(name: "BookingId", table: "Feedbacks");
            migrationBuilder.DropColumn(name: "ItemName", table: "Feedbacks");
            migrationBuilder.DropColumn(name: "ItemType", table: "Feedbacks");
            migrationBuilder.DropColumn(name: "Rating", table: "Feedbacks");
            migrationBuilder.DropColumn(name: "UserId", table: "Feedbacks");
        }
    }
}
