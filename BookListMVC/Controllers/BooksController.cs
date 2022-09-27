using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BookListMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BookListMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Book Book { get; set; }
        public BooksController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Book = new Book();
            if (id == null)
            {
                //create
                return View(Book);
            }
            //update
            Book = _db.Books.FirstOrDefault(u => u.Id == id);
            if (Book == null)
            {
                return NotFound();
            }
            return View(Book);
        }

        public IActionResult Export(int id, string format)
        {
            Book = _db.Books.FirstOrDefault(u => u.Id == id);

            if (Book == null)
            {
                return NotFound();
            }
            else
            {
                MemoryStream fileContents = SerializeBook(Book, format);

                return format switch
                {
                    "json" => File(fileContents, "application/json", $"{Book.Author} - {Book.Name}.json"),
                    "xml" => File(fileContents, "application/xml", $"{Book.Author} - {Book.Name}.xml"),
                    "yaml" => File(fileContents, "text/x-yaml", $"{Book.Author} - {Book.Name}.yaml"),
                    _ => NotFound(),
                };
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert()
        {
            if (ModelState.IsValid)
            {
                if (Book.Id == 0)
                {
                    //create
                    _db.Books.Add(Book);
                }
                else
                {
                    _db.Books.Update(Book);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(Book);
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Books.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var bookFromDb = await _db.Books.FirstOrDefaultAsync(u => u.Id == id);
            if (bookFromDb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _db.Books.Remove(bookFromDb);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

        private MemoryStream SerializeBook(Book book, string format)
        {
            return format switch
            {
                "json" => GetJsonBook(book),
                "xml" => GetXmlBook(book),
                "yaml" => GetYamlBook(book),
                _ => throw new Exception("Invalid export format.")
            };
        }
        private MemoryStream GetJsonBook(Book book)
        {
            string jsonString = JsonSerializer.Serialize(book);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            MemoryStream memory = new MemoryStream(jsonBytes);

            return memory;
        }

        private MemoryStream GetXmlBook(Book book)
        {
            string xmlString = String.Empty;

            XmlSerializer xmlSerializer = new XmlSerializer(book.GetType());

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(xmlWriter, book);
                    xmlString = stringWriter.ToString();
                }
            }

            byte[] jsonBytes = Encoding.UTF8.GetBytes(xmlString);

            MemoryStream memory = new MemoryStream(jsonBytes);

            return memory;
        }

        private MemoryStream GetYamlBook(Book book)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yamlString = serializer.Serialize(book);
            byte[] yamlBytes = Encoding.UTF8.GetBytes(yamlString);

            MemoryStream memory = new MemoryStream(yamlBytes);

            return memory;
        }
    }
}