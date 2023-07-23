using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pantree.Core.Utilities.Interfaces;
using Pantree.Server.Database;
using Pantree.Server.Models.Interfaces;

namespace Pantree.Server.Controllers
{
    /// <summary>
    /// The foundational class for any API controller which performs CRUD operations on a collection of data
    /// </summary>
    /// <typeparam name="TModel">The proper model of the collection</typeparam>
    /// <typeparam name="TEntity">
    /// The database mapping type corresponding to <typeparamref name="TModel"/>. It is mandatory for there to be a
    /// <see cref="AutoMapper"/> <see cref="Profile"/> that maps this type to and from <typeparamref name="TModel"/>
    /// </typeparam>
    /// <typeparam name="TDto">
    /// The data transfer type corresponding to <typeparamref name="TModel"/>. It is mandatory for there to be a
    /// <see cref="AutoMapper"/> <see cref="Profile"/> that maps this type to and from <typeparamref name="TModel"/>
    /// </typeparam>
    public abstract partial class BaseCollectionController<TModel, TEntity, TDto> : BaseController
        where TModel : class, Identifiable
        where TEntity : class, Identifiable
        where TDto : IdentifiableDto
    {
        /// <summary>
        /// The database-persisted collection of <typeparamref name="TModel"/> entries
        /// </summary>
        /// <value></value>
        protected abstract DbSet<TEntity> Collection { get; }

        /// <summary>
        /// Asynchronously load the <typeparamref name="TEntity"/> entries in this collection from the database, fully
        /// including their reference properties, if any
        /// </summary>
        /// <returns>The loaded <typeparamref name="TEntity"/> entries</returns>
        protected abstract Task<List<TEntity>> LoadAllAsync();

        /// <summary>
        /// Asynchronously load a single <typeparamref name="TEntity"/> from this collection, specified by an ID,
        /// including its reference properties, if any. If no entry with a matching ID exists, an exception is thrown.
        /// </summary>
        /// <param name="id">The identifier of the <typeparamref name="TEntity"/> to load</param>
        /// <returns>The loaded <typeparamref name="TEntity"/></returns>
        protected abstract Task<TEntity> LoadSingleAsync(Guid id);

        /// <summary>
        /// Asynchronously load a single <typeparamref name="TEntity"/> from this collection including its reference
        /// properties, if any
        /// </summary>
        /// <param name="model">The <typeparamref name="TEntity"/> to load</param>
        /// <returns>The fully loaded <typeparamref name="TEntity"/></returns>
        protected async Task<TEntity> LoadSingleAsync(TEntity model) => await LoadSingleAsync(model.Id);

        /// <summary>
        /// Manually update the <see cref="PantreeDataContext"/> used for a given request used to add or modify the
        /// provided <typeparamref name="TEntity"/>
        /// </summary>
        /// <remarks>
        /// The default implementation of this method does nothing and completes instantly. Subclasses, especially those
        /// working on models that own or reference types that are stored as separate database entities, should consider
        /// overriding this function to ensure the database state matches expectations.
        /// </remarks>
        /// <param name="entity">
        /// The new or modified database <typeparamref name="TEntity"/> to update the database context to properly
        /// reflect
        /// </param>
        /// <returns>A <see cref="Task"/> tracking the completion of this operation</returns>
        protected virtual Task UpdateContext(TEntity entity) => Task.CompletedTask;
    }

    public partial class BaseCollectionController<TModel, TEntity, TDto>
    {
        /// <summary>
        /// Get every <typeparamref name="TModel"/> stored in the database
        /// </summary>
        /// <returns>An <see cref="OkObjectResult"/> with a body containing the models</returns>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            List<TEntity> entities = await LoadAllAsync();
            return Ok(entities.Select(entity => _mapper.Map<TDto>(_mapper.Map<TModel>(entity))));
        }

        /// <summary>
        /// Get a specific <typeparamref name="TModel"/> from the database, keyed by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the model to retrieve
        /// </param>
        /// <returns>An <see cref="OkObjectResult"/> with a body containing the model</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSingle(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not TEntity existing)
                return NotFound("An entity with the provided ID does not exist.");

            await LoadSingleAsync(existing);
            return Ok(_mapper.Map<TDto>(_mapper.Map<TModel>(existing)));
        }

        /// <summary>
        /// Add (or modify) a <typeparamref name="TModel"/>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     When the <paramref name="dto"/> provided does not define an ID or its ID does not exist in the database,
        ///     this endpoint adds a new <typeparamref name="TModel"/> to the database.
        /// </para>
        /// <para>
        ///     Otherwise (i.e., the ID provided exists in the database), this endpoint transparently edits the existing
        ///     <typeparamref name="TModel"/> to match the state of the provided <paramref name="dto"/>. This behavior
        ///     is intended to match the behavior of <see cref="Edit"/>.
        /// </para>
        /// </remarks>
        /// <param name="dto">
        /// The <typeparamref name="TDto"/> representing the <typeparamref name="TModel"/> to add or modify.
        /// </param>
        /// <returns>
        /// A <see cref="CreatedResult"/> when adding a new entity, or an <see cref="OkObjectResult"/> when modifying
        /// an existing one. In both cases, the response body contains the full model.
        /// </returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] TDto dto)
        {
            if (!dto.Validate(out List<string>? errorMessages))
                return BadRequest(FormatDtoValidationMessages(errorMessages));

            TModel newModel = _mapper.Map<TModel>(dto);

            IActionResult output;
            TEntity entityPendingSave;
            if (Collection.Where(model => model.Id == newModel.Id).SingleOrDefault() is TEntity existing)
            {
                await LoadSingleAsync(existing);
                _mapper.Map<TModel, TEntity>(newModel, existing);

                output = Ok(_mapper.Map<TDto>(newModel));
                entityPendingSave = existing;
            }
            else
            {
                TEntity newEntity = _mapper.Map<TEntity>(newModel);
                Collection.Add(newEntity);

                output = Created(newEntity.Id.ToString(), _mapper.Map<TDto>(_mapper.Map<TModel>(newEntity)));
                entityPendingSave = newEntity;
            }

            await UpdateContext(entityPendingSave);
            await _context.SaveChangesAsync();
            return output;
        }

        /// <summary>
        /// Edit an existing <typeparamref name="TModel"/> keyed by its <paramref name="id"/>
        /// </summary>
        /// <param name="dto">
        /// The <typeparamref name="TDto"/> representing the <typeparamref name="TModel"/> to modify.
        /// </param>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the model to modify
        /// </param>
        /// <returns>A <see cref="NoContentResult"/> when the modification is successful</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([FromBody] TDto dto, string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");

            dto.Id = id;
            if (!dto.Validate(out List<string>? errorMessages))
                return BadRequest(FormatDtoValidationMessages(errorMessages));

            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not TEntity existing)
                return NotFound("An entity with the provided ID does not exist.");
            
            await LoadSingleAsync(existing);

            TModel newModel = _mapper.Map<TModel>(dto);
            _mapper.Map<TModel, TEntity>(newModel, existing);

            await UpdateContext(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete an existing <typeparamref name="TModel"/>
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the model to modify
        /// </param>
        /// <returns>A <see cref="NoContentResult"/> when the deletion is successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not TEntity toDelete)
                return NotFound("An entity with the provided ID does not exist.");

            try
            {
                Collection.Remove(toDelete);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("The entity could not be deleted, possibly due to database constraints.");
            }

            return NoContent();
        }
    }

    public partial class BaseCollectionController<TModel, TEntity, TDto>
    {
        /// <summary>
        /// The <see cref="IMapper"/> used to convert between <typeparamref name="TModel"/>,
        /// <typeparamref name="TEntity"/>, and <typeparamref name="TDto"/> as needed
        /// </summary>
        protected readonly IMapper _mapper;

        /// <summary>
        /// Construct a new <see cref="BaseCollectionController{TModel, TEntity, TDto}"/>
        /// </summary>
        /// <param name="context">The database context provided via dependency injection</param>
        /// <param name="mapper">The <see cref="IMapper"/> provided via dependency injection</param>
        /// <returns></returns>
        protected BaseCollectionController(PantreeDataContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Given a collection of error messages from <see cref="IDto.Validate(out List{string}?)"/>, format them into
        /// a readable list to be presented to a user
        /// </summary>
        /// <param name="errorMessages">The error messages to display</param>
        /// <returns>The formatted combination of the error messages</returns>
        private static string FormatDtoValidationMessages(List<string> errorMessages)
        {
            string BuildMultiLineMessage(string parentMessage, List<string> childMessages)
            {
                StringBuilder sb = new(parentMessage);
                foreach ((int iMessage, string message) in Enumerable.Range(1, childMessages.Count).Zip(childMessages))
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("  ");
                    sb.Append($"{iMessage}. {message}");
                }
                return sb.ToString();
            }

            return errorMessages.Count switch
            {
                1 => errorMessages[0],
                >= 2 => BuildMultiLineMessage("The provided configuration was not valid:", errorMessages),
                _ => "The provided configuration was not valid for an unspecified reason.",
            };
        }
    }
}
