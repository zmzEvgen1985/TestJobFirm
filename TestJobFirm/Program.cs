using System;
using Microsoft.Data.Sqlite;

namespace TestJobFirm
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1.Суммарную зарплату в разрезе департаментов(без руководителей и с руководителями)
            string strSQL_1 = "Select id, Name, (Select Sum(Salary) From employee Where department_id = department.id and employee.chief_id is not null) " +
                                   "From department";
            // 2. Департамент, в котором у сотрудника зарплата максимальна (не у руководителя.)
            string strSQL_2 = "Select  (Select Name From department Where department_id = id), Name " +
                              "From employee " +
                              "Where salary = (Select MAX(salary) From employee Where department_id <> 3)";
            // 3. Зарплаты руководителей департаментов (по убыванию)
            string strSQL_3 = "Select Name, (Select Name From department Where department_id = id), salary  " +
                              "From employee Where department_id = 3 Order By salary DESC";

            var connect = new SqliteConnection("Data Source=MyFurm.db");
            {
                // Работа с запросами 
                connect.Open();

                try
                {
                    SqliteCommand command_1 = new SqliteCommand(strSQL_1, connect);
                    SqliteDataReader reader_1 = command_1.ExecuteReader();
                    {
                        Console.WriteLine("Задание № 1: Суммарную зарплату в разрезе департаментов (без руководителей и с руководителями)");
                        if (reader_1.HasRows)
                        {
                            Console.WriteLine($"Суммарная зарплата в разрезе департаментов (без руководителей / с руководителями)");
                            Console.WriteLine($"Деп-т\tБез рук.\tС рук.");
                            while (reader_1.Read())
                            {
                                var depID = reader_1.GetValue(0);
                                var dep = reader_1.GetValue(1);
                                var sum = reader_1.GetInt32(2);
   
                                string strSQL_4 = $"Select distinct chief_id, department_id from employee where chief_id is not null and department_id = {depID}";
                                SqliteCommand command_4 = new SqliteCommand(strSQL_4, connect);
                                SqliteDataReader reader_4 = command_4.ExecuteReader();
                                while (reader_4.Read())
                                {
                                    var ChiefID = reader_4.GetValue(0);
                                    string strSQL_5 = $"Select salary from employee where id = {ChiefID}";
                                    SqliteCommand command_5 = new SqliteCommand(strSQL_5, connect);
                                    SqliteDataReader reader_5 = command_5.ExecuteReader();
                                    while (reader_5.Read())
                                    {
                                        var Salary = reader_5.GetInt32(0);
                                        int sumRabandRuk = Salary + sum;
                                        Console.WriteLine($"{dep}\t{sum}\t\t{sumRabandRuk}");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine("Задание № 2: Департамент, в котором у сотрудника зарплата максимальна");
                    SqliteCommand command_2 = new SqliteCommand(strSQL_2, connect);
                    SqliteDataReader reader = command_2.ExecuteReader();
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var dep = reader.GetValue(0);
                                var rab = reader.GetValue(1);
                                Console.WriteLine($"Максимальная зарплата у сотрудника: {rab} в департаменте: {dep}");
                            }
                        }
                    }
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine("Задание № 3: Зарплаты руководителей департаментов (по убыванию)");
                    SqliteCommand command_3 = new SqliteCommand(strSQL_3, connect);
                    SqliteDataReader reader_3 = command_3.ExecuteReader();
                    {
                        if (reader_3.HasRows)
                        {
                            Console.WriteLine("Рук-ль.\tЗ.П.");
                            while (reader_3.Read())
                            {
                                var NameRuk = reader_3.GetValue(0);
                                var SalRuk = reader_3.GetValue(2);
                                Console.WriteLine($"{NameRuk}\t{SalRuk}");
                            }
                        }
                    }
                }
                catch
                {
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connect;
                    connect.Open();
                    // Создание таблицы Департамент
                    command.CommandText = "CREATE TABLE department(id int(11), Name varchar(100))";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Таблица Департаменты создана");
                    // Создание таблицы Работники
                    command.CommandText = "CREATE TABLE employee(id int(11), department_id int(11), " +
                                                            "chief_id int(11), Name varchar(100), salary int(11))";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Таблица Работники создана");

                    // Добавление данных в таблицы
                    command.CommandText = "INSERT INTO department (id, name) VALUES (1, 'D1'),(2, 'D2'),(3, 'D3')";
                    int numberdepart = command.ExecuteNonQuery();
                    Console.WriteLine($"Добавлено объектов: {numberdepart}");
                        
                    command.CommandText = "INSERT INTO employee (id, department_id, chief_id, name, salary)" +
                        "VALUES (1, 1, 5, 'John', 100), (2, 1, 5, 'Misha', 600), (3, 2, 6, 'Eugen', 300), (4, 2, 6, 'Tolya', 400)," +
                           "(5, 3, 7, 'Stepan', 500), (6, 3, 7, 'Alex', 1000), (7, 3, NULL, 'Ivan', 1100)";
                    int numberempl = command.ExecuteNonQuery();
                    Console.WriteLine($"Добавлено объектов: {numberempl}");
                    Console.WriteLine($"Перезапустите программу");
                 }
            }
            Console.ReadKey();
        }
    }
}
