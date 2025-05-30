﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20250331011105_UpdateUser")]
    partial class UpdateUser
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("Infrastructure.Entity.Assigned", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("LightId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SensorId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("LightId");

                    b.HasIndex("SensorId");

                    b.ToTable("Assigneds");
                });

            modelBuilder.Entity("Infrastructure.Entity.Light", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Brightness")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("EspId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Overide")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Pin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Lights");
                });

            modelBuilder.Entity("Infrastructure.Entity.Motion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SensorId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("motion")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SensorId");

                    b.ToTable("Motion");
                });

            modelBuilder.Entity("Infrastructure.Entity.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EspId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Pin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Sensitivity")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Timeout")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("Infrastructure.Entity.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Infrastructure.Entity.Assigned", b =>
                {
                    b.HasOne("Infrastructure.Entity.Light", "Light")
                        .WithMany("assigned")
                        .HasForeignKey("LightId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Infrastructure.Entity.Sensor", "Sensor")
                        .WithMany("assigned")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Light");

                    b.Navigation("Sensor");
                });

            modelBuilder.Entity("Infrastructure.Entity.Motion", b =>
                {
                    b.HasOne("Infrastructure.Entity.Sensor", "Sensor")
                        .WithMany("MotionHistory")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");
                });

            modelBuilder.Entity("Infrastructure.Entity.Light", b =>
                {
                    b.Navigation("assigned");
                });

            modelBuilder.Entity("Infrastructure.Entity.Sensor", b =>
                {
                    b.Navigation("MotionHistory");

                    b.Navigation("assigned");
                });
#pragma warning restore 612, 618
        }
    }
}
