using System.Linq;
using System.Reflection;

#nullable enable

public static class ObjectExtension
{
    public static string GetObjectString(this object? obj, bool includePrivate = false)
    {
        if (obj == null) return "null";

        var output = $"{obj}";
        var type = obj.GetType();
        var publicFields = type.GetFields().Where(f => f.IsPublic);
        foreach (var f in publicFields)
        {
            var value = f.GetValue(obj);
            var valueString = value == null ? "null" : value.ToString();
            output += $"\n {f.Name}: {valueString}";
        }

        var privateFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in privateFields)
        {
            var value = f.GetValue(obj);
            var valueString = value == null ? "null" : value.ToString();
            output += $"\n[private] {f.Name}: {valueString}";
        }

        var publicProps = type.GetProperties();
        foreach (var f in publicProps)
        {
            var value = f.GetValue(obj, null);
            var valueString = value == null ? "null" : value.ToString();
            output += $"\n {f.Name}: {valueString}";
        }

        return output;
    }
}