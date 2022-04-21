using MudBlazor;

namespace PutAway.Client.Themes;

public class PutAwayTheme : MudTheme
{
    public PutAwayTheme()
    {
        Typography = new Typography()
        {
           H1 = new H1()
           {
               FontSize = "2rem"
           }
        };
    }
}