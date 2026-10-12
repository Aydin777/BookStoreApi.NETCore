
using Microsoft.AspNetCore.Mvc;
using BookStoreApi.Models;

namespace BookStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        // Kitablar yaddaşda saxlanılır
        private static List<Book> books = new List<Book>
        {
            new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", Category = "Programming", Price = 35, Stock = 10, PageCount = 464, PublishedYear = 2008, IsAvailable = true },
            new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Category = "Programming", Price = 40, Stock = 8, PageCount = 352, PublishedYear = 1999, IsAvailable = true },
            new Book { Id = 3, Title = "Atomic Habits", Author = "James Clear", Category = "Self-Help", Price = 25, Stock = 12, PageCount = 320, PublishedYear = 2018, IsAvailable = true },
            new Book { Id = 4, Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Fantasy", Price = 20, Stock = 0, PageCount = 310, PublishedYear = 1937, IsAvailable = false },
            new Book { Id = 5, Title = "1984", Author = "George Orwell", Category = "Novel", Price = 15, Stock = 5, PageCount = 328, PublishedYear = 1949, IsAvailable = true },
            new Book { Id = 6, Title = "The Martian", Author = "Andy Weir", Category = "Science Fiction", Price = 30, Stock = 7, PageCount = 369, PublishedYear = 2011, IsAvailable = true }
        };

        // Məlumatların düzgünlüyünü yoxlayırıq
        private bool IsValidBook(Book book)
        {
            return !string.IsNullOrWhiteSpace(book.Title)
                && !string.IsNullOrWhiteSpace(book.Author)
                && !string.IsNullOrWhiteSpace(book.Category)
                && book.Price > 0
                && book.Stock >= 0
                && book.PageCount > 0
                && book.PublishedYear >= 1450
                && book.PublishedYear <= DateTime.Now.Year;
        }

        // 1. Bütün kitabları qaytarır
        [HttpGet]
        public ActionResult<List<Book>> GetAll()
        {
            return Ok(books);
        }

        // 2. ID-yə görə kitabı tapır
        [HttpGet("{id:int}")]
        public ActionResult<Book> GetById(int id)
        {
            var book = books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Kitab tapılmadı.");

            return Ok(book);
        }

        // 3. Yeni kitab əlavə edir
        [HttpPost]
        public ActionResult<Book> Create(Book book)
        {
            if (!IsValidBook(book))
                return BadRequest("Kitab məlumatları düzgün deyil.");

            book.Id = books.Count == 0
                ? 1
                : books.Max(b => b.Id) + 1;

            books.Add(book);

            return CreatedAtAction(
                nameof(GetById),
                new { id = book.Id },
                book
            );
        }

        // 4. Kitabın bütün məlumatlarını yeniləyir
        [HttpPut("{id:int}")]
        public ActionResult<Book> Update(int id, Book updatedBook)
        {
            var book = books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Kitab tapılmadı.");

            if (!IsValidBook(updatedBook))
                return BadRequest("Kitab məlumatları düzgün deyil.");

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Category = updatedBook.Category;
            book.Price = updatedBook.Price;
            book.Stock = updatedBook.Stock;
            book.PageCount = updatedBook.PageCount;
            book.PublishedYear = updatedBook.PublishedYear;
            book.IsAvailable = updatedBook.IsAvailable;

            return Ok(book);
        }

        // 5. Yalnız stok sayını yeniləyir
        [HttpPatch("{id:int}/stock")]
        public ActionResult<Book> UpdateStock(int id, [FromQuery] int stock)
        {
            var book = books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Kitab tapılmadı.");

            if (stock < 0)
                return BadRequest("Stok mənfi ola bilməz.");

            book.Stock = stock;

            return Ok(book);
        }

        // 6. Kitabı silir
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var book = books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Kitab tapılmadı.");

            books.Remove(book);

            return NoContent();
        }

        // 7. Kitab adına görə axtarış
        [HttpGet("search")]
        public ActionResult<List<Book>> Search([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Kitab adı daxil edilməlidir.");

            var result = books
                .Where(b => b.Title.Contains(
                    title,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(result);
        }

        // 8. Müəllifə görə axtarış
        [HttpGet("author")]
        public ActionResult<List<Book>> SearchByAuthor([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Müəllif adı daxil edilməlidir.");

            var result = books
                .Where(b => b.Author.Contains(
                    name,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(result);
        }

        // 9. Kateqoriyaya görə filtr
        [HttpGet("category/{category}")]
        public ActionResult<List<Book>> GetByCategory(string category)
        {
            var result = books
                .Where(b => b.Category.Equals(
                    category,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(result);
        }

        // 10. Qiymət aralığına görə filtr
        [HttpGet("filter")]
        public ActionResult<List<Book>> FilterByPrice(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < minPrice)
                return BadRequest("Qiymət aralığı düzgün deyil.");

            var result = books
                .Where(b => b.Price >= minPrice && b.Price <= maxPrice)
                .ToList();

            return Ok(result);
        }

        // 11. Stokda olan və aktiv kitablar
        [HttpGet("available")]
        public ActionResult<List<Book>> GetAvailable()
        {
            var result = books
                .Where(b => b.Stock > 0 && b.IsAvailable)
                .ToList();

            return Ok(result);
        }

        // 12. Nəşr ilinə görə filtr
        [HttpGet("year/{year:int}")]
        public ActionResult<List<Book>> GetByYear(int year)
        {
            if (year < 1450 || year > DateTime.Now.Year)
                return BadRequest("Nəşr ili düzgün deyil.");

            var result = books
                .Where(b => b.PublishedYear == year)
                .ToList();

            return Ok(result);
        }

        // 13. Qiymətə görə sıralama
        [HttpGet("sorted")]
        public ActionResult<List<Book>> SortByPrice(
            [FromQuery] string direction = "asc")
        {
            if (direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
                return Ok(books.OrderBy(b => b.Price).ToList());

            if (direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                return Ok(books.OrderByDescending(b => b.Price).ToList());

            return BadRequest("Direction asc və ya desc olmalıdır.");
        }

        // 14. Kitab statistikası
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var statistics = new
            {
                TotalBooks = books.Count,
                TotalStock = books.Sum(b => b.Stock),
                AveragePrice = books.Count > 0
                    ? books.Average(b => b.Price)
                    : 0,
                MinimumPrice = books.Count > 0
                    ? books.Min(b => b.Price)
                    : 0,
                MaximumPrice = books.Count > 0
                    ? books.Max(b => b.Price)
                    : 0
            };

            return Ok(statistics);
        }
    }
}
