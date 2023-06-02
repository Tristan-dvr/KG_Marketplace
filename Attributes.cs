namespace Marketplace;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Market_Autoload : Attribute
{
    public enum Priority
    {
        Init, First, Normal, Last
    }
    public enum Type
    {
        Server, Client, Both
    }
    public readonly Priority priority;
    public readonly Type type;
    public readonly string InitMethod;
    public readonly string[] OnWatcherNames;
    public readonly string[] OnWatcherMethods;
    public Market_Autoload(Type type, Priority priority, string InitMethod = null, string[] OnWatcherNames = null, string[] OnWatcherMethods = null)
    {
        this.priority = priority;
        this.type = type;
        this.InitMethod = InitMethod;
        this.OnWatcherNames = OnWatcherNames;
        this.OnWatcherMethods = OnWatcherMethods;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClientOnlyPatch : Attribute{}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServerOnlyPatch : Attribute{}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConditionalPatch : Attribute
{
    private readonly string Condition;
    public ConditionalPatch(string Condition = "Condition") => this.Condition = Condition;
    public bool Check(Type t)
    {
        MethodInfo method = AccessTools.Method(t, Condition);
        if (method != null) return (bool)method.Invoke(null, null);
        Utils.print($"Error loading {t.Name} conditional patch, method {Condition} not found", ConsoleColor.Red);
        return false;
    }
    
    
}