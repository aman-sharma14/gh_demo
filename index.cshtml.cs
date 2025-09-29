using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Todo.Data;
using Todo.Models;

namespace Todo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Todo.Data.TodoContext _context;

        // The database context is injected into the page model via the constructor.
        public IndexModel(Todo.Data.TodoContext context)
        {
            _context = context;
        }

        public IList<TodoItem> TodoItem { get; set; } = default!;

        // This method runs for a standard GET request when the page is first loaded.
        // It fetches the initial list of tasks to display.
        public async Task OnGetAsync()
        {
            TodoItem = await _context.TodoItem.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        // --- Handler Method for Adding a Task ---
        // This method is specifically targeted by the 'AddTask' AJAX POST request.
        // The 'title' parameter is automatically bound from the form data sent by jQuery's .serialize().
        public async Task<JsonResult> OnPostAddTaskAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new JsonResult(new { success = false, message = "Title must not be empty." });
            }

            TodoItem newtask = new(title);

            _context.TodoItem.Add(newtask);
            await _context.SaveChangesAsync();

            // Return a JSON object to the 'success' callback in our jQuery code.
            return new JsonResult(new { success = true, message = "Task added successfully!" });
        }

        // --- Handler Method for Fetching All Tasks ---
        // This method is targeted by the 'FetchTasks' AJAX GET request.
        public async Task<JsonResult> OnGetFetchTasksAsync()
        {
            var tasks = await _context.TodoItem.OrderByDescending(x => x.CreatedAt).ToListAsync();
            // This returns the list of tasks as a JSON array.
            return new JsonResult(tasks);
        }

        // --- Handler Method for Deleting a Task ---
        // Targeted by the 'DeleteTask' AJAX POST request.
        // The 'id' parameter is bound from the data object: { id: taskId }.
        public async Task<JsonResult> OnPostDeleteTaskAsync(int id)
        {
            var item = await _context.TodoItem.FirstOrDefaultAsync(x => x.Id == id);

            if (item != null)
            {
                _context.TodoItem.Remove(item);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            
            return new JsonResult(new { success = false });
        }

        // --- Handler Method for Toggling Task Status ---
        // Targeted by the 'ToggleTask' AJAX POST request.
        public async Task<JsonResult> OnPostToggleTaskAsync(int id)
        {
            var item = await _context.TodoItem.FirstOrDefaultAsync(x => x.Id == id);

            if (item != null)
            {
                // Flip the boolean value.
                item.IsCompleted = !item.IsCompleted;
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            
            return new JsonResult(new { success = false });
        }
    }
}
