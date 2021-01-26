﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NameBadger.Bot.Contexts;

namespace NameBadger.Bot.Migrations
{
    [DbContext(typeof(NameBadgeContext))]
    [Migration("20210126003757_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("NameBadger.Bot.Models.NameBadge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHoisted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastInteraction")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleColor")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoleName")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("NameBadges");
                });
#pragma warning restore 612, 618
        }
    }
}
