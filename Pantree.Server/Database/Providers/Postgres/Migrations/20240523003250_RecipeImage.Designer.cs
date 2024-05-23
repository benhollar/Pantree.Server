﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pantree.Server.Database.Providers.Postgres;

#nullable disable

namespace Pantree.Server.Database.Providers.Postgres.Migrations
{
    [DbContext(typeof(PostgresContext))]
    [Migration("20240523003250_RecipeImage")]
    partial class RecipeImage
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.FoodEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Nutrition")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Foods");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.IngredientEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("FoodId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("RecipeEntityId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("FoodId");

                    b.HasIndex("RecipeEntityId");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.RecipeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<TimeSpan?>("CookingTime")
                        .HasColumnType("interval");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<byte[]>("ImageBlob")
                        .HasColumnType("bytea");

                    b.Property<string>("ImageContentType")
                        .HasColumnType("text");

                    b.Property<string>("Instructions")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeSpan?>("PreparationTime")
                        .HasColumnType("interval");

                    b.Property<long>("Servings")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.FoodEntity", b =>
                {
                    b.OwnsOne("Pantree.Server.Database.Entities.Cooking.FoodEntity.Measurement#Measurement", "Measurement", b1 =>
                        {
                            b1.Property<Guid>("FoodEntityId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Unit")
                                .HasColumnType("integer");

                            b1.Property<double>("Value")
                                .HasColumnType("double precision");

                            b1.HasKey("FoodEntityId");

                            b1.ToTable("Foods");

                            b1.WithOwner()
                                .HasForeignKey("FoodEntityId");
                        });

                    b.Navigation("Measurement");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.IngredientEntity", b =>
                {
                    b.HasOne("Pantree.Server.Database.Entities.Cooking.FoodEntity", "Food")
                        .WithMany("Ingredients")
                        .HasForeignKey("FoodId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Pantree.Server.Database.Entities.Cooking.RecipeEntity", null)
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeEntityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("Pantree.Core.Utilities.Measurement.Measurement<Pantree.Core.Cooking.FoodUnit>", "Quantity", b1 =>
                        {
                            b1.Property<Guid>("IngredientEntityId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Unit")
                                .HasColumnType("integer");

                            b1.Property<double>("Value")
                                .HasColumnType("double precision");

                            b1.HasKey("IngredientEntityId");

                            b1.ToTable("Ingredients");

                            b1.WithOwner()
                                .HasForeignKey("IngredientEntityId");
                        });

                    b.Navigation("Food");

                    b.Navigation("Quantity")
                        .IsRequired();
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.FoodEntity", b =>
                {
                    b.Navigation("Ingredients");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.RecipeEntity", b =>
                {
                    b.Navigation("Ingredients");
                });
#pragma warning restore 612, 618
        }
    }
}
