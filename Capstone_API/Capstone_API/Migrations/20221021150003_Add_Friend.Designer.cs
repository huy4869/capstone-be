﻿// <auto-generated />
using System;
using Capstone_API.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Capstone_API.Migrations
{
    [DbContext(typeof(MyDBContext))]
    [Migration("20221021150003_Add_Friend")]
    partial class Add_Friend
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("Capstone_API.Models.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("ID");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Capstone_API.Models.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("EventDescript")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EventLink")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EventLogo")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("EventStatus")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("ID");

                    b.ToTable("Event");
                });

            modelBuilder.Entity("Capstone_API.Models.EventUser", b =>
                {
                    b.Property<int>("EventID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.Property<int>("UserRole")
                        .HasColumnType("int");

                    b.HasKey("EventID", "UserID");

                    b.ToTable("EventUser");
                });

            modelBuilder.Entity("Capstone_API.Models.Friend", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.Property<int>("UserFriendID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EventStatus")
                        .HasColumnType("int");

                    b.HasKey("UserID", "UserFriendID");

                    b.ToTable("Friend");
                });

            modelBuilder.Entity("Capstone_API.Models.Otp", b =>
                {
                    b.Property<int>("OtpID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("JWToken")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("OtpCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("OtpID");

                    b.ToTable("OtpCode");
                });

            modelBuilder.Entity("Capstone_API.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountID")
                        .HasColumnType("int");

                    b.Property<string>("BankInfo")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FBlink")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Capstone_API.Models.Friend", b =>
                {
                    b.HasOne("Capstone_API.Models.User", null)
                        .WithMany("Friends")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Capstone_API.Models.User", b =>
                {
                    b.HasOne("Capstone_API.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Capstone_API.Models.User", b =>
                {
                    b.Navigation("Friends");
                });
#pragma warning restore 612, 618
        }
    }
}
