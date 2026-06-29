# MobileShop - E-Commerce Platform

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blueviolet.svg?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-SQL_Server-blue.svg?style=for-the-badge&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Entity Framework](https://img.shields.io/badge/ORM-EF_Core_8.0-brightgreen.svg?style=for-the-badge)](https://learn.microsoft.com/ef/)
[![Payment Gateways](https://img.shields.io/badge/Payments-Stripe_%26_SSLCommerz-orange.svg?style=for-the-badge&logo=stripe)](https://stripe.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](LICENSE)

A feature-rich, production-grade e-commerce application tailored for mobile device retail. Built on a clean, scalable architecture using **ASP.NET Core 8.0 MVC**, **Entity Framework Core**, and **SQL Server**. It integrates multiple payment options, third-party authentication, automated email alerts, and an administrative management dashboard.

---
## Live Demo

[![Live Demo](https://img.shields.io/badge/🚀-Live%20Demo-success?style=for-the-badge)](https://mobileshop-2c8y.onrender.com)
## 🌟 Key Features

### 🛒 Customer Experience
* **Dynamic Product Catalog:** Filter products by brand, category, price range, and availability.
* **Granular Product Specifications:** Detailed technical specifications grouped by categories (e.g., Performance, Camera, Battery).
* **Multi-Image Viewer:** Gallery with multiple high-quality product images.
* **Advanced Search:** Responsive keyword searching for products and models.
* **Persistent Shopping Cart & Wishlist:** Fully-featured cart session management and persistent user wishlists.
* **Dual Payment Integrations:**
  * **Stripe:** For international credit card payments.
  * **SSLCommerz:** The leading payment gateway in Bangladesh, configured with success, fail, cancel, and Instant Payment Notification (IPN) handlers.
* **Third-Party Authentication:** Single-click sign-in via **Google OAuth**.
* **Order History & Tracking:** Track past orders and view details in user profiles.
* **Automated Notification System:** Transactional and account confirmation emails processed asynchronously using secure SMTP.

### 🛡️ Administrative Dashboard (`Areas/Admin`)
* **Sales Analytics & Reports:** Interactive dashboard offering summary statistics and key metrics.
* **Product Inventory Management:** Complete CRUD interface with multi-image file uploads and specifications manager.
* **Order Tracking & Lifecycle Management:** Process pending orders, transition order statuses, and update shipping details.
* **Brand & Category Management:** Direct control over catalog classification.
* **User Management:** Access list of registered customers, update roles, and manage account statuses.

---

## 🛠️ Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Backend Framework** | ASP.NET Core 8.0 (MVC pattern) |
| **Database & ORM** | SQL Server + Entity Framework Core (Code-First) |
| **Identity & Security** | ASP.NET Core Identity (Roles, Claims, Cookie Auth, Google OAuth) |
| **Payments** | Stripe SDK, SSLCommerz API (Direct HTTP Client integration) |
| **Email Services** | SMTP Integration (Gmail SMTP relay ready) |
| **Frontend Utilities** | Bootstrap 5, Custom CSS, jQuery |
| **Other Packages** | AutoMapper (Entity-to-ViewModel mapping), Microsoft.AspNetCore.Session |

---

## 📂 Project Structure

```text
MobileShopSln/
├── MobileShopSln.sln           # Visual Studio Solution file
└── MobileShop/                 # Primary Project Directory
    ├── Areas/
    │   └── Admin/              # Dashboard Controllers, Views, and ViewModels
    ├── Controllers/            # Customer-facing controllers (Account, Cart, Products, Checkout)
    ├── Data/
    │   ├── ApplicationDbContext.cs # EF Core Database Context
    │   └── DbInitializer.cs    # Seeding scripts for initial database configuration
    ├── Interfaces/             # Service Interface Contracts
    ├── Migrations/             # EF Core Database Migrations
    ├── Models/                 # Domain Entity Models (Product, Order, User, Wishlist, etc.)
    ├── Services/               # Core Domain Service Implementations (Payments, Orders, Products, Email)
    ├── ViewComponents/         # Reusable dynamic view modules
    ├── ViewModels/             # Strongly-typed models for frontend data transfer
    ├── Views/                  # Customer-facing Razor views
    ├── wwwroot/                # Static assets (CSS, JS, product images, uploads)
    ├── appsettings.json        # Main configuration file
    └── Program.cs              # Middleware setup and dependency injection container
```

---

## 🚀 Getting Started

### 📋 Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server (Express or LocalDB)](https://www.microsoft.com/sql-server/sql-server-downloads)
* [EF Core CLI Tools](https://learn.microsoft.com/ef/core/cli/dotnet) (install via `dotnet tool install --global dotnet-ef`)
* Optional: [Ngrok](https://ngrok.com/) (required to test SSLCommerz payment webhooks locally)

---

### ⚙️ Configuration Setup

Rename `appsettings.json` or configure it with your local credentials and service API keys:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=Prc_MobileShop;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=True"
  },
  "Stripe": {
    "PublishableKey": "YOUR_STRIPE_PUBLISHABLE_KEY",
    "SecretKey": "YOUR_STRIPE_SECRET_KEY"
  },
  "SSLCommerz": {
    "StoreId": "YOUR_SSLCOMMERZ_STORE_ID",
    "StorePassword": "YOUR_SSLCOMMERZ_STORE_PASSWORD",
    "IsSandbox": true,
    "SandboxUrl": "https://sandbox.sslcommerz.com/gwprocess/v4/api.php",
    "LiveUrl": "https://securepay.sslcommerz.com/gwprocess/v4/api.php",
    "ValidationSandboxUrl": "https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php",
    "ValidationLiveUrl": "https://securepay.sslcommerz.com/validator/api/validationserverAPI.php",
    "SuccessUrl": "https://<your-ngrok-subdomain>.ngrok-free.dev/Checkout/SSLSuccess",
    "FailUrl": "https://<your-ngrok-subdomain>.ngrok-free.dev/Checkout/SSLFail",
    "CancelUrl": "https://<your-ngrok-subdomain>.ngrok-free.dev/Checkout/SSLCancel",
    "IPNUrl": "https://<your-ngrok-subdomain>.ngrok-free.dev/Checkout/SSLIPN"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "YOUR_EMAIL@gmail.com",
    "SenderPassword": "YOUR_APP_PASSWORD"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

---

### 🏃 Installation Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-username/MobileShopSln.git
   cd MobileShopSln
   ```

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Database Migration:**
   Apply EF Migrations to generate the database schema automatically:
   ```bash
   cd MobileShop
   dotnet ef database update
   ```
   *(Note: On first startup, the database will also be automatically migrated and seeded through the `DbInitializer` implementation).*

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Open your browser and navigate to `https://localhost:7030` or the port shown in your terminal.

---

## 🔑 Default Seed Account

The database seeds a system administrator account on initial startup for configuration and testing.

* **Admin Panel Access:** Navigate to `/Admin`
* **Username:** `admin@mobileshop.com`
* **Password:** `Admin@123`

---

## 🛜 Testing Payment Gateways Locally

### Stripe
Stripe handles payments on its secure checkout page. Use test cards (e.g. `4242 4242 4242 4242`) to test simulated transactions.

### SSLCommerz with Ngrok
Since SSLCommerz relies on webhook/IPN endpoints to notify your application of successful transactions, you need to expose your local instance to the web:

1. Start your local ASP.NET Core server (e.g. running on port `7030` or `5001`).
2. Start an Ngrok tunnel pointing to your local HTTPS port:
   ```bash
   ngrok http https://localhost:7030 --host-header="localhost:7030"
   ```
3. Update the `SSLCommerz` redirect URLs (`SuccessUrl`, `FailUrl`, `CancelUrl`, `IPNUrl`) in `appsettings.json` with the assigned `ngrok-free.dev` domain.

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
