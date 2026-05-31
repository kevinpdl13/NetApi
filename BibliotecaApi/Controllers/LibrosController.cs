using BibliotecaApi.Datos;
using BibliotecaApi.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers;
[ApiController]
[Route("api/libros")]
public class LibrosController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public LibrosController(ApplicationDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Libro>> Get()
    {
        return await context.Libros.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Libro>> Get(int id)
    {
        var libroEncontrado = await context.Libros
            .Include(x => x.Autor)
                .ThenInclude(x1 => x1.Libros)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (libroEncontrado is null)
        {
            return NotFound();
        }

        return libroEncontrado;
    }

    [HttpPost]
    public async Task<ActionResult> Post(Libro libro)
    {
        var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);

        if (!existeAutor)
        {
            ModelState.AddModelError(nameof(libro.AutorId), $"El autor de id {libro.AutorId} no existe");
            return ValidationProblem();
        }

        context.Add(libro);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put (int id, Libro libro)
    {
        if(id != libro.Id)
        {
            return BadRequest("Los ids deben de coincidir");
        }

        var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);

        if (!existeAutor)
        {
            return BadRequest($"El autor de id {libro.AutorId} no existe");
        }

        context.Update(libro);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> delete(int id)
    {
        var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();

        if (registrosBorrados == 0)
        {
            return NotFound();
        }

        return Ok("registro Borrado");
    }
}
