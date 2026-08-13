using ProjectExamine.Controllers;

namespace ProjectExamine.Services
{
    /// <summary>
    /// ALTERNATIVE IMPLEMENTATION OF IProductService
    /// 
    /// This demonstrates the KEY ADVANTAGE OF DEPENDENCY INJECTION:
    /// 
    /// PROBLEM WITHOUT DI:
    /// If ProductController directly instantiated ProductService like:
    ///     private ProductService _service = new ProductService();
    /// Then to switch to a database version, you'd need to:
    /// 1. Find the ProductController code
    /// 2. Change "new ProductService()" to "new ProductServiceDatabase()"
    /// 3. Recompile and redeploy
    /// 4. You'd need to change EVERY controller that uses the service
    /// 
    /// ADVANTAGE WITH DI:
    /// Just change ONE line in Program.cs:
    ///     FROM: builder.Services.AddScoped<IProductService, ProductService>();
    ///     TO:   builder.Services.AddScoped<IProductService, ProductServiceDatabase>();
    /// 
    /// The controller doesn't know (or care) which implementation is being used!
    /// This is called "Dependency Inversion Principle" - depend on abstractions, not concretions.
    /// </summary>
    public class ProductServiceDatabase : IProductService
    {
        // ============================================
        // SIMULATED DATABASE CONNECTION
        // ============================================
        // In a real application, you'd have:
        // - Entity Framework DbContext
        // - SQL Server connection string
        // - ORM (Object-Relational Mapper)
        // 
        // For this demo, we're simulating a database with a different storage approach
        private readonly ILogger<ProductServiceDatabase> _logger;

        public ProductServiceDatabase(ILogger<ProductServiceDatabase> logger)
        {
            _logger = logger;
            _logger.LogInformation("ProductServiceDatabase initialized - Using DATABASE implementation");
        }

        /// <summary>
        /// GET ALL PRODUCTS
        /// Simulates: SELECT * FROM Products
        /// 
        /// In real scenario:
        ///     return await _dbContext.Products.AsNoTracking().ToListAsync();
        /// </summary>
        public List<Product> GetAllProducts()
        {
            _logger.LogInformation("Database: Executing SELECT * FROM Products");

            // Simulate database query with a slight delay
            System.Threading.Thread.Sleep(100);

            var products = new List<Product>
            {
                new Product { Id = 1, ProductName = "Dell Laptop", Price = 1299.99m, CreatedDate = DateTime.Now.AddDays(-30) },
                new Product { Id = 2, ProductName = "Wireless Mouse", Price = 49.99m, CreatedDate = DateTime.Now.AddDays(-20) },
                new Product { Id = 3, ProductName = "Mechanical Keyboard", Price = 129.99m, CreatedDate = DateTime.Now.AddDays(-10) },
                new Product { Id = 4, ProductName = "USB-C Hub", Price = 89.99m, CreatedDate = DateTime.Now.AddDays(-5) }
            };

            _logger.LogInformation($"Database returned {products.Count} products");
            return products;
        }

        /// <summary>
        /// GET PRODUCT BY ID
        /// Simulates: SELECT * FROM Products WHERE Id = @id
        /// </summary>
        public Product? GetProductById(int id)
        {
            _logger.LogInformation($"Database: Executing SELECT * FROM Products WHERE Id = {id}");

            // Simulate database query with delay
            System.Threading.Thread.Sleep(50);

            var products = GetAllProducts();
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning($"Database: Product with ID {id} NOT FOUND");
            }

            return product;
        }

        /// <summary>
        /// SEARCH PRODUCTS
        /// Simulates: SELECT * FROM Products WHERE ProductName LIKE '%@searchTerm%'
        /// </summary>
        public List<Product> SearchProducts(string searchTerm)
        {
            _logger.LogInformation($"Database: Executing FULL TEXT SEARCH for '{searchTerm}'");

            // Simulate full-text search with delay
            System.Threading.Thread.Sleep(150);

            var allProducts = GetAllProducts();
            var results = allProducts
                .Where(p => p.ProductName != null && 
                           p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation($"Database: Full-text search returned {results.Count} results");
            return results;
        }

        /// <summary>
        /// CREATE PRODUCT
        /// Simulates: INSERT INTO Products (ProductName, Price, CreatedDate) VALUES (...)
        /// 
        /// Real database code would be:
        ///     var product = new Product { ProductName = productName, Price = price, CreatedDate = DateTime.Now };
        ///     _dbContext.Products.Add(product);
        ///     await _dbContext.SaveChangesAsync();
        ///     return product;
        /// </summary>
        public Product CreateProduct(string productName, decimal price)
        {
            _logger.LogInformation($"Database: Executing INSERT - Name: {productName}, Price: {price}");

            // Simulate database insert with delay
            System.Threading.Thread.Sleep(200);

            var product = new Product
            {
                Id = new Random().Next(1000, 9999), // Simulate auto-generated ID from DB
                ProductName = productName,
                Price = price,
                CreatedDate = DateTime.Now
            };

            _logger.LogInformation($"Database: Product created with ID {product.Id}");
            return product;
        }

        /// <summary>
        /// UPDATE PRODUCT
        /// Simulates: UPDATE Products SET ProductName = @name, Price = @price WHERE Id = @id
        /// </summary>
        public (bool success, Product? product) UpdateProduct(int id, string productName, decimal price)
        {
            _logger.LogInformation($"Database: Executing UPDATE on Product ID {id}");

            var product = GetProductById(id);
            if (product == null)
            {
                _logger.LogWarning($"Database: Cannot update - Product ID {id} does not exist");
                return (false, null);
            }

            // Simulate database update with delay
            System.Threading.Thread.Sleep(150);

            product.ProductName = productName;
            product.Price = price;

            _logger.LogInformation($"Database: Product ID {id} updated successfully");
            return (true, product);
        }

        /// <summary>
        /// PARTIAL UPDATE PRODUCT
        /// Simulates: UPDATE Products SET <only provided fields> WHERE Id = @id
        /// </summary>
        public (bool success, Product? product) PartialUpdateProduct(int id, string? productName, decimal? price)
        {
            _logger.LogInformation($"Database: Executing PARTIAL UPDATE on Product ID {id}");

            var product = GetProductById(id);
            if (product == null)
            {
                _logger.LogWarning($"Database: Cannot partial update - Product ID {id} does not exist");
                return (false, null);
            }

            // Simulate database update with delay
            System.Threading.Thread.Sleep(100);

            if (!string.IsNullOrEmpty(productName))
            {
                _logger.LogInformation($"Database: Updating ProductName to '{productName}'");
                product.ProductName = productName;
            }

            if (price.HasValue)
            {
                _logger.LogInformation($"Database: Updating Price to {price}");
                product.Price = price.Value;
            }

            _logger.LogInformation($"Database: Partial update completed for Product ID {id}");
            return (true, product);
        }

        /// <summary>
        /// DELETE PRODUCT
        /// Simulates: DELETE FROM Products WHERE Id = @id
        /// </summary>
        public (bool success, Product? product) DeleteProduct(int id)
        {
            _logger.LogInformation($"Database: Executing DELETE from Products WHERE Id = {id}");

            var product = GetProductById(id);
            if (product == null)
            {
                _logger.LogWarning($"Database: Cannot delete - Product ID {id} does not exist");
                return (false, null);
            }

            // Simulate database delete with delay
            System.Threading.Thread.Sleep(100);

            _logger.LogInformation($"Database: Product ID {id} deleted successfully");
            return (true, product);
        }

        /// <summary>
        /// BULK DELETE ALL PRODUCTS
        /// Simulates: DELETE FROM Products
        /// </summary>
        public (bool success, int deletedCount) DeleteAllProducts()
        {
            _logger.LogWarning("Database: Executing BULK DELETE - DELETE FROM Products (all records)");

            // Simulate database bulk delete with delay
            System.Threading.Thread.Sleep(300);

            int deletedCount = 4; // Simulating 4 products deleted
            _logger.LogWarning($"Database: Bulk delete completed - {deletedCount} records removed");
            return (true, deletedCount);
        }
    }

    /// <summary>
    /// COMPARISON: CACHE IMPLEMENTATION
    /// 
    /// Another example of a different IProductService implementation
    /// This could be used when you want to cache products in-memory for performance
    /// 
    /// REAL-WORLD SCENARIO:
    /// Your boss says: "The ProductService is too slow! We need to cache results!"
    /// 
    /// WITHOUT DI:
    /// You'd refactor ProductService, mix caching logic with database logic,
    /// test everything again, and risk breaking existing functionality.
    /// 
    /// WITH DI:
    /// Create ProductServiceCached, change ONE line in Program.cs.
    /// Old implementation still exists, can be tested separately.
    /// Easy to switch back if needed!
    /// </summary>
    public class ProductServiceCached : IProductService
    {
        private readonly ILogger<ProductServiceCached> _logger;
        private readonly IProductService _innerService;
        private Dictionary<int, Product> _cache = new();
        private DateTime _cacheExpiryTime = DateTime.MinValue;
        private const int CACHE_DURATION_SECONDS = 60;

        // Notice: We're injecting ANOTHER IProductService!
        // This allows ProductServiceCached to wrap another implementation
        // (Decorator Pattern)
        // 
        // In Program.cs you could do:
        // builder.Services.AddScoped<ProductService>();
        // builder.Services.AddScoped<IProductService>(sp => 
        //     new ProductServiceCached(
        //         logger, 
        //         sp.GetRequiredService<ProductService>()
        //     )
        // );

        public ProductServiceCached(ILogger<ProductServiceCached> logger, IProductService innerService)
        {
            _logger = logger;
            _innerService = innerService;
            _logger.LogInformation("ProductServiceCached initialized - Using CACHING implementation");
        }

        private bool IsCacheExpired => DateTime.Now > _cacheExpiryTime;

        public List<Product> GetAllProducts()
        {
            _logger.LogInformation("Cache: Checking if cache is valid");

            if (!IsCacheExpired && _cache.Count > 0)
            {
                _logger.LogInformation($"Cache: CACHE HIT - Returning {_cache.Count} cached products");
                return _cache.Values.ToList();
            }

            _logger.LogInformation("Cache: CACHE MISS - Fetching from inner service");
            var products = _innerService.GetAllProducts();

            _cache = products.ToDictionary(p => p.Id);
            _cacheExpiryTime = DateTime.Now.AddSeconds(CACHE_DURATION_SECONDS);

            _logger.LogInformation($"Cache: Updated cache with {products.Count} products. Expires in {CACHE_DURATION_SECONDS}s");
            return products;
        }

        public Product? GetProductById(int id)
        {
            _logger.LogInformation($"Cache: Looking up Product ID {id}");

            if (!IsCacheExpired && _cache.TryGetValue(id, out var cachedProduct))
            {
                _logger.LogInformation($"Cache: CACHE HIT - Found Product ID {id} in cache");
                return cachedProduct;
            }

            _logger.LogInformation($"Cache: CACHE MISS - Fetching Product ID {id} from inner service");
            var product = _innerService.GetProductById(id);

            if (product != null)
            {
                _cache[id] = product;
            }

            return product;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            // Searches shouldn't be cached (results might be different each time)
            _logger.LogInformation($"Cache: Search queries bypass cache - Searching in inner service");
            return _innerService.SearchProducts(searchTerm);
        }

        public Product CreateProduct(string productName, decimal price)
        {
            _logger.LogInformation("Cache: Invalidating cache (create operation)");
            _cache.Clear();
            _cacheExpiryTime = DateTime.MinValue;

            return _innerService.CreateProduct(productName, price);
        }

        public (bool success, Product? product) UpdateProduct(int id, string productName, decimal price)
        {
            _logger.LogInformation("Cache: Invalidating cache (update operation)");
            _cache.Clear();
            _cacheExpiryTime = DateTime.MinValue;

            return _innerService.UpdateProduct(id, productName, price);
        }

        public (bool success, Product? product) PartialUpdateProduct(int id, string? productName, decimal? price)
        {
            _logger.LogInformation("Cache: Invalidating cache (partial update operation)");
            _cache.Clear();
            _cacheExpiryTime = DateTime.MinValue;

            return _innerService.PartialUpdateProduct(id, productName, price);
        }

        public (bool success, Product? product) DeleteProduct(int id)
        {
            _logger.LogInformation("Cache: Invalidating cache (delete operation)");
            _cache.Clear();
            _cacheExpiryTime = DateTime.MinValue;

            return _innerService.DeleteProduct(id);
        }

        public (bool success, int deletedCount) DeleteAllProducts()
        {
            _logger.LogInformation("Cache: Invalidating cache (bulk delete operation)");
            _cache.Clear();
            _cacheExpiryTime = DateTime.MinValue;

            return _innerService.DeleteAllProducts();
        }
    }

    /// <summary>
    /// MOCK IMPLEMENTATION FOR UNIT TESTING
    /// 
    /// This is another great example of DI advantages!
    /// 
    /// In unit tests, you don't want to:
    /// - Connect to a real database
    /// - Wait for network calls
    /// - Deal with test data cleanup
    /// 
    /// Instead, inject a mock that returns fixed test data instantly!
    /// 
    /// USAGE IN UNIT TEST:
    /// var mockService = new ProductServiceMock();
    /// var controller = new ProductController(logger, mockService);
    /// var result = controller.GetAllProducts();
    /// // Assert the result...
    /// 
    /// The controller doesn't know it's using a mock!
    /// This is TESTABILITY - a major DI benefit.
    /// </summary>
    public class ProductServiceMock : IProductService
    {
        private List<Product> _testData = new()
        {
            new Product { Id = 100, ProductName = "Test Product 1", Price = 99.99m, CreatedDate = DateTime.Now },
            new Product { Id = 101, ProductName = "Test Product 2", Price = 199.99m, CreatedDate = DateTime.Now }
        };

        public List<Product> GetAllProducts() => _testData;
        public Product? GetProductById(int id) => _testData.FirstOrDefault(p => p.Id == id);
        public List<Product> SearchProducts(string searchTerm) => 
            _testData.Where(p => p.ProductName != null && 
                           p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        public Product CreateProduct(string productName, decimal price) =>
            new Product { Id = 999, ProductName = productName, Price = price, CreatedDate = DateTime.Now };
        public (bool, Product?) UpdateProduct(int id, string productName, decimal price) =>
            (true, new Product { Id = id, ProductName = productName, Price = price, CreatedDate = DateTime.Now });
        public (bool, Product?) PartialUpdateProduct(int id, string? productName, decimal? price) =>
            (true, new Product { Id = id, ProductName = productName ?? "Updated", Price = price ?? 0, CreatedDate = DateTime.Now });
        public (bool, Product?) DeleteProduct(int id) => (true, null);
        public (bool, int) DeleteAllProducts() => (true, _testData.Count);
    }
}
