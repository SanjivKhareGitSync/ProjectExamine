// ============================================
// PRACTICAL GUIDE: HOW TO TEST DIFFERENT IMPLEMENTATIONS
// ============================================
// This file shows you exactly how to see the DI advantages in action

/*
HOW TO RUN AND SEE THE MAGIC OF DEPENDENCY INJECTION:

===========================================
TEST 1: IN-MEMORY IMPLEMENTATION (CURRENT)
===========================================

STEP 1: Keep Program.cs as is:
    builder.Services.AddScoped<IProductService, ProductService>();

STEP 2: Start the application
    - Press F5 in Visual Studio
    - Wait for it to start

STEP 3: Open browser to Swagger
    - Go to: https://localhost:5001/swagger/index.html

STEP 4: Test GET /product
    - Click on "GET /product"
    - Click "Try it out"
    - Click "Execute"

EXPECTED RESULTS:
    ✓ Response time: < 100ms (instant!)
    ✓ Status: 200 OK
    ✓ Returns with data
    ✓ Logs show: "GetAllProducts called"
    ✓ NO "Database:" prefix in logs

WHY:
    ProductService stores data in program memory
    No delays, no database calls

---

===========================================
TEST 2: DATABASE IMPLEMENTATION
===========================================

STEP 1: Open Program.cs and CHANGE this line:
    FROM: builder.Services.AddScoped<IProductService, ProductService>();
    TO:   builder.Services.AddScoped<IProductService, ProductServiceDatabase>();

STEP 2: Rebuild and start
    - Rebuild solution: Ctrl+Shift+B
    - Press F5

STEP 3: Open Swagger again
    - https://localhost:5001/swagger/index.html

STEP 4: Test GET /product
    - Click "Try it out" → "Execute"

EXPECTED RESULTS:
    ✓ Response time: ~100-150ms (slower than in-memory)
    ✓ Status: 200 OK
    ✓ Different data (simulated from "database")
    ✓ Logs show: "Database: Executing SELECT * FROM Products"
    ✓ Contains "Database:" prefix
    ✓ Logs show the delay time

WHY:
    ProductServiceDatabase simulates database latency
    Real database would be even slower

KEY INSIGHT:
    - The ProductController code is IDENTICAL
    - Same endpoints respond
    - But implementation changed completely
    - All you did: Change ONE line in Program.cs!

---

===========================================
TEST 3: CACHED IMPLEMENTATION
===========================================

STEP 1: Open Program.cs and UNCOMMENT the cached implementation section
    We need to set it up as shown in Program.cs (with the advanced DI syntax)

Actually, for simplicity, let's just show the concept:

WHAT WOULD HAPPEN:
    First call to GET /product:
    ├─ Check cache: MISS
    ├─ Call inner service (ProductService)
    └─ Store in cache for 60 seconds
    └─ Response time: ~10-50ms

    Second call within 60 seconds:
    ├─ Check cache: HIT ✓
    ├─ Return cached data instantly
    └─ Response time: < 5ms

    After 60 seconds:
    ├─ Cache expires
    ├─ Next call fetches fresh data
    └─ Cycle repeats

LOGS WOULD SHOW:
    First request:  "Cache: CACHE MISS"
    Second request: "Cache: CACHE HIT"

---

===========================================
TEST 4: MOCK IMPLEMENTATION (FOR TESTING)
===========================================

STEP 1: Open Program.cs and change:
    FROM: builder.Services.AddScoped<IProductService, ProductService>();
    TO:   builder.Services.AddScoped<IProductService, ProductServiceMock>();

STEP 2: Rebuild and test
    - Ctrl+Shift+B
    - Press F5

STEP 3: Test GET /product

EXPECTED RESULTS:
    ✓ Response time: < 1ms (LIGHTNING FAST!)
    ✓ Returns fixed test data (IDs: 100, 101)
    ✓ Same response every time
    ✓ Perfect for unit testing

WHY:
    No database calls, no delays
    Returns hardcoded test data
    Ideal for CI/CD pipelines and unit tests

---

===========================================
COMPARISON: RESPONSE TIMES
===========================================

Implementation           | First Call | Avg Response | Best For
------------------------|-----------|--------------|------------------------------------------
ProductService          | 0-10ms    | 1-5ms        | Development, debugging
ProductServiceDatabase  | 100-200ms | 100-150ms    | Production (simulated)
ProductServiceCached    | 100ms     | 1-5ms        | High-traffic production
ProductServiceMock      | <1ms      | <1ms         | Unit testing, CI/CD

---

===========================================
HOW TO SEE THE LOGS
===========================================

WHERE TO LOOK:
1. Visual Studio Debug Output Window
   - Debug → Output Window (or Ctrl+Alt+O)
   - Select dropdown: "Debug"

2. Application Output
   - Open browser console (F12)
   - Check Network tab for response times

WHAT TO LOOK FOR:
- "ProductService initialized" → In-memory active
- "ProductServiceDatabase initialized" → Database active
- "ProductServiceCached initialized" → Cache active
- "ProductServiceMock initialized" → Mock active

EXAMPLE LOGS:

In-Memory:
    GetAllProducts called - Retrieving all products from database
    Information: GetAllProducts returned 3 products

Database:
    ProductServiceDatabase initialized - Using DATABASE implementation
    Database: Executing SELECT * FROM Products
    Database returned 4 products

Cached:
    ProductServiceCached initialized - Using CACHING implementation
    Cache: Checking if cache is valid
    Cache: CACHE MISS - Fetching from inner service
    Cache: Updated cache with 3 products. Expires in 60s

Mock:
    Information: ProductServiceMock initialized

---

===========================================
THE KEY LEARNING: 
===========================================

JUST ONE LINE IN Program.cs CONTROLS:
✓ Which implementation is used
✓ Response times
✓ Database vs In-Memory
✓ Caching behavior
✓ Mock vs Real

NO CONTROLLER CHANGES NEEDED!
NO API ENDPOINT CHANGES!
SAME API, DIFFERENT BEHAVIOR!

This is the POWER of Dependency Injection!

---

===========================================
PRACTICE EXERCISE
===========================================

Try this:
1. Start with ProductService (in-memory)
2. Make a note of response times in Swagger
3. Switch to ProductServiceDatabase
4. Compare response times
5. Check the logs
6. Notice how ProductController never changed
7. Realize: "I can swap entire implementations with ONE line!"
8. Appreciate: "This is why DI is so powerful!"

---

===========================================
WHY THIS MATTERS IN REAL PROJECTS
===========================================

SCENARIO 1: Local Development
✓ Use ProductServiceMock for fast testing
✓ No database setup needed
✓ Tests run in seconds

SCENARIO 2: Testing Environment
✓ Use ProductServiceDatabase with test DB
✓ Realistic behavior
✓ Catch integration bugs

SCENARIO 3: Production
✓ Use ProductServiceDatabase with real DB
✓ Add ProductServiceCached for performance
✓ Scale horizontally with confidence

SCENARIO 4: High Traffic Event
✓ Temporarily switch to ProductServiceCached
✓ Reduce database load
✓ Serve more users faster

SCENARIO 5: Performance Issue
✓ Add caching layer WITHOUT changing controller code
✓ Wrap existing service with ProductServiceCached
✓ Boss happy, no refactoring needed

ALL POSSIBLE BECAUSE OF DEPENDENCY INJECTION!

---

===========================================
COMMON QUESTIONS
===========================================

Q: What if I forget to register the service?
A: App will crash on startup with error:
   "Unable to resolve service for type 'IProductService'"
   The DI container can't find what to inject.

Q: Can I register multiple implementations?
A: Yes! You can have multiple registrations:
   builder.Services.AddScoped<IProductService, ProductService>();
   builder.Services.AddScoped<IAnotherService, AnotherImpl>();
   Each one gets injected to the right type.

Q: What's the difference between interface and implementation?
A: Interface (IProductService) = What it does (contract)
   Implementation (ProductService) = How it does it (code)
   DI = "Give me something that does what IProductService does"
   It doesn't care HOW, just that it works!

Q: Why use interfaces instead of concrete classes?
A: Flexibility! If you inject concrete class ProductService,
   you're locked to that. With interface IProductService,
   you can swap implementations anytime.

---

Happy Learning! Test each implementation and see the magic of DI! 🚀
*/
