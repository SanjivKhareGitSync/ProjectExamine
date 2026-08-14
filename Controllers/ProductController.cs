using Microsoft.AspNetCore.Mvc;

namespace ProjectExamine.Controllers
{
    /// <summary>
    /// [ApiController] - This attribute indicates that this class is an API controller
    /// It enables automatic model validation and binds parameters from request bodies automatically
    /// It also provides automatic HTTP 400 responses for invalid requests
    /// </summary>
    [ApiController]

    /// <summary>
    /// [Route] - This attribute defines the base route/URL path for this controller
    /// [controller] is a token that automatically replaces with the controller name ('Product')
    /// So this controller will respond to requests at: /product, /product/1, /product/search, etc.
    /// </summary>
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        // ============================================
        // DEPENDENCY INJECTION SETUP
        // ============================================
        // These are readonly fields that will be injected automatically by ASP.NET Core
        // Dependency Injection allows us to pass dependencies into the controller
        // rather than creating them inside (loose coupling, better testability)

        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;

        /// <summary>
        /// Constructor - This is where Dependency Injection happens
        /// When ASP.NET Core creates an instance of this controller, it automatically
        /// looks at the constructor parameters and injects the required services
        /// 
        /// HOW IT WORKS:
        /// 1. ASP.NET Core sees we need ILogger and IProductService
        /// 2. It checks the DI container (registered in Program.cs) for these services
        /// 3. If registered, it creates an instance and passes them to the constructor
        /// 4. If not registered, the app will throw an error at startup
        /// 
        /// SETUP IN Program.cs:
        /// builder.Services.AddLogging();  // For ILogger
        /// builder.Services.AddScoped<IProductService, ProductService>();  // For IProductService
        /// </summary>
        public ProductController(ILogger<ProductController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        // ============================================
        // GET OPERATIONS (Read Data)
        // ============================================

        /// <summary>
        /// GET /product
        /// [HttpGet] - This HTTP verb attribute makes this method respond to GET requests
        /// GET is used to retrieve/read data from the server
        /// RETURN TYPE: IActionResult is a flexible return type that can return different HTTP responses
        /// Common IActionResult implementations:
        ///   - Ok(data) = HTTP 200 with data
        ///   - NotFound() = HTTP 404
        ///   - BadRequest() = HTTP 400
        ///   - Unauthorized() = HTTP 401
        /// </summary>
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            _logger.LogInformation("GetAllProducts called - Retrieving all products from database");

            // Calling injected service to get products
            var products = _productService.GetAllProducts();

            // Ok() returns HTTP 200 status with the data in the response body
            return Ok(products);
        }

        /// <summary>
        /// GET /product/1
        /// [HttpGet("{id}")] - The {id} in curly braces is a route parameter
        /// This makes the method respond to: /product/1, /product/5, /product/999 etc
        /// The id value is automatically extracted and passed to the method parameter
        /// 
        /// int id - This will receive the numeric value from the URL (e.g., 1, 2, 3)
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            // Validate the input - id should be a positive number
            if (id <= 0)
            {
                // BadRequest() returns HTTP 400 - Client sent bad data
                return BadRequest("Product ID must be greater than 0");
            }

            _logger.LogInformation($"GetProductById called with ID: {id}");

            var product = _productService.GetProductById(id);

            // If product not found, return 404
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            // Return the found product with HTTP 200
            return Ok(product);
        }

        /// <summary>
        /// GET /product/search?name=laptop
        /// [HttpGet("search")] - Custom route segment added after the base route
        /// This makes the method respond to: /product/search
        /// The "?name=laptop" part is a query parameter, not a route parameter
        /// Query parameters are optional and extracted by the method parameter name
        /// 
        /// string? searchTerm - The ? means it's nullable (parameter is optional)
        /// The name ties to the query parameter name in the URL
        /// </summary>
        [HttpGet("search")]
        public IActionResult SearchProducts([FromQuery] string? searchTerm)
        {
            // If search term is empty, return all products
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            _logger.LogInformation($"SearchProducts called with search term: {searchTerm}");

            var products = _productService.SearchProducts(searchTerm);

            return Ok(products);
        }

        // ============================================
        // POST OPERATIONS (Create Data)
        // ============================================

        /// <summary>
        /// POST /product
        /// [HttpPost] - Makes this method respond to POST requests
        /// POST is used to create/insert new data on the server
        /// 
        /// CreateProductRequest model - The data received from the client
        /// [FromBody] attribute tells ASP.NET Core to read the request body
        /// It automatically deserializes JSON from the request into the object
        /// 
        /// RETURN TYPE: CreatedAtAction() is specialized for POST operations
        /// It returns HTTP 201 (Created) with the newly created resource
        /// and a Location header pointing to the new resource
        /// </summary>
        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductRequest request)
        {
            // Validate the request data
            if (string.IsNullOrEmpty(request?.ProductName))
            {
                // ModelState automatically captures validation errors
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"CreateProduct called with name: {request.ProductName}");

            // Call service to create the product and get back the created product
            var createdProduct = _productService.CreateProduct(request.ProductName, request.Price);

            // CreatedAtAction() returns:
            // - HTTP 201 Created status
            // - Location header: /product/{id} (where to find the new resource)
            // - The created product object in the response body
            // Parameters: (actionName, routeValues, value)
            return CreatedAtAction(nameof(GetProductById), 
                new { id = createdProduct.Id }, 
                createdProduct);
        }

        // ============================================
        // PUT OPERATIONS (Update/Replace Complete Resource)
        // ============================================

        /// <summary>
        /// PUT /product/1
        /// [HttpPut("{id}")] - Makes this method respond to PUT requests
        /// PUT is used to completely replace/update an existing resource
        /// The entire resource must be provided in the request body
        /// 
        /// DIFFERENCE BETWEEN PUT and PATCH:
        /// PUT = Replace the entire resource (idempotent)
        /// PATCH = Partial update, only update specific fields
        /// 
        /// RETURN TYPE: IActionResult allows multiple possible responses:
        /// - Ok() for successful update - HTTP 200
        /// - NotFound() if resource doesn't exist - HTTP 404
        /// - BadRequest() if data is invalid - HTTP 400
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
            {
                return BadRequest("Product ID must be greater than 0");
            }

            if (request == null || string.IsNullOrEmpty(request.ProductName))
            {
                return BadRequest("Product name cannot be empty");
            }

            _logger.LogInformation($"UpdateProduct called with ID: {id}");

            var (success, updatedProduct) = _productService.UpdateProduct(id, request.ProductName, request.Price);

            if (!success)
            {
                return NotFound($"Product with ID {id} not found");
            }

            // Ok() returns HTTP 200 with the updated resource
            return Ok(updatedProduct);
        }

        // ============================================
        // PATCH OPERATIONS (Partial Update)
        // ============================================

        /// <summary>
        /// PATCH /product/1
        /// [HttpPatch("{id}")] - Makes this method respond to PATCH requests
        /// PATCH is used to partially update a resource (only provided fields are updated)
        /// This is different from PUT which requires the entire resource
        /// 
        /// USE PATCH WHEN:
        /// - You want to update just one or two fields
        /// - You don't require the client to send the entire resource
        /// - You want a more flexible update mechanism
        /// 
        /// EXAMPLE:
        /// PUT:   Must send { ProductName: "...", Price: ... } (complete object)
        /// PATCH: Can send just { Price: 99.99 } (only the field to update)
        /// </summary>
        [HttpPatch("{id}")]
        public IActionResult PartialUpdateProduct(int id, [FromBody] PatchProductRequest request)
        {
            if (id <= 0)
            {
                return BadRequest("Product ID must be greater than 0");
            }

            _logger.LogInformation($"PartialUpdateProduct called with ID: {id}");

            // Only update fields that are provided (not null)
            var (success, updatedProduct) = _productService.PartialUpdateProduct(
                id, 
                request.ProductName, 
                request.Price
            );

            if (!success)
            {
                return NotFound($"Product with ID {id} not found");
            }

            return Ok(updatedProduct);
        }

        // ============================================
        // DELETE OPERATIONS (Remove Data)
        // ============================================

        /// <summary>
        /// DELETE /product/1
        /// [HttpDelete("{id}")] - Makes this method respond to DELETE requests
        /// DELETE is used to remove/delete a resource from the server
        /// 
        /// IDEMPOTENT PRINCIPLE:
        /// DELETE should be idempotent - calling it multiple times has the same effect
        /// - First call: Deletes the resource, returns 200/204
        /// - Second call: Resource already deleted, still returns 200/204
        /// 
        /// RETURN TYPE: IActionResult
        /// Common responses:
        /// - NoContent() = HTTP 204 (success, no data to return)
        /// - Ok() = HTTP 200 (success, returns deleted object)
        /// - NotFound() = HTTP 404 (resource doesn't exist)
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Product ID must be greater than 0");
            }

            _logger.LogInformation($"DeleteProduct called with ID: {id}");

            var (success, deletedProduct) = _productService.DeleteProduct(id);

            if (!success)
            {
                return NotFound($"Product with ID {id} not found");
            }

            // NoContent() returns HTTP 204 - successful deletion, no content in response
            // Alternatively, you can use Ok(deletedProduct) to return the deleted object
            return NoContent();
        }

        // ============================================
        /// <summary>
        /// DELETE /product
        /// [HttpDelete] - Delete without ID parameter
        /// This is a bulk/batch delete operation
        /// It demonstrates deleting multiple resources
        /// </summary>
        [HttpDelete("bulk")]
        public IActionResult DeleteAllProducts()
        {
            _logger.LogWarning("DeleteAllProducts called - Performing bulk delete operation");

            var (success, deletedCount) = _productService.DeleteAllProducts();

            if (!success)
            {
                return BadRequest("Failed to delete products");
            }

            return Ok(new { message = $"Successfully deleted {deletedCount} products" });
        }
    }

    // ============================================
    // REQUEST/RESPONSE MODELS (DTOs - Data Transfer Objects)
    // ============================================
    // These classes define the structure of data sent by clients and returned by the API
    // They help with:
    // - Validation - properties can have validation attributes
    // - Consistency - ensures expected data format
    // - Security - don't expose database models directly

    public class CreateProductRequest
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateProductRequest
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class PatchProductRequest
    {
        // These are nullable because in PATCH, fields might not be provided
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
    }

    // ============================================
    // SERVICE INTERFACE (For Dependency Injection)
    // ============================================
    // This interface defines the contract for the service
    // It allows us to depend on an abstraction, not a concrete implementation
    // This makes testing easier (we can mock this interface)

    public interface IProductService
    {
        List<Product> GetAllProducts();
        Product? GetProductById(int id);
        List<Product> SearchProducts(string searchTerm);
        Product CreateProduct(string productName, decimal price);
        (bool success, Product? product) UpdateProduct(int id, string productName, decimal price);
        (bool success, Product? product) PartialUpdateProduct(int id, string? productName, decimal? price);
        (bool success, Product? product) DeleteProduct(int id);
        (bool success, int deletedCount) DeleteAllProducts();
    }

    // ============================================
    // PRODUCT MODEL
    // ============================================

    public class Product
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ============================================
    // SAMPLE SERVICE IMPLEMENTATION
    // ============================================
    // This is a simple in-memory implementation for demonstration
    // In a real app, this would interact with a database using Entity Framework or Dapper

    public class ProductService : IProductService
    {
        // In-memory storage for demo purposes
        private static List<Product> _products = new()
        {
            new Product { Id = 1, ProductName = "Laptop", Price = 999.99m, CreatedDate = DateTime.Now },
            new Product { Id = 2, ProductName = "Mouse", Price = 29.99m, CreatedDate = DateTime.Now },
            new Product { Id = 3, ProductName = "Keyboard", Price = 79.99m, CreatedDate = DateTime.Now }
        };

        private static int _nextId = 4;

        public List<Product> GetAllProducts()
        {
            return _products;
        }

        public Product? GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            return _products
                .Where(p => p.ProductName != null && p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Product CreateProduct(string productName, decimal price)
        {
            var product = new Product
            {
                Id = _nextId++,
                ProductName = productName,
                Price = price,
                CreatedDate = DateTime.Now
            };
            _products.Add(product);
            return product;
        }

        public (bool success, Product? product) UpdateProduct(int id, string productName, decimal price)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return (false, null);

            product.ProductName = productName;
            product.Price = price;
            return (true, product);
        }

        public (bool success, Product? product) PartialUpdateProduct(int id, string? productName, decimal? price)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return (false, null);

            if (!string.IsNullOrEmpty(productName))
                product.ProductName = productName;

            if (price.HasValue)
                product.Price = price.Value;

            return (true, product);
        }

        public (bool success, Product? product) DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return (false, null);

            _products.Remove(product);
            return (true, product);
        }

        public (bool success, int deletedCount) DeleteAllProducts()
        {
            int count = _products.Count;
            _products.Clear();
            return (true, count);
        }
    }
}
