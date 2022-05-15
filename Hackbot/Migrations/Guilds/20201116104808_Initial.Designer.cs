﻿// <auto-generated />
using System;
using Hackbot.Services.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hackbot.Migrations.Guilds
{
    [DbContext(typeof(GuildsContext))]
    [Migration("20201116104808_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9");

            modelBuilder.Entity("Hackbot.Structures.Guild", b =>
                {
                    b.Property<ulong>("P_KEY")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("CaptainId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("InSearching")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("P_KEY");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Hackbot.Structures.Member", b =>
                {
                    b.Property<ulong>("P_KEY")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("GuildP_KEY")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Role")
                        .HasColumnType("TEXT");

                    b.HasKey("P_KEY");

                    b.HasIndex("GuildP_KEY");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("Hackbot.Structures.Member", b =>
                {
                    b.HasOne("Hackbot.Structures.Guild", null)
                        .WithMany("Members")
                        .HasForeignKey("GuildP_KEY");
                });
#pragma warning restore 612, 618
        }
    }
}