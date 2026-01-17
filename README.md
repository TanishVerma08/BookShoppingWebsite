# 🛒 EcommProject – ASP.NET Core MVC E-Commerce Application

A full-featured **E-Commerce web application** built using **ASP.NET Core MVC**, Entity Framework Core, and SQL Server.  
The project includes admin and customer areas, authentication, payment integration, and modular architecture following best practices.

---

## 🚀 Features

### 👤 User & Authentication
- ASP.NET Core Identity
- Role-based access (Admin / Customer)
- Login, Register, Email confirmation
- Password reset & 2FA support

### 🛍️ E-Commerce
- Product catalog with categories & cover types
- Shopping cart functionality
- Order management & checkout flow
- Stripe payment integration

### 🛠️ Admin Panel
- Manage Products, Categories, Companies
- Manage Users & Roles
- View & manage Orders
- Order status filtering

### 📩 Integrations
- **Stripe** – Payments
- **Twilio** – SMS notifications
- **SMTP Email** – Email notifications

---

## 🧱 Project Structure

EcommProject/
│── EcommProject
│── EcommProject.Models
│── EcommProject.DataAccess
│── EcommProject.Utility
│── EcommProject.sln

## 🛠️ Tech Stack

- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQL Server / LocalDB**
- **Bootstrap 5**
- **jQuery**
- **Stripe API**
- **Twilio API**

---

## 🔐 Configuration & Security

> ⚠️ Secrets are NOT stored in GitHub

### Configuration files:
- `appsettings.json` → Structure only (safe to commit)
- `appsettings.Development.json` → Local secrets (ignored)

Example:
```json
"StripeSettings": {
  "PublishableKey": "",
   "SecretKey": ""
}

▶️ How to Run Locally
1️⃣ Clone the repository
git clone https://github.com/your-username/BookShoppingWebsite.git

2️⃣ Open in Visual Studio

Open EcommProject.sln

Restore NuGet packages

3️⃣ Configure database

Update connection string in appsettings.Development.json

4️⃣ Apply migrations
Update-Database

5️⃣ Run the application

Press F5 or Ctrl + F5


👨‍💻 Author

Tanish Verma
ASP.NET Core Developer

GitHub: https://github.com/TanishVerma08



## 📄 License
This project is for **learning and portfolio purposes**.
