namespace ReolMarket.MVVM.Model
{
    internal class Economy
    {
        public decimal Rent = 7500;
        public decimal Electricity = 1500;
        public decimal Water = 500;
        public decimal Heating = 100;
        public decimal Internet = 300;
        public decimal JonasFixedSalary = 25000;
        public decimal SofieFixedSalary = 25000;
        public decimal MetteHourlySalary = 155;
        public decimal MetteWeeklyHours = 25;
        public decimal SalaryPeriod = 4.33M;
        public decimal MetteSalary => MetteHourlySalary * MetteWeeklyHours * SalaryPeriod;
        public int MonthlyRentedBooths = 0;
        public int MonthlyAvailableBooths = 0;
        public decimal MonthlyRentIncome;
        public decimal MonthlyCommission;
        public decimal MonthlyOutstanding;
        public decimal MonthlySalesTotal; // placeholder, fetch data from sales.
        public int MonthlySalesAmount; // placeholder, fetch data from sales.
        public decimal MonthlyComission; // placeholder, fetch data from sales.
    }
}
