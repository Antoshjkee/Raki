namespace Raki.TegramBot.Core;
public class Employee
{
    public string Name { get; set; }
    public int Age { get; set; }

    public bool IsEligibleForDiscount()
    {
        return Age >= 60; // Magic number: 60
    }
}
