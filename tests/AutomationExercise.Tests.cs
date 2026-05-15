using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AutomationExercise.Tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AutomationExerciseTests : PageTest
    {
        private readonly string baseUrl = "https://automationexercise.com/";
        // Using a predefined user for login tests. Ensure this user exists or the tests will fail.
        private readonly string validEmail = "test_playwright_user@example.com";
        private readonly string validPassword = "Password123!";

        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync(baseUrl);
            // Verify that home page is visible successfully
            await Expect(Page.Locator("body")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC01_RegisterUser()
        {
            string uniqueEmail = $"newuser_{Guid.NewGuid()}@example.com";

            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('New User Signup!')")).ToBeVisibleAsync();

            await Page.GetByPlaceholder("Name").FillAsync("Playwright User");
            await Page.Locator("form").Filter(new() { HasText = "Signup" }).GetByPlaceholder("Email Address").FillAsync(uniqueEmail);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Signup" }).ClickAsync();

            await Expect(Page.Locator("h2:has-text('Enter Account Information')")).ToBeVisibleAsync();

            await Page.GetByLabel("Mr.").CheckAsync();
            await Page.GetByLabel("Password *").FillAsync("SecurePass123!");
            await Page.Locator("#days").SelectOptionAsync(new[] { "10" });
            await Page.Locator("#months").SelectOptionAsync(new[] { "5" });
            await Page.Locator("#years").SelectOptionAsync(new[] { "1990" });

            await Page.GetByLabel("First name *").FillAsync("John");
            await Page.GetByLabel("Last name *").FillAsync("Doe");
            await Page.GetByLabel("Address * (Street address, P.O. Box, Company name, etc.)").FillAsync("123 Test Street");
            await Page.Locator("#country").SelectOptionAsync(new[] { "United States" });
            await Page.GetByLabel("State *").FillAsync("California");
            await Page.GetByLabel("City *").FillAsync("Los Angeles");
            await Page.Locator("#zipcode").FillAsync("90001");
            await Page.GetByLabel("Mobile Number *").FillAsync("1234567890");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
            await Expect(Page.Locator("h2[data-qa='account-created']")).ToHaveTextAsync("Account Created!");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Continue" }).ClickAsync();
            await Expect(Page.Locator("text=Logged in as Playwright User")).ToBeVisibleAsync();

            // Teardown: Delete Account to keep environment clean
            await Page.GetByRole(AriaRole.Link, new() { Name = "Delete Account" }).ClickAsync();
            await Expect(Page.Locator("h2[data-qa='account-deleted']")).ToHaveTextAsync("Account Deleted!");
        }

        [Test]
        public async Task TC02_LoginUserWithCorrectEmailAndPassword()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('Login to your account')")).ToBeVisibleAsync();

            await Page.Locator("form").Filter(new() { HasText = "Login" }).GetByPlaceholder("Email Address").FillAsync(validEmail);
            await Page.GetByPlaceholder("Password").FillAsync(validPassword);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            await Expect(Page.Locator("text=Logged in as")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC03_LoginUserWithIncorrectEmailAndPassword()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('Login to your account')")).ToBeVisibleAsync();

            await Page.Locator("form").Filter(new() { HasText = "Login" }).GetByPlaceholder("Email Address").FillAsync("wrong_email@example.com");
            await Page.GetByPlaceholder("Password").FillAsync("WrongPassword!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            await Expect(Page.Locator("text=Your email or password is incorrect!")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC04_LogoutUser()
        {
            // Login first
            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await Page.Locator("form").Filter(new() { HasText = "Login" }).GetByPlaceholder("Email Address").FillAsync(validEmail);
            await Page.GetByPlaceholder("Password").FillAsync(validPassword);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await Expect(Page.Locator("text=Logged in as")).ToBeVisibleAsync();

            // Logout
            await Page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();
            
            // Verify routed back to login page
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*login"));
            await Expect(Page.Locator("h2:has-text('Login to your account')")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC05_RegisterUserWithExistingEmail()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('New User Signup!')")).ToBeVisibleAsync();

            await Page.GetByPlaceholder("Name").FillAsync("Existing User");
            await Page.Locator("form").Filter(new() { HasText = "Signup" }).GetByPlaceholder("Email Address").FillAsync(validEmail);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Signup" }).ClickAsync();

            await Expect(Page.Locator("text=Email Address already exist!")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC06_ContactUsForm()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Contact us" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('Get In Touch')")).ToBeVisibleAsync();

            await Page.GetByPlaceholder("Name").FillAsync("Test User");
            await Page.GetByPlaceholder("Email", new() { Exact = true }).FillAsync("test@example.com");
            await Page.GetByPlaceholder("Subject").FillAsync("Test Subject");
            await Page.GetByPlaceholder("Your Message Here").FillAsync("This is a test message for automation.");

            // Handle the JS alert that appears when submitting
            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            
            await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

            await Expect(Page.Locator(".status.alert.alert-success")).ToHaveTextAsync("Success! Your details have been submitted successfully.");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Home" }).ClickAsync();
        }

        [Test]
        public async Task TC07_VerifyTestCasesPage()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Test Cases", Exact = true }).ClickAsync();
            
            // Verify navigation to test cases page
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*test_cases"));
            await Expect(Page.Locator("h2:has-text('Test Cases')").First).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC08_VerifyAllProductsAndProductDetailPage()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Products" }).ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*products"));
            await Expect(Page.Locator("h2:has-text('All Products')")).ToBeVisibleAsync();

            // Click view product of first product
            await Page.Locator(".choose > .nav > li > a").First.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*product_details"));

            // Verify details are visible
            await Expect(Page.Locator(".product-information h2")).ToBeVisibleAsync();
            await Expect(Page.Locator(".product-information p:has-text('Category:')")).ToBeVisibleAsync();
            await Expect(Page.Locator(".product-information span:has-text('Rs.')")).ToBeVisibleAsync();
            await Expect(Page.Locator(".product-information p:has-text('Availability:')")).ToBeVisibleAsync();
            await Expect(Page.Locator(".product-information p:has-text('Condition:')")).ToBeVisibleAsync();
            await Expect(Page.Locator(".product-information p:has-text('Brand:')")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TC09_SearchProduct()
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "Products" }).ClickAsync();
            await Expect(Page.Locator("h2:has-text('All Products')")).ToBeVisibleAsync();

            await Page.Locator("#search_product").FillAsync("T-Shirt");
            await Page.Locator("#submit_search").ClickAsync();

            await Expect(Page.Locator("h2:has-text('Searched Products')")).ToBeVisibleAsync();
            
            // Verify all the products related to search are visible
            var productsCount = await Page.Locator(".productinfo.text-center").CountAsync();
            Assert.That(productsCount, Is.GreaterThan(0), "No products found in search results.");
        }

        [Test]
        public async Task TC10_VerifySubscriptionInHomePage()
        {
            // Scroll to bottom
            await Page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

            await Expect(Page.Locator("h2:has-text('Subscription')")).ToBeVisibleAsync();

            await Page.Locator("#susbscribe_email").FillAsync("subscribe_test@example.com");
            await Page.Locator("#subscribe").ClickAsync();

            await Expect(Page.Locator("#success-subscribe")).ToHaveTextAsync("You have been successfully subscribed!");
        }
    }
}