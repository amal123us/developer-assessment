using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger)
        {
            _context = context ?? throw new ArgumentException(nameof(context));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetTodoItems()
        {
            var results = await _context.TodoItems.Where(x => !x.IsCompleted).ToListAsync();
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            var result = await _context.TodoItems.FindAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // PUT: api/TodoItems/... 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
                return BadRequest();

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemIdExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        } 

        // PATCH: api/TodoItems/... 
        [HttpPatch("{id}")]
        public async Task<IActionResult> CompleteTodoItem(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
                return BadRequest();

            _context.Entry(todoItem).Property(x=>x.IsCompleted).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemIdExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        } 

        // POST: api/TodoItems 
        [HttpPost]
        public async Task<IActionResult> AddTodoItem(TodoItem todoItem)
        {
            if (string.IsNullOrEmpty(todoItem?.Description))
                return BadRequest("Description is required");
            else if (TodoItemDescriptionExists(todoItem.Description))
                return BadRequest("Description already exists");

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
             
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        } 

        private bool TodoItemIdExists(Guid id) => _context.TodoItems.Any(x => x.Id == id);

        private bool TodoItemDescriptionExists(string description)=> _context.TodoItems
                   .Any(x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
    }
}
