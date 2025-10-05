# ReolMarket
Casen til 2. semester

# Import af database i SSMS:

# Import af .bacpac i SQL Server Management Studio (SSMS)

Denne guide viser, hvordan du kan importere en SQL Server-database fra en `.bacpac`-fil.

---

##  Trin-for-trin

1. **Kopiér `.bacpac`-filen** til din maskine  
   F.eks.: `C:\Temp\MinDatabase.bacpac`  
     >  Pak ikke filen ud – `.bacpac` er i forvejen et zip-arkiv!

2. **Åbn SSMS** og forbind til din SQL Server-instans  
   - Det kan f.eks. være `localhost`, `(localdb)\MSSQLLocalDB` eller en Azure SQL Server.

3. **Højreklik på `Databases` → vælg `Import Data-tier Application…`**

4. Klik **Next** på intro-siden.

5. Vælg **Import from local disk**  
   - Klik **Browse…** og vælg din `.bacpac`-fil  
   - Klik **Next**

6. På **Database Settings**:
   - **Database name**: Skriv et navn til databasen (fx `MinDatabase`)
   - (Valgfrit) ændr **Data file path**, hvis du vil gemme datafilerne et andet sted
   - Klik **Next**

7. Gennemgå **Summary** og klik **Finish** for at starte importen.

8. Når **Operation Complete** vises, klik **Close**.  
   Udvid **Databases** i Object Explorer for at se den nye database.

---

## Test at importen lykkedes

1. Højreklik på den nye database → **New Query**
2. Kør fx:

   ```sql
   SELECT TOP (10) * FROM sys.tables;
