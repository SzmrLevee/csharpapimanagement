using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApiController.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApiController.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TodoController : ControllerBase
{
    readonly IDataStore dataStore;
    readonly IValidator<TodoItem> validator;

#pragma warning disable IDE0290
    public TodoController(IDataStore dataStore, IValidator<TodoItem> validator)
    {
        this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }
#pragma warning restore IDE0290

    // GET: api/<TodoController>
    [HttpGet]
    public IEnumerable<TodoItem> Get()
    {
        return ((IItemStore<TodoItem>)dataStore).GetAll();
    }

    // GET api/<TodoController>/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult Get(int id)
    {
        var item = Get().FirstOrDefault(x => x.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    // POST api/<TodoController>
    [HttpPost]
    public IActionResult Post([FromBody] TodoItem value)
    {
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }
        var newId = 1;
        try
        {
            newId = Get().Max(x => x.Id) + 1;
        }
        catch { }
        value.Id = newId;
        dataStore.Add(value);
        return Ok(value);
    }

    // PUT api/<TodoController>/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] TodoItem value)
    {
        var result = validator.Validate(value);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }
        value.Id = id;
        if(dataStore.Update(value))
        {
            return NotFound();
        }
        return Ok(value);
    }

    // DELETE api/<TodoController>/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var item = Get().FirstOrDefault(x => x.Id == id);
        if (item == null) { return NotFound(); }
        dataStore.Delete(item);
        return Ok();
    }
}
