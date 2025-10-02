namespace ReolMarket.MVVM.Model
{
    internal class Economy
    {
        public int Rent = 7500;
        public int Electricity = 1500;
        public int Water = 500;
        public int Heating = 100;
        public int Internet = 300;
        public int JonasFixedSalary = 25000;
        public int SofieFixedSalary = 25000;
        public int MetteHourlySalary = 155;
        public int MetteWeeklyHours = 25;
        public double SalaryPeriod = 4.33;
        public double MetteSalary => MetteHourlySalary * MetteWeeklyHours * SalaryPeriod;
    }
}
