Osobní finanèní manaer
=======================

Popis aplikace:
---------------
Aplikace slouí ke správì osobních financí, evidenci pøíjmù a vıdajù, rozpoètù, cílù, opakovanıch plateb a nabízí AI asistenci pro analızu a tipy. Umoòuje export dat, zálohování a správu uivatelù s rozlišením rolí.

Pouité technologie:
--------------------
- .NET 8 (C#)
- Entity Framework Core (SQLite)
- Spectre.Console (moderní konzolové UI)
- ClosedXML, QuestPDF (export do Excel/PDF)
- SkiaSharp (grafy)
- OpenAI API (AI funkce)

Bezpeènost:
-----------
- Hesla jsou ukládána pouze jako hash (PBKDF2)
- Citlivé popisy transakcí jsou šifrovány (AES)
- Role uivatelù (admin/user) omezují pøístup k funkcím
- Audit logy evidují dùleité akce
- Dvoufaktorová autentizace (2FA) pøi pøihlášení

Funkce aplikace:
----------------
- Evidence transakcí (pøíjmy/vıdaje, kategorie, mìny)
- Správa rozpoètù a upozornìní na pøekroèení
- Opakované platby (automatické generování)
- Cíle a notifikace na deadline
- Export do Excel/PDF, import CSV
- Zálohování databáze
- AI tipy, analızy, chat, automatická kategorizace
- Správa uivatelù, zmìna hesla, role

Návod na spuštìní:
------------------
1. Stáhnìte a rozbalte sloku s aplikací.
2. Spuste soubor ConsoleApp4.exe (nebo FinanceManager.exe) ve sloce publish.
3. Pro AI funkce nastavte OpenAI API klíè:
   - V pøíkazové øádce spuste:
     set OPENAI_API_KEY=váš_api_klíè
   - Nebo v PowerShellu:
     $env:OPENAI_API_KEY="váš_api_klíè"
4. Aplikace je pøipravena k pouití.

Další informace:
----------------
- Pro instalaci lze pouít pøipravenı instalátor (wizard).
- Data jsou ukládána do souboru finance.db (SQLite).
- Pro správu uivatelù se pøihlaste jako admin (demo úèet: admin/admin).

