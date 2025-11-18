using Microsoft.AspNetCore.Mvc;

namespace Pet.API.Controllers
{
    /// <summary>
    /// Base controller with common helper methods for API responses
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger Logger;

        protected BaseController(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles GetById pattern with validation and error handling
        /// </summary>
        protected async Task<ActionResult<T>> GetByIdAsync<T>(
            string id,
            Func<string, Task<T?>> getByIdFunc,
            string entityName)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest($"{entityName} id is required.");

            try
            {
                T? entity = await getByIdFunc(id);

                if (entity == null)
                    return NotFound(new { message = $"{entityName} not found" });

                return Ok(entity!);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching {entityName} {id}");
                return StatusCode(500, new { message = $"Error fetching {entityName}", error = ex.Message });
            }
        }

        /// <summary>
        /// Handles GetAll pattern with error handling
        /// </summary>
        protected async Task<ActionResult<IEnumerable<T>>> GetAllAsync<T>(
            Func<Task<IEnumerable<T>>> getAllFunc,
            string entityName)
        {
            try
            {
                IEnumerable<T> entities = await getAllFunc();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching all {entityName}");
                return StatusCode(500, new { message = $"Error fetching {entityName}", error = ex.Message });
            }
        }

        /// <summary>
        /// Handles Create pattern with validation and error handling
        /// </summary>
        protected async Task<ActionResult<T>> CreateAsync<T, TId>(
            T entity,
            Func<T, Task<T>> createFunc,
            Func<T, TId> getIdFunc,
            string entityName,
            string getByIdActionName,
            Func<T, bool>? validateFunc = null)
        {
            if (entity == null)
                return BadRequest(new { message = $"{entityName} data is required." });

            if (validateFunc != null && !validateFunc(entity))
                return BadRequest(new { message = $"Invalid {entityName} data." });

            try
            {
                T created = await createFunc(entity);
                TId id = getIdFunc(created);
                Logger.LogInformation($"{entityName} created: {id}");
                return CreatedAtAction(getByIdActionName, new { id }, created);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error creating {entityName}");
                return StatusCode(500, new { message = $"Error creating {entityName}", error = ex.Message });
            }
        }

        /// <summary>
        /// Handles Update pattern with validation and error handling
        /// </summary>
        protected async Task<ActionResult<T>> UpdateAsync<T, TId>(
            TId id,
            T entity,
            Func<TId, Task<T?>> getByIdFunc,
            Func<T, Task<T>> updateFunc,
            Action<T, TId> setIdAction,
            string entityName)
        {
            if (string.IsNullOrWhiteSpace(id?.ToString()))
                return BadRequest(new { message = $"{entityName} id is required." });

            if (entity == null)
                return BadRequest(new { message = $"{entityName} data is required." });

            try
            {
                T? existing = await getByIdFunc(id);
                if (existing == null)
                    return NotFound(new { message = $"{entityName} not found" });

                setIdAction(entity, id);
                T updated = await updateFunc(entity);
                Logger.LogInformation($"{entityName} {id} updated");
                return Ok(updated);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating {entityName} {id}");
                return StatusCode(500, new { message = $"Error updating {entityName}", error = ex.Message });
            }
        }

        /// <summary>
        /// Handles Delete pattern with validation and error handling
        /// </summary>
        protected async Task<IActionResult> DeleteAsync<T, TId>(
            TId id,
            Func<TId, Task<T?>> getByIdFunc,
            Func<TId, Task> deleteFunc,
            string entityName)
        {
            if (string.IsNullOrWhiteSpace(id?.ToString()))
                return BadRequest(new { message = $"{entityName} id is required." });

            try
            {
                T? existing = await getByIdFunc(id);
                if (existing == null)
                    return NotFound(new { message = $"{entityName} not found" });

                await deleteFunc(id);
                Logger.LogInformation($"{entityName} {id} deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error deleting {entityName} {id}");
                return StatusCode(500, new { message = $"Error deleting {entityName}", error = ex.Message });
            }
        }
    }
}

