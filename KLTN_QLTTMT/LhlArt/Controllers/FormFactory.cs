using System.ComponentModel;
using System.Reflection;
using LhlArt.Models;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;
internal static class FormFactory
{
    public static InputForm Edit<T>(CenterService s, T entity, bool isNew, string title)
        where T : Entity
    {
        var f = new InputForm((isNew ? "Thêm" : "Sửa") + " – " + title);
        foreach (var p in typeof(T).GetProperties())
        {
            if (p.Name == "Status")
                continue;
            IEnumerable<Choice>? choices = p.Name switch
            {
                "TypeId" => s.D.CourseTypes.Select(x => new Choice(x.Id, x.Id + " – " + x.Name)),
                "CourseId" => s.D.Courses.Select(x => new Choice(x.Id, x.Id + " – " + x.Name)),
                "TeacherId" => s.D.Teachers.Select(x => new Choice(x.Id, x.Id + " – " + x.Name)),
                "AssistantId" => new[]
                {
                    new Choice("", "Không có")
                }.Concat(s.D.Teachers.Select(x => new Choice(x.Id, x.Id + " – " + x.Name))),
                "RoomId" => s.D.Rooms.Select(x => new Choice(x.Id, x.Id + " – " + x.Name)),
                _ => null
            };
            if (p.Name == "Group")
                choices = new[]
                {
                    new Choice("Học phí", "Học phí"),
                    new Choice("Họa cụ", "Họa cụ")
                };
            f.Add(p.Name, p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.Name, p.GetValue(entity), choices, p.Name == "Id" && !isNew);
        }

        return f;
    }
}
