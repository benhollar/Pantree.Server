using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pantree.Core.Cooking;
using Pantree.Server.Database;
using Pantree.Server.Database.Entities.Cooking;
using Pantree.Server.Models.Cooking;

namespace Pantree.Server.Controllers.Cooking
{
    /// <summary>
    /// An API controller for <see cref="Food"/> entities
    /// </summary>
    [ApiVersion("1.0")]
    public class FoodsController : BaseCollectionController<Food, FoodEntity, FoodDto>
    {
        /// <summary>
        /// Construct a new <see cref="FoodsController"/>
        /// </summary>
        /// <inheritdoc/>
        public FoodsController(PantreeDataContext context, IMapper mapper) : base(context, mapper) { }

        /// <inheritdoc/>
        protected override DbSet<FoodEntity> Collection => _context.Foods;

        /// <inheritdoc/>
        protected override async Task<List<FoodEntity>> LoadAllAsync() => await Collection.ToListAsync();

        /// <inheritdoc/>
        protected override async Task<FoodEntity> LoadSingleAsync(Guid id) => await Collection
            .Where(food => food.Id == id)
            .SingleAsync();
    }
}
