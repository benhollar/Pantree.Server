﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pantree.Server.Database.Providers.Sqlite;

#nullable disable

namespace Pantree.Server.Database.Providers.Sqlite.Migrations
{
    [DbContext(typeof(SqliteContext))]
    [Migration("20230722171946_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.FoodEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Nutrition")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Foods");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.IngredientEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FoodId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("RecipeEntityId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FoodId");

                    b.HasIndex("RecipeEntityId");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.RecipeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan?>("CookingTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Instructions")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan?>("PreparationTime")
                        .HasColumnType("TEXT");

                    b.Property<uint>("Servings")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("Pantree.Server.Database.Entities.Cooking.FoodEntity", b =>
                {
                    b.OwnsOne("Pantree.Server.Database.Entities.Cooking.FoodEntity.Measurement#Measurement", "Measurement", b1 =>
                        {
                            b1.Property<Guid>("FoodEntityId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("Unit")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("Value")
                                .HasColumnType("REAL");

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
                                .HasColumnType("TEXT");

                            b1.Property<int>("Unit")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("Value")
                                .HasColumnType("REAL");

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
