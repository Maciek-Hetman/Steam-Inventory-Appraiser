using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyApi.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260102145617_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MyApi.Entities.InventoryValuation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SteamId64")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("TotalValueUsd")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("InventoryValuations");
                });

            modelBuilder.Entity("MyApi.Entities.InventoryValuationItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<int>("InventoryValuationId")
                        .HasColumnType("integer");

                    b.Property<string>("MarketHashName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("ValueUsd")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("InventoryValuationId");

                    b.ToTable("InventoryValuationItems");
                });

            modelBuilder.Entity("MyApi.Entities.InventoryValuationItem", b =>
                {
                    b.HasOne("MyApi.Entities.InventoryValuation", null)
                        .WithMany("Items")
                        .HasForeignKey("InventoryValuationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MyApi.Entities.InventoryValuation", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
