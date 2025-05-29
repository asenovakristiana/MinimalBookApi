using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBookService, BookService>();

var app = builder.Build();

app.UseStatusCodePages(async statusCodeContext
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
        .ExecuteAsync(statusCodeContext.HttpContext));

{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/books", (IBookService bookService) =>
    TypedResults.Ok(bookService.GetBooks()))
.WithName("GetBooks")
.WithOpenApi(x => new OpenApiOperation(x)
{
    Summary = "Get Library Books",
    Description = "Returns information about all the available books from the Amy's library.",
    Tags = new List<OpenApiTag> { new() { Name = "Amy's Library" } }
});

app.MapGet("/books/{id}", Results<Ok<Book>, NotFound> (IBookService bookService, int id) =>
        bookService.GetBook(id) is { } book
            ? TypedResults.Ok(book)
            : TypedResults.NotFound()
    )
    .WithName("GetBookById")
    .WithOpenApi(x => new OpenApiOperation(x)
    {
        Summary = "Get Library Book By Id",
        Description = "Returns information about selected book from the Amy's library.",
        Tags = new List<OpenApiTag> { new() { Name = "Amy's Library" } }
    });

app.Run();