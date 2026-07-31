namespace Benchmark;

public class NameGenerator
{
    static string GenerateArabicName()
    {
        var random = new Random();
    
        // 1. Authentic Arabic Name Pools
        var maleFirstNames = new[] { "محمد", "أحمد", "محمود", "علي", "عمر", "عثمان", "يوسف", "إبراهيم", "خالد", "عبدالله", "عبدالرحمن", "مصطفى", "حسن", "حسين", "كريم", "طارق", "عمرو", "هاني", "وائل", "زياد" };
        var femaleFirstNames = new[] { "فاطمة", "عائشة", "زينب", "مريم", "سارة", "نور", "أمل", "منى", "ريم", "هند", "دعاء", "شيماء", "يارا", "ندى", "سلمى", "آية", "رانيا", "هبة", "إيمان", "غادة" };
        var middleAndLastNames = new[] { "أحمد", "محمد", "محمود", "علي", "حسن", "حسين", "إبراهيم", "مصطفى", "العربي", "منصور", "شاكر", "السيد", "بدوي", "كامل", "سعيد", "سليم", "الخطيب", "عوض", "راضي", "شرف" };

        string firstName = random.Next(2) == 0 
            ? maleFirstNames[random.Next(maleFirstNames.Length)] 
            : femaleFirstNames[random.Next(femaleFirstNames.Length)];

        int totalParts = random.Next(3, 6); 
        var nameParts = new List<string> { firstName };

        for (int i = 1; i < totalParts; i++)
        {
            nameParts.Add(middleAndLastNames[random.Next(middleAndLastNames.Length)]);
        }

        return string.Join(" ", nameParts);
    }

    public static List<string> GenerateArabicName(int count)
    {
        List<string> names = [];
        for (int i = 0; i < count; i++)
        {
            names.Add(GenerateArabicName());
        }

        return names;
    }
}