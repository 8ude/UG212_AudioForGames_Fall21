namespace MutCommon
{
  public static class StringExtensionMethods
  {
    public static string MinimizeWhiteSpace(this string input)
    {
      var output = input.Trim();
      do
      {
        output = output.Replace("  ", " ");
      } while (output.IndexOf("  ") != -1);
      return output;
    }
  }

}