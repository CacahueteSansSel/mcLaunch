using System.Threading.Tasks;
using Avalonia.Controls;
using ddLaunch.Core.Boxes;

namespace ddLaunch.Views.Pages.BoxDetails;

public interface ISubControl : IControl
{
    public Box Box { get; set; }
    public string Title { get; }
    
    Task PopulateAsync();
}