using System.Threading.Tasks;
using Avalonia.Controls;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Pages.BoxDetails;

public abstract class SubControl : UserControl
{
    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public virtual string Title { get; }

    public abstract Task PopulateAsync();
}