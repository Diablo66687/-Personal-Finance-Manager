using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.IO;
using System.IO.Compression; // Přidat pro práci se ZIP
using SkiaSharp; // Přidat pro export grafu do PNG

// Czech: Hlavní třída aplikace pro správu osobních financií
// English: Main application class for personal finance management
namespace ConsoleApp4
{
    internal class Program
    {
        // Czech: Jazyk aplikace (cz/en)
        // English: Application language (cz/en)
        static string lang = "cz";
        // Czech: Slovník barev kategorií
        // English: Category color dictionary
        static Dictionary<string, string> CategoryColors = new();
        // Czech: Soubor s kategoriemi
        // English: Category file
        static string CategoryFile = "categories.txt";

        // Czech: Přihlášený uživatel
        // English: Logged-in user
        static User CurrentUser = null;

        // Czech: Základní měna
        // English: Base currency
        static string BaseCurrency = "CZK";
        // Czech: Jednoduché kurzy měn
        // English: Simple currency exchange rates
        static Dictionary<string, decimal> ExchangeRates = new()
        {
            { "CZK", 1m },
            { "EUR", 25m },
            { "USD", 23m }
        };

        // Czech: Složka pro zálohy
        // English: Backup folder
        static string BackupFolder = "backups";
        // Czech: Interval zálohování v minutách
        // English: Backup interval in minutes
        static int BackupIntervalMinutes = 60;
        // Czech: Token pro zrušení zálohování
        // English: Cancellation token for backup
        static CancellationTokenSource backupCts = new();

        // Czech: Spustí automatické zálohování databáze
        // English: Starts automatic database backup
        static void StartAutoBackup()
        {
            Directory.CreateDirectory(BackupFolder);
            Task.Run(async () =>
            {
                while (!backupCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var dbFile = "finance.db";
                        var backupFile = Path.Combine(BackupFolder, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                        if (File.Exists(dbFile))
                        {
                            File.Copy(dbFile, backupFile, true);
                        }
                    }
                    catch { }
                    await Task.Delay(TimeSpan.FromMinutes(BackupIntervalMinutes), backupCts.Token);
                }
            }, backupCts.Token);
        }

        // Czech: Vstupní bod aplikace bez automatické migrace
        // English: Application entry point without automatic migration
        static void Main(string[] args)
        {
            try
            {
                FinanceContext.SeedDemoData(); // Vloží demo data při startu
                StartAutoBackup();
                using var db = new FinanceContext();
                GenerateRecurringTransactions(db); // Automatické generování opakovaných transakcí
                // db.Database.Migrate(); // Migrace pouze na vyžádání
                CheckBudgetWarnings(db);
                CheckGoalNotifications(db); // Upozornění na cíle
                LoadCategoryColors();

                while (true)
                {
                    // Moderní úvodní panel bez ASCII artu, s brandingem a webem
                    AnsiConsole.Clear();
                    // Oprava: odstranění všech emoji a vnořených tagů ze Spectre.Console Markup
                    // Panel bez emoji a bez vnořených tagů
                    var header = new FigletText(lang == "cz" ? "Osobní finanční manažer" : "Personal Finance Manager")
                        .Color(Spectre.Console.Color.Aqua);
                    var subtitle = new Markup(lang == "cz" ? "Vytvořil DevBrain" : "Created by DevBrain");
                    var website = new Markup("Web: devbrain.cz");
                    var panel = new Panel(new Rows(header, subtitle, website))
                        .Header(lang == "cz" ? "Osobní finance" : "Personal Finance")
                        .BorderColor(Spectre.Console.Color.Aqua)
                        .Padding(1,1)
                        .Expand();
                    AnsiConsole.Write(panel);
                    AnsiConsole.MarkupLine(lang == "cz"
                        ? "[bold yellow]Zadejte příkaz:[/]" : "[bold yellow]Enter command:[/]");
                    AnsiConsole.MarkupLine(
    "[green]add[/], [green]list[/], [green]report[/], [green]year-report[/], [green]category-report[/], [green]pie-report[/], [green]balance-trend[/], [green]export-pie-pdf[/], [green]export-balance-pdf[/], [green]budget[/], [green]export-excel[/], [green]export-pdf[/], [green]export-budgets[/], [green]export-budgets-pdf[/], [green]import-csv[/], [green]import-budgets-csv[/], [green]delete[/], [green]edit[/], [green]recurring[/], [green]lang[/], [green]help[/], [green]exit[/]"
);
                    var command = Console.ReadLine()?.Trim().ToLower();
                    switch (command)
                    {
                        case "add":
                            AddTransaction(db);
                            break;
                        case "list":
                            var filters = GetTransactionFilters();
                            ListTransactions(db, filters);
                            break;
                        case "budget":
                            ListBudgets(db);
                            break;
                        case "report":
                            ShowMonthlyReport(db);
                            break;
                        case "export-excel":
                            var excelFilters = GetTransactionFilters();
                            ExportTransactionsToExcel(db, excelFilters);
                            break;
                        case "export-pdf":
                            var pdfFilters = GetTransactionFilters();
                            ExportTransactionsToPdf(db, pdfFilters);
                            break;
                        case "export-budgets":
                            ExportBudgetsToExcel(db);
                            break;
                        case "export-budgets-pdf":
                            ExportBudgetsToPdf(db);
                            break;
                        case "delete":
                            DeleteTransaction(db);
                            break;
                        case "edit":
                            EditTransaction(db);
                            break;
                        case "help":
                            ShowHelp();
                            break;
                        case "exit":
                            return;
                        case "year-report":
                            ShowYearReport(db);
                            break;
                        case "import-csv":
                            ImportTransactionsFromCsv(db);
                            break;
                        case "lang":
                            lang = lang == "cz" ? "en" : "cz";
                            AnsiConsole.MarkupLine(lang == "cz" ? "Jazyk přepnut na češtinu." : "Language switched to English.");
                            break;
                        case "import-budgets-csv":
                            ImportBudgetsFromCsv(db);
                            break;
                        case "category-report":
                            ShowCategoryReport(db);
                            break;
                        case "pie-report":
                            ShowPieReport(db);
                            break;
                        case "balance-trend":
                            ShowBalanceTrend(db);
                            break;
                        case "export-pie-pdf":
                            ExportPieReportToPdf(db);
                            break;
                        case "export-balance-pdf":
                            ExportBalanceTrendToPdf(db);
                            break;
                        case "recurring":
                            AddRecurringTransaction(db);
                            break;
                        case "edit-budget":
                            EditBudget(db);
                            break;
                        case "delete-budget":
                            DeleteBudget(db);
                            break;
                        case "menu":
                            ShowInteractiveMenu(db);
                            break;
                        case "import-csv-batch":
                            ImportTransactionsBatch(db);
                            break;
                        case "export-csv-batch":
                            ExportTransactionsBatch(db);
                            break;
                        case "manage-categories":
                            ManageCategories();
                            break;
                        case "import-csv-zip":
                            ImportTransactionsFromZip(db);
                            break;
                        case "export-csv-zip":
                            ExportTransactionsToZip(db);
                            break;
                        case "backup-db":
                            BackupDatabase();
                            break;
                        case "restore-db":
                            RestoreDatabase();
                            break;
                        case "login":
                            Login(db);
                            break;
                        case "register":
                            Register(db);
                            break;
                        case "set-goal":
                            SetGoal(db);
                            break;
                        case "show-goals":
                            ShowGoals(db);
                            break;
                        case "dashboard":
                            ShowDashboard(db);
                            break;
                        case "search":
                            SearchTransactions(db);
                            break;
                        case "expense-analysis":
                            ShowAdvancedExpenseAnalysis(db);
                            break;
                        case "ai-budget-tip":
                            ShowAIBudgetTip(db).GetAwaiter().GetResult();
                            break;
                        case "ai-analyze":
                            ShowAIAnalyze(db).GetAwaiter().GetResult();
                            break;
                        case "ai-chat":
                            ShowAIChat().GetAwaiter().GetResult();
                            break;
                        case "ai-categorize":
                            ShowAICategorize().GetAwaiter().GetResult();
                            break;
                        default:
                            AnsiConsole.MarkupLine("[red]Neplatný příkaz.[/]");
                            break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Chyba aplikace: {ex.Message}[/]");
                AnsiConsole.MarkupLine("[red]Zkontrolujte konfiguraci databáze nebo migrace.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Neočekávaná chyba: {ex.Message}[/]");
            }
        }

        // Czech: Přidá novou transakci do databáze
        // English: Adds a new transaction to the database
        static void AddTransaction(FinanceContext db)
        {
            try
            {
                // Czech: Získání a validace vstupních dat od uživatele
                // English: Get and validate user input data
                Console.Write(lang == "cz" ? "Datum (YYYY-MM-DD): " : "Date (YYYY-MM-DD): ");
                var dateStr = Console.ReadLine();
                if (!DateTime.TryParse(dateStr, out var date))
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatné datum![/]" : "[red]Invalid date![/]");
                    return;
                }
                Console.Write(lang == "cz" ? "Částka (+příjem, -výdaj): " : "Amount (+income, -expense): ");
                var amountStr = Console.ReadLine();
                if (!decimal.TryParse(amountStr, out var amount))
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatná částka![/]" : "[red]Invalid amount![/]");
                    return;
                }
                Console.Write(lang == "cz" ? "Měna (CZK/EUR/USD): " : "Currency (CZK/EUR/USD): ");
                var currency = Console.ReadLine()?.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(currency) || !ExchangeRates.ContainsKey(currency))
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatná měna![/]" : "[red]Invalid currency![/]");
                    return;
                }
                var rate = ExchangeRates[currency];
                Console.Write(lang == "cz" ? "Popis: " : "Description: ");
                var desc = Console.ReadLine() ?? "";
                if (desc.Length > 128)
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Popis je příliš dlouhý (max. 128 znaků)![/]" : "[red]Description too long (max 128 chars)![/]");
                    return;
                }
                var catList = CategoryColors.Keys.ToList();
                string cat = "";
                if (catList.Count > 0)
                {
                    cat = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title(lang == "cz" ? "Vyberte kategorii:" : "Select category:")
                        .AddChoices(catList));
                }
                else
                {
                    Console.Write(lang == "cz" ? "Kategorie: " : "Category: ");
                    cat = Console.ReadLine() ?? "";
                }
                if (string.IsNullOrWhiteSpace(cat) || cat.Length > 32)
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Kategorie je povinná a max. 32 znaků![/]" : "[red]Category required, max 32 chars![/]");
                    return;
                }
                // Czech: Šifrování citlivých dat
                // English: Encrypt sensitive data
                var encryptedDesc = AesEncryptionHelper.Encrypt(desc);
                db.Transactions.Add(new Transaction { Date = date, Amount = amount, Description = encryptedDesc, Category = cat, Currency = currency, ExchangeRate = rate });
                db.SaveChanges();
                LogAudit(db, "AddTransaction", $"UserId: {CurrentUser?.Id}, Amount: {amount}, Category: {cat}");
                AnsiConsole.MarkupLine(lang == "cz" ? "[green]Transakce přidána.[/]" : "[green]Transaction added.[/]");
                Console.Write(lang == "cz" ? "Nastavit opakovanou platbu? (ano/ne): " : "Set recurring payment? (yes/no): ");
                var recurringAnswer = Console.ReadLine()?.Trim().ToLower();
                if (recurringAnswer == "ano" || recurringAnswer == "yes")
                {
                    AddRecurringTransaction(db, date, amount, desc, cat);
                }
            }
            catch (Exception ex)
            {
                LogAudit(db, "AddTransactionFailed", ex.Message);
                AnsiConsole.MarkupLine(lang == "cz" ? $"[red]Chybný vstup: {ex.Message}[/]" : $"[red]Invalid input: {ex.Message}[/]");
            }
        }

        static void AddRecurringTransaction(FinanceContext db, DateTime? date = null, decimal? amount = null, string desc = null, string cat = null)
        {
            try
            {
                date ??= DateTime.Today;
                Console.Write(lang == "cz" ? "Interval (denně, týdenně, měsíčně): " : "Interval (daily, weekly, monthly): ");
                var interval = Console.ReadLine()?.Trim().ToLower();
                
                db.RecurringTransactions.Add(new RecurringTransaction {
                    StartDate = date.Value,
                    Amount = amount.Value,
                    Description = desc,
                    Category = cat,
                    Frequency = interval
                });
                db.SaveChanges();
                AnsiConsole.MarkupLine(lang == "cz" ? "[green]Opakovaná platba nastavena.[/]" : "[green]Recurring payment set.[/]");
            }
            catch
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Chybný vstup pro opakovanou platbu![/]" : "[red]Invalid input for recurring payment![/]");
            }
        }

        static void AddRecurringTransaction(FinanceContext db)
        {
            Console.Write(lang == "cz" ? "Datum zahájení (YYYY-MM-DD): " : "Start date (YYYY-MM-DD): ");
            var startStr = Console.ReadLine();
            if (!DateTime.TryParse(startStr, out var start))
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatné datum![/]" : "[red]Invalid date![/]");
                return;
            }
            Console.Write(lang == "cz" ? "Částka (+příjem, -výdaj): " : "Amount (+income, -expense): ");
            var amountStr = Console.ReadLine();
            if (!decimal.TryParse(amountStr, out var amount))
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatná částka![/]" : "[red]Invalid amount![/]");
                return;
            }
            Console.Write(lang == "cz" ? "Popis: " : "Description: ");
            var desc = Console.ReadLine() ?? "";
            Console.Write(lang == "cz" ? "Kategorie: " : "Category: ");
            var cat = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(cat))
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Kategorie je povinná![/]" : "[red]Category is required![/]");
                return;
            }
            Console.Write(lang == "cz" ? "Frekvence (monthly/weekly): " : "Frequency (monthly/weekly): ");
            var freq = Console.ReadLine()?.Trim().ToLower();
            int? dayOfMonth = null;
            DayOfWeek? dayOfWeek = null;
            if (freq == "monthly")
            {
                Console.Write(lang == "cz" ? "Den v měsíci (1-28): " : "Day of month (1-28): ");
                if (int.TryParse(Console.ReadLine(), out var dom) && dom >= 1 && dom <= 28) dayOfMonth = dom;
            }
            else if (freq == "weekly")
            {
                Console.Write(lang == "cz" ? "Den v týdnu (0=neděle...6=sobota): " : "Day of week (0=Sunday...6=Saturday): ");
                if (int.TryParse(Console.ReadLine(), out var dow) && dow >= 0 && dow <= 6) dayOfWeek = (DayOfWeek)dow;
            }
            db.RecurringTransactions.Add(new RecurringTransaction {
                StartDate = start,
                Amount = amount,
                Description = desc,
                Category = cat,
                Frequency = freq,
                DayOfMonth = dayOfMonth,
                DayOfWeek = dayOfWeek
            });
            db.SaveChanges();
            AnsiConsole.MarkupLine(lang == "cz" ? "[green]Opakovaná platba nastavena.[/]" : "[green]Recurring payment set.[/]");
        }

        static void CheckBudgetWarnings(FinanceContext db)
        {
            var now = DateTime.Now;
            var budgets = db.Budgets.Where(b => b.Year == now.Year && b.Month == now.Month).ToList();
            foreach (var budget in budgets)
            {
                var expense = db.Transactions.Where(t => t.Date.Year == budget.Year && t.Date.Month == budget.Month && t.Category == budget.Category && t.Amount < 0).Sum(t => t.Amount);
                if (-expense >= 0.9m * budget.Limit && -expense < budget.Limit)
                {
                    AnsiConsole.MarkupLine($"[yellow]Upozornění: Výdaje v kategorii {budget.Category} dosáhly 90 % limitu![/]");
                }
                else if (-expense >= budget.Limit)
                {
                    AnsiConsole.MarkupLine($"[red]Upozornění: Překrocen rozpočet v kategorii {budget.Category}![/]");
                }
            }
        }

        static (string category, DateTime? from, DateTime? to, decimal? min, decimal? max) GetTransactionFilters()
        {
            Console.Write("Kategorie (Enter pro vše): ");
            var cat = Console.ReadLine();
            Console.Write("Od data (YYYY-MM-DD, Enter pro vše): ");
            var from = DateTime.TryParse(Console.ReadLine(), out var fromDate) ? fromDate : (DateTime?)null;
            Console.Write("Do data (YYYY-MM-DD, Enter pro vše): ");
            var to = DateTime.TryParse(Console.ReadLine(), out var toDate) ? toDate : (DateTime?)null;
            Console.Write("Min. částka (Enter pro vše): ");
            var min = decimal.TryParse(Console.ReadLine(), out var minVal) ? minVal : (decimal?)null;
            Console.Write("Max. částka (Enter pro vše): ");
            var max = decimal.TryParse(Console.ReadLine(), out var maxVal) ? maxVal : (decimal?)null;
            return (cat, from, to, min, max);
        }

        static List<Transaction> FilterTransactions(FinanceContext db, (string category, DateTime? from, DateTime? to, decimal? min, decimal? max) filters)
        {
            var query = db.Transactions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filters.category))
                query = query.Where(t => t.Category.ToLower() == filters.category.ToLower());
            if (filters.from.HasValue)
                query = query.Where(t => t.Date >= filters.from.Value);
            if (filters.to.HasValue)
                query = query.Where(t => t.Date <= filters.to.Value);
            if (filters.min.HasValue)
                query = query.Where(t => t.Amount >= filters.min.Value);
            if (filters.max.HasValue)
                query = query.Where(t => t.Amount <= filters.max.Value);
            var list = query.OrderByDescending(t => t.Date).ToList();
            // Dešifrování citlivých dat
            foreach (var t in list)
            {
                t.Description = AesEncryptionHelper.Decrypt(t.Description);
                // Amount zůstává jako číslo, nešifruje se
            }
            return list;
        }

        static void ListTransactions(FinanceContext db, (string category, DateTime? from, DateTime? to, decimal? min, decimal? max) filters)
        {
            var transactions = FilterTransactions(db, filters);
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold aqua]:credit_card: Transakce[/]")
                .Caption("[grey]Seznam všech transakcí podle filtru[/]")
                .AddColumn(new TableColumn("ID").Centered())
                .AddColumn(new TableColumn("Datum").Centered())
                .AddColumn(new TableColumn("Částka").Centered())
                .AddColumn(new TableColumn("Měna").Centered())
                .AddColumn(new TableColumn("CZK").Centered())
                .AddColumn(new TableColumn("Kategorie").Centered())
                .AddColumn(new TableColumn("Popis").Centered());
            foreach (var t in transactions)
            {
                var color = CategoryColors.ContainsKey(t.Category) ? CategoryColors[t.Category] : "white";
                var czkAmount = t.Currency == BaseCurrency ? t.Amount : t.Amount * (t.ExchangeRate ?? 1m);
                table.AddRow(
                    $"[bold]{t.Id}[/]",
                    $"[blue]{t.Date:yyyy-MM-dd}[/]",
                    t.Amount >= 0 ? $"[green]:arrow_up:{t.Amount:N2}[/]" : $"[red]:arrow_down:{t.Amount:N2}[/]",
                    $"[yellow]{t.Currency ?? BaseCurrency}[/]",
                    $"[bold yellow]{czkAmount:N2}[/]",
                    $"[{color}]:label:{t.Category}[/]",
                    $"[grey]{t.Description ?? ""}[/]"
                );
            }
            var panel = new Panel(table)
                .Header("[bold aqua]Přehled transakcí[/]")
                .BorderColor(Spectre.Console.Color.Aqua)
                .Padding(1,1)
                .Expand();
            AnsiConsole.Write(panel);
        }

        static void ListBudgets(FinanceContext db)
        {
            var budgets = db.Budgets.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).ToList();
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold green]:moneybag: Rozpočty[/]")
                .Caption("[grey]Seznam všech rozpočtů[/]")
                .AddColumn(new TableColumn("Rok").Centered())
                .AddColumn(new TableColumn("Měsíc").Centered())
                .AddColumn(new TableColumn("Kategorie").Centered())
                .AddColumn(new TableColumn("Limit").Centered());
            foreach (var b in budgets)
            {
                table.AddRow(
                    $"[bold]{b.Year}[/]",
                    $"[blue]{b.Month}[/]",
                    $"[aqua]:label:{b.Category ?? ""}[/]",
                    $"[bold green]{b.Limit:C}[/]"
                );
            }
            var panel = new Panel(table)
                .Header("[bold green]Přehled rozpočtů[/]")
                .BorderColor(Spectre.Console.Color.Green)
                .Padding(1,1)
                .Expand();
            AnsiConsole.Write(panel);
        }

        static void SetBudget(FinanceContext db)
        {
            try
            {
                Console.Write(lang == "cz" ? "Rok: " : "Year: ");
                var yearStr = Console.ReadLine();
                if (!int.TryParse(yearStr, out var year) || year < 1900 || year > 2100)
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatný rok![/]" : "[red]Invalid year![/]");
                    return;
                }
                Console.Write(lang == "cz" ? "Měsíc: " : "Month: ");
                var monthStr = Console.ReadLine();
                if (!int.TryParse(monthStr, out var month) || month < 1 || month > 12)
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatný měsíc![/]" : "[red]Invalid month![/]");
                    return;
                }
                Console.Write(lang == "cz" ? "Kategorie: " : "Category: ");
                var category = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(category))
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Kategorie je povinná![/]" : "[red]Category is required![/]");
                    return;
                }
                Console.Write(lang == "cz" ? "Limit: " : "Limit: ");
                var limitStr = Console.ReadLine();
                if (!decimal.TryParse(limitStr, out var limit) || limit < 0)
                {
                    AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatný limit![/]" : "[red]Invalid limit![/]");
                    return;
                }
                var budget = db.Budgets.FirstOrDefault(b => b.Year == year && b.Month == month && b.Category == category);
                if (budget == null)
                {
                    db.Budgets.Add(new Budget { Year = year, Month = month, Category = category, Limit = limit });
                }
                else
                {
                    budget.Limit = limit;
                    db.Budgets.Update(budget);
                }
                db.SaveChanges();
                AnsiConsole.MarkupLine(lang == "cz" ? "[green]Rozpočet nastaven.[/]" : "[green]Budget set.[/]");
            }
            catch
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Chybný vstup![/]" : "[red]Invalid input![/]");
            }
        }

        static void ShowMonthlyReport(FinanceContext db)
        {
            try
            {
                Console.Write("Rok: ");
                var year = int.Parse(Console.ReadLine() ?? "");
                Console.Write("Měsíc: ");
                var month = int.Parse(Console.ReadLine() ?? "");
                var transactions = db.Transactions.Where(t => t.Date.Year == year && t.Date.Month == month).ToList();
                var budgets = db.Budgets.Where(b => b.Year == year && b.Month == month).ToList();
                var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                var totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                var balance = totalIncome + totalExpense;

                AnsiConsole.MarkupLine($"[bold underline]Report za {month}/{year}[/]");
                AnsiConsole.MarkupLine($"Příjmy: [green]{totalIncome:C}[/]");
                AnsiConsole.MarkupLine($"Výdaje: [red]{-totalExpense:C}[/]");
                AnsiConsole.MarkupLine($"Zůstatek: [yellow]{balance:C}[/]");

                foreach (var budget in budgets)
                {
                    var catExpense = transactions.Where(t => t.Category == budget.Category && t.Amount < 0).Sum(t => t.Amount);
                    AnsiConsole.MarkupLine($"Rozpočet pro kategorii [blue]{budget.Category}[/]: [blue]{budget.Limit:C}[/]");
                    if (-catExpense > budget.Limit)
                        AnsiConsole.MarkupLine($"[bold red]Upozornění: Překrocen rozpočet pro kategorii {budget.Category}![/]");
                }

                if (transactions.Count > 0)
                {
                    var categories = transactions.Where(t => t.Amount < 0)
                        .GroupBy(t => t.Category)
                        .Select(g => new { Category = g.Key, Total = -g.Sum(t => t.Amount) })
                        .OrderByDescending(x => x.Total)
                        .ToList();
                    var chart = new BarChart()
                        .Width(60)
                        .Label("Výdaje podle kategorií")
                        .CenterLabel();
                    foreach (var cat in categories)
                    {
                        var color = CategoryColors.ContainsKey(cat.Category) ? (Spectre.Console.Color)typeof(Spectre.Console.Color).GetProperty(CategoryColors[cat.Category], System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null) : Spectre.Console.Color.Red;
                        chart.AddItem(cat.Category ?? "Neznámá", (float)cat.Total, color);
                    }
                    AnsiConsole.Write(chart);
                }
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Chybný vstup![/]");
            }
        }

        static void ShowYearReport(FinanceContext db)
        {
            Console.Write(lang == "cz" ? "Zadejte rok: " : "Enter year: ");
            if (!int.TryParse(Console.ReadLine(), out var year))
            {
                AnsiConsole.MarkupLine(lang == "cz" ? "[red]Neplatný rok![/]" : "[red]Invalid year![/]");
                return;
            }
            var months = Enumerable.Range(1, 12).ToList();
            var incomes = new decimal[12];
            var expenses = new decimal[12];
            foreach (var m in months)
            {
                incomes[m-1] = db.Transactions.Where(t => t.Date.Year == year && t.Date.Month == m && t.Amount > 0).Sum(t => t.Amount);
                expenses[m-1] = -db.Transactions.Where(t => t.Date.Year == year && t.Date.Month == m && t.Amount < 0).Sum(t => t.Amount);
            }
            var table = new Table().Title(lang == "cz" ? $"Roční report {year}" : $"Yearly report {year}")
                .AddColumn(lang == "cz" ? "Měsíc" : "Month")
                .AddColumn(lang == "cz" ? "Příjmy" : "Income")
                .AddColumn(lang == "cz" ? "Výdaje" : "Expense");
            for (int i = 0; i < 12; i++)
            {
                table.AddRow(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i+1),
                    $"[green]{incomes[i]:C}[/]", $"[red]{expenses[i]:C}[/]");
            }
            AnsiConsole.Write(table);
            var chart = new BarChart().Width(60).Label(lang == "cz" ? "Trend příjmů" : "Income trend").CenterLabel();
            for (int i = 0; i < 12; i++)
                chart.AddItem(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i+1), (float)incomes[i], Spectre.Console.Color.Green);
            AnsiConsole.Write(chart);
            chart = new BarChart().Width(60).Label(lang == "cz" ? "Trend výdajů" : "Expense trend").CenterLabel();
            for (int i = 0; i < 12; i++)
                chart.AddItem(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i+1), (float)expenses[i], Spectre.Console.Color.Red);
            AnsiConsole.Write(chart);
        }

        static void ExportTransactionsToExcel(FinanceContext db, (string category, DateTime? from, DateTime? to, decimal? min, decimal? max) filters)
        {
            var transactions = FilterTransactions(db, filters);
            Console.Write("Zadejte název souboru (bez přípony): ");

            var fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"transakce_{DateTime.Now:yyyyMMdd_HHmmss}";
            var filePath = $"{fileName}.xlsx";
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Transakce");

                // Přidání hlavičky
                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Datum";
                ws.Cell(1, 3).Value = "Částka";
                ws.Cell(1, 4).Value = "Měna";
                ws.Cell(1, 5).Value = "CZK";
                ws.Cell(1, 6).Value = "Kategorie";
                ws.Cell(1, 7).Value = "Popis";

                // Styly
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.Aqua;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                foreach (var cell in ws.Row(1).Cells())
                {
                    cell.Style = headerStyle;
                }

                // Přidání dat
                var row = 2;
                foreach (var t in transactions)
                {
                    ws.Cell(row, 1).Value = t.Id;
                    ws.Cell(row, 2).Value = t.Date;
                    ws.Cell(row, 3).Value = t.Amount;
                    ws.Cell(row, 4).Value = t.Currency;
                    ws.Cell(row, 5).Value = t.Amount * (t.ExchangeRate ?? 1);
                    ws.Cell(row, 6).Value = t.Category;
                    ws.Cell(row, 7).Value = t.Description;
                    row++;
                }

                // Autofit sloupce
                ws.Columns().AdjustToContents();

                // Uložení souboru
                workbook.SaveAs(filePath);
            }
            AnsiConsole.MarkupLine($"[green]Úspěšně exportováno do {filePath}[/]");
        }

        static void ExportTransactionsToPdf(FinanceContext db, (string category, DateTime? from, DateTime? to, decimal? min, decimal? max) filters)
        {
            var transactions = FilterTransactions(db, filters);
            Console.Write("Zadejte název souboru (bez přípony): ");
            var fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"transakce_{DateTime.Now:yyyyMMdd_HHmmss}";
            var filePath = $"{fileName}.pdf";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Výpis transakcí").Bold().FontSize(20);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Hlavička tabulky
                        table.Cell().Text("ID");
                        table.Cell().Text("Datum");
                        table.Cell().Text("Částka");
                        table.Cell().Text("Kategorie");
                        table.Cell().Text("Popis");

                        // Data
                        foreach (var t in transactions)
                        {
                            table.Cell().Text(t.Id.ToString());
                            table.Cell().Text(t.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(t.Amount.ToString("N2"));
                            table.Cell().Text(t.Category);
                            table.Cell().Text(t.Description);
                        }
                    });
                });
            });
            document.GeneratePdf(filePath);
            AnsiConsole.MarkupLine($"[green]Úspěšně exportováno do {filePath}[/]");
        }

        static void ExportBudgetsToExcel(FinanceContext db)
        {
            var budgets = db.Budgets.ToList();
            Console.Write("Zadejte název souboru (bez přípony): ");
            var fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"rozpocty_{DateTime.Now:yyyyMMdd_HHmmss}";
            var filePath = $"{fileName}.xlsx";
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Rozpočty");

                // Přidání hlavičky
                ws.Cell(1, 1).Value = "Rok";
                ws.Cell(1, 2).Value = "Měsíc";
                ws.Cell(1, 3).Value = "Kategorie";
                ws.Cell(1, 4).Value = "Limit";

                // Styly
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.BackgroundColor = XLColor.Aqua;
                headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                foreach (var cell in ws.Row(1).Cells())
                {
                    cell.Style = headerStyle;
                }

                // Přidání dat
                var row = 2;
                foreach (var b in budgets)
                {
                    ws.Cell(row, 1).Value = b.Year;
                    ws.Cell(row, 2).Value = b.Month;
                    ws.Cell(row, 3).Value = b.Category;
                    ws.Cell(row, 4).Value = b.Limit;
                    row++;
                }

                // Autofit sloupce
                ws.Columns().AdjustToContents();

                // Uložení souboru
                workbook.SaveAs(filePath);
            }
        }

        static void ExportBudgetsToPdf(FinanceContext db)
        {
            var budgets = db.Budgets.ToList();
            Console.Write("Zadejte název souboru (bez přípony): ");
            var fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"rozpocty_{DateTime.Now:yyyyMMdd_HHmmss}";
            var filePath = $"{fileName}.pdf";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Výpis rozpočtů").Bold().FontSize(20);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Hlavička tabulky
                        table.Cell().Text("ID");
                        table.Cell().Text("Rok");
                        table.Cell().Text("Měsíc");
                        table.Cell().Text("Kategorie");
                        table.Cell().Text("Limit");

                        // Data
                        foreach (var b in budgets)
                        {
                            table.Cell().Text(b.Id.ToString());
                            table.Cell().Text(b.Year.ToString());
                            table.Cell().Text(b.Month.ToString());
                            table.Cell().Text(b.Category);
                            table.Cell().Text(b.Limit.ToString("N2"));
                        }
                    });
                });
            });
            document.GeneratePdf(filePath);
            AnsiConsole.MarkupLine($"[green]Úspěšně exportováno do {filePath}[/]");
        }

        static void ImportTransactionsFromCsv(FinanceContext db)
        {
            // Základní implementace bez CsvHelper
            AnsiConsole.MarkupLine("[yellow]Import transakcí z CSV není implementován.[/]");
        }

        static void ImportBudgetsFromCsv(FinanceContext db)
        {
            // Základní implementace bez CsvHelper
            AnsiConsole.MarkupLine("[yellow]Import rozpočtů z CSV není implementován.[/]");
        }

        // Doplnění prázdných metod pro všechny volané příkazy ve switch
        static void LoadCategoryColors() { }
        static void DeleteTransaction(FinanceContext db) { }
        static void EditTransaction(FinanceContext db) { }
        static void ShowHelp() { }
        static void ShowCategoryReport(FinanceContext db) { }
        static void ShowPieReport(FinanceContext db) { }
        static void ShowBalanceTrend(FinanceContext db) { }
        static void ExportPieReportToPdf(FinanceContext db) { }
        static void ExportBalanceTrendToPdf(FinanceContext db) { }
        static void EditBudget(FinanceContext db) { }
        static void DeleteBudget(FinanceContext db) { }
        static void ShowInteractiveMenu(FinanceContext db) { }
        static void ImportTransactionsBatch(FinanceContext db) { }
        static void ExportTransactionsBatch(FinanceContext db) { }
        static void ManageCategories() { }
        static void ImportTransactionsFromZip(FinanceContext db) { }
        static void ExportTransactionsToZip(FinanceContext db) { }
        static void BackupDatabase() { }
        static void RestoreDatabase() { }
        static void Register(FinanceContext db)
        {
            if (CurrentUser == null || CurrentUser.Role != "admin")
            {
                AnsiConsole.MarkupLine("[red]Pouze admin může registrovat nové uživatele.[/]");
                return;
            }
            Console.Write("Uživatelské jméno: ");
            var username = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                AnsiConsole.MarkupLine("[red]Jméno je povinné.[/]");
                return;
            }
            if (db.Users.Any(u => u.Username == username))
            {
                AnsiConsole.MarkupLine("[red]Uživatel již existuje.[/]");
                return;
            }
            Console.Write("Zobrazované jméno: ");
            var displayName = Console.ReadLine()?.Trim();
            Console.Write("Heslo: ");
            var password = Console.ReadLine();
            Console.Write("Role (admin/user): ");
            var role = Console.ReadLine()?.Trim().ToLower();
            if (role != "admin" && role != "user") role = "user";
            var hash = PasswordHelper.HashPassword(password);
            db.Users.Add(new User { Username = username, DisplayName = displayName, PasswordHash = hash, Role = role });
            db.SaveChanges();
            AnsiConsole.MarkupLine("[green]Registrace úspěšná.[/]");
        }

        static void Login(FinanceContext db)
        {
            Console.Write("Uživatelské jméno: ");
            var username = Console.ReadLine()?.Trim();
            Console.Write("Heslo: ");
            var password = Console.ReadLine();
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                LogAudit(db, "LoginFailed", $"Username: {username}");
                AnsiConsole.MarkupLine("[red]Neplatné přihlašovací údaje.[/]");
                return;
            }
            // 2FA demo: vygeneruj kód a ověř
            var code = new Random().Next(100000, 999999).ToString();
            LogAudit(db, "Login2FARequested", $"UserId: {user.Id}");
            Console.WriteLine($"2FA kód: {code}"); // V produkci poslat e-mailem
            Console.Write("Zadejte 2FA kód: ");
            var inputCode = Console.ReadLine();
            if (inputCode != code)
            {
                LogAudit(db, "Login2FAFailed", $"UserId: {user.Id}");
                AnsiConsole.MarkupLine("[red]Neplatný 2FA kód.[/]");
                return;
            }
            CurrentUser = user;
            LogAudit(db, "LoginSuccess", $"UserId: {user.Id}");
            AnsiConsole.MarkupLine($"[green]Přihlášen jako {user.DisplayName} ({user.Role})[/]");
        }

        static void Logout()
        {
            CurrentUser = null;
            AnsiConsole.MarkupLine("[yellow]Odhlášeno.[/]");
        }

        static void ChangePassword(FinanceContext db)
        {
            if (CurrentUser == null)
            {
                AnsiConsole.MarkupLine("[red]Nejste přihlášeni.[/]");
                return;
            }
            Console.Write("Staré heslo: ");
            var oldPwd = Console.ReadLine();
            if (!PasswordHelper.VerifyPassword(oldPwd, CurrentUser.PasswordHash))
            {
                LogAudit(db, "ChangePasswordFailed", $"UserId: {CurrentUser.Id}");
                AnsiConsole.MarkupLine("[red]Staré heslo je špatně.[/]");
                return;
            }
            Console.Write("Nové heslo: ");
            var newPwd = Console.ReadLine();
            CurrentUser.PasswordHash = PasswordHelper.HashPassword(newPwd);
            db.Users.Update(CurrentUser);
            db.SaveChanges();
            LogAudit(db, "ChangePasswordSuccess", $"UserId: {CurrentUser.Id}");
            AnsiConsole.MarkupLine("[green]Heslo změněno.[/]");
        }

        static void LogAudit(FinanceContext db, string action, string details = null)
        {
            db.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.Now,
                UserId = CurrentUser?.Id,
                Action = action,
                Details = details
            });
            db.SaveChanges();
        }

        static void SetGoal(FinanceContext db) { }
        static void ShowGoals(FinanceContext db) { }
        static void ShowDashboard(FinanceContext db) { }
        static void SearchTransactions(FinanceContext db) { }
        static void ShowAdvancedExpenseAnalysis(FinanceContext db) { }
        static async Task ShowAIBudgetTip(FinanceContext db)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AnsiConsole.MarkupLine("[red]OpenAI API klíč není nastaven.[/]");
                return;
            }
            var service = new OpenAIService(apiKey);
            var history = string.Join("; ", db.Transactions.OrderByDescending(t => t.Date).Take(20).Select(t => $"{t.Date:yyyy-MM-dd}: {t.Amount} {t.Category}"));
            var tip = await service.GetBudgetRecommendationAsync(history);
            AnsiConsole.MarkupLine($"[green]AI tip na rozpočet:[/] {tip}");
        }

        static async Task ShowAIAnalyze(FinanceContext db)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AnsiConsole.MarkupLine("[red]OpenAI API klíč není nastaven.[/]");
                return;
            }
            var service = new OpenAIService(apiKey);
            var history = string.Join("; ", db.Transactions.OrderByDescending(t => t.Date).Take(20).Select(t => $"{t.Date:yyyy-MM-dd}: {t.Amount} {t.Category}"));
            var analysis = await service.AnalyzeTransactionsAsync(history);
            AnsiConsole.MarkupLine($"[green]AI analýza výdajů:[/] {analysis}");
        }

        static async Task ShowAIChat()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AnsiConsole.MarkupLine("[red]OpenAI API klíč není nastaven.[/]");
                return;
            }
            var service = new OpenAIService(apiKey);
            Console.Write("Zeptejte se AI: ");
            var userMsg = Console.ReadLine();
            var reply = await service.ChatbotAsync(userMsg);
            AnsiConsole.MarkupLine($"[green]AI odpověď:[/] {reply}");
        }

        static async Task ShowAICategorize()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AnsiConsole.MarkupLine("[red]OpenAI API klíč není nastaven.[/]");
                return;
            }
            var service = new OpenAIService(apiKey);
            Console.Write("Popis transakce: ");
            var desc = Console.ReadLine();
            var category = await service.CategorizeTransactionAsync(desc);
            AnsiConsole.MarkupLine($"[green]AI doporučená kategorie:[/] {category}");
        }

        static void GenerateRecurringTransactions(FinanceContext db)
        {
            var today = DateTime.Today;
            var recs = db.RecurringTransactions.ToList();
            foreach (var rec in recs)
            {
                // Zjistit poslední vygenerovanou transakci pro tento RecurringTransaction
                var last = db.Transactions
                    .Where(t => t.Category == rec.Category && t.Description == rec.Description && t.Amount == rec.Amount)
                    .OrderByDescending(t => t.Date)
                    .FirstOrDefault();
                DateTime nextDate;
                if (last != null)
                    nextDate = last.Date;
                else
                    nextDate = rec.StartDate;
                // Generovat nové transakce až do dneška
                while (true)
                {
                    DateTime genDate = nextDate;
                    if (rec.Frequency == "monthly" && rec.DayOfMonth.HasValue)
                    {
                        genDate = new DateTime(genDate.Year, genDate.Month, rec.DayOfMonth.Value);
                        if (genDate < nextDate) genDate = genDate.AddMonths(1);
                    }
                    else if (rec.Frequency == "weekly" && rec.DayOfWeek.HasValue)
                    {
                        int daysToAdd = ((int)rec.DayOfWeek.Value - (int)genDate.DayOfWeek + 7) % 7;
                        genDate = genDate.AddDays(daysToAdd);
                        if (genDate < nextDate) genDate = genDate.AddDays(7);
                    }
                    else
                    {
                        genDate = nextDate;
                    }
                    if (genDate > today) break;
                    // Zkontrolovat, zda už transakce pro tento den existuje
                    bool exists = db.Transactions.Any(t => t.Date == genDate && t.Category == rec.Category && t.Description == rec.Description && t.Amount == rec.Amount);
                    if (!exists)
                    {
                        db.Transactions.Add(new Transaction
                        {
                            Date = genDate,
                            Amount = rec.Amount,
                            Description = rec.Description,
                            Category = rec.Category,
                            Currency = "CZK",
                            ExchangeRate = 1
                        });
                        db.SaveChanges();
                    }
                    nextDate = genDate.AddDays(rec.Frequency == "monthly" ? 30 : rec.Frequency == "weekly" ? 7 : 1);
                }
            }
        }

        static void CheckGoalNotifications(FinanceContext db)
        {
            if (CurrentUser == null) return;
            var today = DateTime.Today;
            // Upozornění na blížící se deadline cíle (do 7 dní)
            var goals = db.Goals.Where(g => g.UserId == CurrentUser.Id && g.Deadline.HasValue).ToList();
            foreach (var goal in goals)
            {
                var daysLeft = (goal.Deadline.Value - today).TotalDays;
                if (daysLeft <= 7 && daysLeft > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]Upozornění: Cíl '{goal.Name}' má deadline za {daysLeft:N0} dní![/]");
                }
                else if (daysLeft <= 0)
                {
                    AnsiConsole.MarkupLine($"[red]Upozornění: Deadline cíle '{goal.Name}' vypršela![/]");
                }
            }
        }
    }
}
