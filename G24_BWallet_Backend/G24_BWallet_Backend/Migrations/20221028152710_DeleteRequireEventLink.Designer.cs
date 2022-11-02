﻿// <auto-generated />
using System;
using G24_BWallet_Backend.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace G24_BWallet_Backend.Migrations
{
    [DbContext(typeof(MyDBContext))]
    [Migration("20221028152710_DeleteRequireEventLink")]
    partial class DeleteRequireEventLink
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("G24_BWallet_Backend.Models.Account", b =>
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

            modelBuilder.Entity("G24_BWallet_Backend.Models.Event", b =>
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

            modelBuilder.Entity("G24_BWallet_Backend.Models.EventUser", b =>
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

            modelBuilder.Entity("G24_BWallet_Backend.Models.Friend", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.Property<int>("UserFriendID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("UserID", "UserFriendID");

                    b.ToTable("Friend");
                });

            modelBuilder.Entity("G24_BWallet_Backend.Models.Otp", b =>
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

            modelBuilder.Entity("G24_BWallet_Backend.Models.Receipt", b =>
                {
                    b.Property<int>("ReceiptID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("DivideType")
                        .HasColumnType("int");

                    b.Property<int>("EventID")
                        .HasColumnType("int");

                    b.Property<double>("ReceiptAmount")
                        .HasColumnType("double");

                    b.Property<string>("ReceiptName")
                        .HasColumnType("longtext");

                    b.Property<string>("ReceiptPicture")
                        .HasColumnType("longtext");

                    b.Property<int>("ReceiptStatus")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ReceiptID");

                    b.ToTable("Receipt");
                });

            modelBuilder.Entity("G24_BWallet_Backend.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountID")
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

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

            modelBuilder.Entity("G24_BWallet_Backend.Models.UserDept", b =>
                {
                    b.Property<int>("DeptId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("Debit")
                        .HasColumnType("double");

                    b.Property<int>("DeptStatus")
                        .HasColumnType("int");

                    b.Property<int?>("ReceiptID")
                        .HasColumnType("int");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("DeptId");

                    b.HasIndex("ReceiptID");

                    b.HasIndex("UserID");

                    b.ToTable("user_dept");
                });

            modelBuilder.Entity("G24_BWallet_Backend.Models.EventUser", b =>
                {
                    b.HasOne("G24_BWallet_Backend.Models.Event", "Events")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Events");
                });

            modelBuilder.Entity("G24_BWallet_Backend.Models.User", b =>
                {
                    b.HasOne("G24_BWallet_Backend.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("G24_BWallet_Backend.Models.UserDept", b =>
                {
                    b.HasOne("G24_BWallet_Backend.Models.Receipt", "Receipt")
                        .WithMany()
                        .HasForeignKey("ReceiptID");

                    b.HasOne("G24_BWallet_Backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("Receipt");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
