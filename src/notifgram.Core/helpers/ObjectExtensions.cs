using System.Text.Json;

namespace notifgram.Core.helpers;
public static class ObjectExtensions
{

  public static string Dump(this object? obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj);
    }
    catch (Exception)
    {
      return string.Empty;
    }
  }

  public static  string ToString(this object? obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj);
    }
    catch (Exception)
    {
      return string.Empty;
    }

  }

}

public static class ListExtensions
{
  public static string Dump(this List<Object>? obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj);
    }
    catch (Exception)
    {
      return string.Empty;
    }
  }
  public static string ToString(this List<Object>? obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj);
    }
    catch (Exception)
    {
      return string.Empty;
    }
  }

}
