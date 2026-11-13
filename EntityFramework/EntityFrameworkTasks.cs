using DatabaseCourseExam.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseCourseExam.EntityFramework;

public static class EntityFrameworkTasks
{
    // TODO (задание 2): Вывести студентов с названием группы; обеспечить загрузку связанной сущности без обращения к незагруженному свойству.
    public static async Task ShowStudentsAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        List<Student> students = await context.Students
            .Include(student => student.Group)
            .OrderByDescending(student => student.Id)
            .ToListAsync();
        foreach (var student in students)
        {
            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {student.Group.Name}");
        }
    }

    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var context = SchoolDbContextFactory.Create();

        Student? student = await context.Students
            .FirstOrDefaultAsync(x => x.Id == studentId);

        if (student is null)
        {
            Console.WriteLine("Студент не найден.");
            return;
        }

        student.Age = newAge;

        int affectedRows = await context.SaveChangesAsync();

        Console.WriteLine(
            $"Возраст изменён. Сохранено изменений: {affectedRows}");
    }

    // TODO (задание 8): Выбирать только нужные поля через Select и добавить вычисляемый признак «мест нет».
    public static async Task ShowCoursesAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        var courses = await context.Courses
            .Select(course => new
            {
                course.AvailablePlaces,
                course.Name,
                course.Price
            })
            .OrderBy(course => course.Name)
            .ToListAsync();

        foreach (var course in courses)
        {
            if (course.AvailablePlaces > 0)
            {
                Console.WriteLine(
                    $"{course.Name} | Цена: {course.Price:N2} | " +
                    $"Свободных мест: {course.AvailablePlaces}");
            }
            else Console.WriteLine($"{course.Name} - мест нет!");
        }
    }
}