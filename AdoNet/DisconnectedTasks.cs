
using System.Data;
using DatabaseCourseExam.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

namespace DatabaseCourseExam.AdoNet;

public static class DisconnectedTasks
{
    // TODO (задание 5): Реализовать добавление новой строки в DataTable с проверкой возраста и правильным RowState Added.
    // TODO (задание 6): Добавить таблицу записей на курсы и вывести количество записей для каждого студента.
    public static async Task<DataTable> AddStudent()
    {
        var table = new DataTable("Students");
        Console.Write("Введите имя: ");
        string name = Console.ReadLine();
        Console.Write("Введите фамилию: ");
        string lname = Console.ReadLine();
        Console.Write("Введите возраст: ");
        int age = Convert.ToInt32(Console.ReadLine());
        bool ageCheck = age > 0 && age < 100;
        while (!ageCheck)
        {
            Console.Write("Введите коректный возраст: ");
            age = Convert.ToInt32(Console.ReadLine());
            ageCheck = age > 0 && age < 100;
        }
        Console.Write("Введите айди группы: ");
        int groupId = Convert.ToInt32(Console.ReadLine());
        table.Rows.Add(name, lname, age, groupId);
        return table;
    }
    public static async Task<DataTable> LoadStudentsAsync()
    {
        var table = new DataTable("Students");

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, first_name, last_name, age, group_id
            FROM students
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        using var adapter = new NpgsqlDataAdapter(command);

        adapter.Fill(table);
        table.PrimaryKey = [table.Columns["id"]!];

        return table;
    }

    public static void PrintTable(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine(
                $"{row["id"]}: {row["first_name"]} " +
                $"{row["last_name"]}, возраст {row["age"]}");
        }
    }

    // TODO (задание 1): Найти строку по первичному ключу, обработать отсутствие результата и не менять строку, если значение не изменилось.
    public static void ChangeAgeInMemory(
        DataTable table,
        int studentId,
        int newAge)
    {
        DataRow row = table.Rows.Find(studentId)!;
        if (row is null)
        {
            Console.WriteLine("Студент не найден в DataTable.");
            return;
        }
        if ((int)row["age"] == newAge)
        {
            Console.WriteLine("Возраст такой-же.");
            return;
        }
        row["age"] = newAge;

        Console.WriteLine(
            $"Возраст изменён в памяти. Состояние строки: {row.RowState}");
    }

    public static int SaveChanges(DataTable table)
    {
        using var connection = DatabaseConnection.Create();
        connection.Open();

        const string sql = """
        UPDATE students
        SET age = @age
        WHERE id = @id;
        """;

        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add("@age", NpgsqlTypes.NpgsqlDbType.Integer,
            4, "age");

        command.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer,
            4, "id");

        using var adapter = new NpgsqlDataAdapter();
        adapter.UpdateCommand = command;

        int affectedRows = adapter.Update(table);
        Console.WriteLine($"Сохранено изменений: {affectedRows}");

        return affectedRows;
    }
}