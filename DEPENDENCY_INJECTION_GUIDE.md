# Dependency Injection (DI) - Complete Learning Guide

## What is Dependency Injection?

Dependency Injection is a software design pattern that makes your code more flexible, testable, and maintainable by **inverting the control** of how dependencies are created and provided to classes.

---

## The Problem WITHOUT Dependency Injection

### ❌ BAD: Tightly Coupled Code

```csharp
public class ProductController : ControllerBase
{
	private ProductService _service = new ProductService();  // HARDCODED!

	[HttpGet]
	public IActionResult GetAllProducts()
	{
		return Ok(_service.GetAllProducts());
	}
}
```

### Problems:
1. **Can't switch implementations** - If boss says "Use database instead of in-memory", you must find and change this line
2. **Hard to test** - You can't inject a mock for unit testing
3. **Tight coupling** - ProductController and ProductService are tightly bound
4. **Code duplication** - Every controller that needs ProductService repeats the `new ProductService()` line
5. **Difficult to manage** - Multiple places to change if service needs different configuration

---

## The Solution WITH Dependency Injection

### ✅ GOOD: Loosely Coupled Code

```csharp
public class ProductController : ControllerBase
{
	private readonly IProductService _service;

	// Dependencies are provided through the constructor
	public ProductController(IProductService service)
	{
		_service = service;  // Received via DI
	}

	[HttpGet]
	public IActionResult GetAllProducts()
	{
		return Ok(_service.GetAllProducts());
	}
}
```

### Register in Program.cs:
```csharp
// This ONE line controls which implementation is used
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## REAL-WORLD Advantage #1: Easy to Switch Implementations

### Scenario: Your boss says "The API is too slow! Cache the results!"

#### WITHOUT DI (Nightmare 😱):
```csharp
// You need to change EVERY file that uses ProductService
// File 1: ProductController.cs
new ProductServiceCached(new ProductServiceMemory());  // Change this

// File 2: CategoryController.cs
new ProductServiceCached(new ProductServiceMemory());  // Change this

// File 3: InventoryService.cs
new ProductServiceCached(new ProductServiceMemory());  // Change this

// ... and 10 more files!
// Risk of forgetting a file, bugs, inconsistency
```

#### WITH DI (Easy! 💚):
```csharp
// In Program.cs - ONLY ONE PLACE TO CHANGE
// Just uncomment one line and comment another:

// builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductService, ProductServiceCached>();

// Done! All controllers automatically get ProductServiceCached
// ProductController doesn't change at all!
```

---

## REAL-WORLD Advantage #2: Easy to Test with Mock Objects

### Scenario: You need to unit test ProductController without a database

#### WITHOUT DI (Can't do it! 😢):
```csharp
[TestMethod]
public void TestGetProduct()
{
	var controller = new ProductController();  // Creates real ProductService
	// This connects to actual database! ❌
	// Slow, unreliable, test data issues

	var result = controller.GetProductById(1);
	Assert.IsNotNull(result);
}
```

#### WITH DI (Easy Testing! 💚):
```csharp
[TestMethod]
public void TestGetProduct()
{
	// Inject a mock that returns test data instantly
	var mockService = new ProductServiceMock();
	var controller = new ProductController(mockService);  // Mock injected
	// No database! Fast, reliable, predictable ✓

	var result = controller.GetProductById(1);
	Assert.IsNotNull(result);
}
```

---

## REAL-WORLD Advantage #3: Works with Multiple Implementations

In your project now, you have:

### 1. **ProductService** (In-Memory)
```csharp
// Best for: Local testing, development
// Speed: ⚡⚡⚡ Very Fast
// Features: Stores in program memory, resets on restart
builder.Services.AddScoped<IProductService, ProductService>();
```

### 2. **ProductServiceDatabase** (Real Database)
```csharp
// Best for: Production environments
// Speed: ⚡⚡ Moderate (database latency)
// Features: Persistent storage, multiple users
builder.Services.AddScoped<IProductService, ProductServiceDatabase>();
```

### 3. **ProductServiceCached** (With Caching)
```csharp
// Best for: High-traffic scenarios
// Speed: ⚡⚡⚡ Very Fast (reads from cache)
// Features: Wraps another service with caching layer
builder.Services.AddScoped<IProductService, ProductServiceCached>();
```

### 4. **ProductServiceMock** (For Testing)
```csharp
// Best for: Unit tests
// Speed: ⚡⚡⚡ Instant
// Features: Returns fixed test data
builder.Services.AddScoped<IProductService, ProductServiceMock>();
```

---

## How to Switch Implementations

### Step 1: Look at Program.cs
```csharp
// Current line:
builder.Services.AddScoped<IProductService, ProductService>();
```

### Step 2: Comment it out and try a different one
```csharp
// OLD (In-Memory):
// builder.Services.AddScoped<IProductService, ProductService>();

// NEW (Database):
builder.Services.AddScoped<IProductService, ProductServiceDatabase>();
```

### Step 3: Run the application
- All API endpoints work the same way
- But they use different logic under the hood
- Check the logs to see which implementation is active:
  - In-memory logs: No delays, simple messages
  - Database logs: Contains "Database:", ~50-200ms delays
  - Cached logs: Contains "Cache:", shows cache hits

---

## Understanding DI Container Lifetimes

### AddScoped (Most Common)
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```
- **One instance per HTTP request**
- Best for: Database services, logging services
- Reason: Each request gets fresh state, thread-safe

### AddSingleton
```csharp
builder.Services.AddSingleton<IProductService, ProductService>();
```
- **One instance for the entire application**
- Best for: Stateless utility services, configuration
- Warning: Share by all requests, thread-safe or stateless only

### AddTransient
```csharp
builder.Services.AddTransient<IProductService, ProductService>();
```
- **New instance every time it's requested**
- Best for: Temporary services, lightweight objects
- Warning: More memory usage

---

## Constructor Injection in Action

### When you have a constructor like this:
```csharp
public ProductController(
	ILogger<ProductController> logger,
	IProductService productService
)
{
	_logger = logger;
	_productService = productService;
}
```

### ASP.NET Core does this automatically:
```
1. Receives HTTP request for /product
2. Needs to create ProductController
3. Checks constructor parameters:
   - Needs ILogger<ProductController> ✓ Registered
   - Needs IProductService ✓ Registered
4. Gets instances from DI container
5. Calls: new ProductController(logger, productService)
6. Your controller is ready to handle the request!
```

---

## Benefits Summary

| Benefit | Without DI | With DI |
|---------|-----------|---------|
| **Easy to switch implementations** | ❌ Change everywhere | ✅ Change in Program.cs |
| **Easy to test** | ❌ Use real database | ✅ Use mock |
| **Single Responsibility** | ❌ Mixed concerns | ✅ Clear separation |
| **Code reuse** | ❌ New() in many places | ✅ Configured once |
| **Flexibility** | ❌ Hard to modify | ✅ Easy to extend |
| **Maintenance** | ❌ Multiple files to change | ✅ Single source of truth |

---

## Try It Yourself!

1. **Run with ProductService** (In-Memory)
   - Logs show no "Database:" prefix
   - Responses are instant

2. **Comment out current line, uncomment ProductServiceDatabase**
   ```csharp
   // builder.Services.AddScoped<IProductService, ProductService>();
   builder.Services.AddScoped<IProductService, ProductServiceDatabase>();
   ```
   - Run the app
   - Logs show "Database:" prefix
   - Responses have slight delay (simulated DB latency)
   - **Controller code hasn't changed!** 🎉

3. **Try ProductServiceCached**
   - See cache hits and misses in logs
   - First request slower, subsequent requests faster

4. **Try ProductServiceMock**
   - Lightning-fast, no delays
   - Perfect for testing

---

## Key Takeaway

**Dependency Injection inverts control:**

### Without DI:
Class creates its own dependencies
```
ProductController → Creates → ProductService
		 ↓
	  Tightly coupled
```

### With DI:
Dependencies are provided from outside
```
Program.cs (DI Container) → Provides → ProductController
									  (receives ProductService)
					↓
			  Loosely coupled, flexible
```

This is called the **Inversion of Control Principle** (IoC) - you're inverting WHO controls creating the dependencies.

---

## Advanced: Decorator Pattern with ProductServiceCached

ProductServiceCached wraps another service:

```csharp
public class ProductServiceCached : IProductService
{
	private readonly IProductService _innerService;  // Wraps another service!

	public ProductServiceCached(IProductService innerService)
	{
		_innerService = innerService;  // Could be ProductService or ProductServiceDatabase!
	}
}
```

This enables:
```csharp
// Wrap in-memory with caching
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductService>(
	provider => new ProductServiceCached(
		provider.GetRequiredService<ProductService>()
	)
);

// Now: Request → ProductServiceCached (cache layer) → ProductService (in-memory)
```

Or:
```csharp
// Wrap database with caching
builder.Services.AddScoped<ProductServiceDatabase>();
builder.Services.AddScoped<IProductService>(
	provider => new ProductServiceCached(
		provider.GetRequiredService<ProductServiceDatabase>()
	)
);

// Now: Request → ProductServiceCached (cache layer) → ProductServiceDatabase (DB layer)
```

Same ProductServiceCached code, different behavior based on what's injected!

---

## Next Steps

1. **Run the application** and test the endpoints
2. **Check the Output/Debug logs** to see which implementation is active
3. **Switch implementations** in Program.cs using the commented options
4. **Observe the differences** - same API, different behavior
5. **Try unit testing** by injecting ProductServiceMock

This is the power of Dependency Injection! 🚀
