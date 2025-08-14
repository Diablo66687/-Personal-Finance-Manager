Osobn� finan�n� mana�er
=======================

Popis aplikace:
---------------
Aplikace slou�� ke spr�v� osobn�ch financ�, evidenci p��jm� a v�daj�, rozpo�t�, c�l�, opakovan�ch plateb a nab�z� AI asistenci pro anal�zu a tipy. Umo��uje export dat, z�lohov�n� a spr�vu u�ivatel� s rozli�en�m rol�.

Pou�it� technologie:
--------------------
- .NET 8 (C#)
- Entity Framework Core (SQLite)
- Spectre.Console (modern� konzolov� UI)
- ClosedXML, QuestPDF (export do Excel/PDF)
- SkiaSharp (grafy)
- OpenAI API (AI funkce)

Bezpe�nost:
-----------
- Hesla jsou ukl�d�na pouze jako hash (PBKDF2)
- Citliv� popisy transakc� jsou �ifrov�ny (AES)
- Role u�ivatel� (admin/user) omezuj� p��stup k funkc�m
- Audit logy eviduj� d�le�it� akce
- Dvoufaktorov� autentizace (2FA) p�i p�ihl�en�

Funkce aplikace:
----------------
- Evidence transakc� (p��jmy/v�daje, kategorie, m�ny)
- Spr�va rozpo�t� a upozorn�n� na p�ekro�en�
- Opakovan� platby (automatick� generov�n�)
- C�le a notifikace na deadline
- Export do Excel/PDF, import CSV
- Z�lohov�n� datab�ze
- AI tipy, anal�zy, chat, automatick� kategorizace
- Spr�va u�ivatel�, zm�na hesla, role

N�vod na spu�t�n�:
------------------
1. St�hn�te a rozbalte slo�ku s aplikac�.
2. Spus�te soubor ConsoleApp4.exe (nebo FinanceManager.exe) ve slo�ce publish.
3. Pro AI funkce nastavte OpenAI API kl��:
   - V p��kazov� ��dce spus�te:
     set OPENAI_API_KEY=v�_api_kl��
   - Nebo v PowerShellu:
     $env:OPENAI_API_KEY="v�_api_kl��"
4. Aplikace je p�ipravena k pou�it�.

Dal�� informace:
----------------
- Pro instalaci lze pou��t p�ipraven� instal�tor (wizard).
- Data jsou ukl�d�na do souboru finance.db (SQLite).
- Pro spr�vu u�ivatel� se p�ihlaste jako admin (demo ��et: admin/admin).

