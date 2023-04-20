using Marketplace.Modules.Marketplace_NPC;

namespace Marketplace;

[Market_Autoload(Market_Autoload.Type.Client,Market_Autoload.Priority.Normal, "OnInit")]
public static class Global_Values_Client
{
    private static void OnInit()
    {
        Global_Values._container.ValueChanged += CurrencyChange;
    }

    private static void CurrencyChange()
    {
        Marketplace_Main_Client.OnUpdateCurrency();
    }
}