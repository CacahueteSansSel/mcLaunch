using System.Threading.Tasks;
using Avalonia.Controls;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Pages.BoxDetails;

public interface ISubControl : IControl
{
    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public string Title { get; }
    
    Task PopulateAsync();
}