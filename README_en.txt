Personal Finance Manager
=======================

App Description:
----------------
This application helps you manage your personal finances, track income and expenses, budgets, goals, recurring payments, and offers AI assistance for analysis and tips. It supports data export, backup, and user management with role-based access.

Technologies Used:
------------------
- .NET 8 (C#)
- Entity Framework Core (SQLite)
- Spectre.Console (modern console UI)
- ClosedXML, QuestPDF (Excel/PDF export)
- SkiaSharp (charts)
- OpenAI API (AI features)

Security:
---------
- Passwords are stored as hashes only (PBKDF2)
- Sensitive transaction descriptions are encrypted (AES)
- User roles (admin/user) restrict access to functions
- Audit logs record important actions
- Two-factor authentication (2FA) on login

App Features:
-------------
- Transaction management (income/expense, categories, currencies)
- Budget management and overspending alerts
- Recurring payments (automatic generation)
- Goals and deadline notifications
- Export to Excel/PDF, CSV import
- Database backup
- AI tips, analysis, chat, automatic categorization
- User management, password change, roles

How to Run:
-----------
1. Download and unzip the application folder.
2. Run ConsoleApp4.exe (or FinanceManager.exe) in the publish folder.
3. For AI features, set your OpenAI API key:
   - In command prompt:
     set OPENAI_API_KEY=your_api_key
   - Or in PowerShell:
     $env:OPENAI_API_KEY="your_api_key"
4. The app is ready to use.

Additional Info:
----------------
- You can use the provided installer (wizard) for easy installation.
- Data is stored in finance.db (SQLite).
- To manage users, log in as admin (demo account: admin/admin).

