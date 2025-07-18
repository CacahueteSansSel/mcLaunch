using Avalonia.Controls;
using SharpNBT;

namespace mcLaunch.Views.Windows.NbtEditor;

public partial class NbtViewTagSnbtWindow : Window
{
    public NbtViewTagSnbtWindow()
    {
        InitializeComponent();
        SnbtTextEditor.Text = "// No tag was passed";
    }

    public NbtViewTagSnbtWindow(Tag tag) : this()
    {
        SnbtTextEditor.Text = PrettifySnbt(tag.Stringify());
    }

    private string PrettifySnbt(string input)
    {
        string final = "";
        bool inQuotes = false;
        bool inSquareBrackets = false;
        int indent = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            char? nextChar = i < input.Length - 1 ? input[i + 1] : null;
            if (c == '"') inQuotes = !inQuotes;

            if (!inQuotes)
            {
                if (c == '[') inSquareBrackets = true;

                if (c == '{')
                {
                    indent += 4;
                    final +=
                        $"{(inSquareBrackets ? $"\n{new string(' ', indent)}" : "")}{{\n{new string(' ', indent + (inSquareBrackets ? 4 : 0))}";
                    continue;
                }

                if (c == '}')
                {
                    bool addNewLine = nextChar != ',';
                    indent -= 4;
                    final +=
                        $"\n{new string(' ', indent + (inSquareBrackets ? 4 : 0))}}}{(addNewLine ? $"\n{(inSquareBrackets ? new string(' ', indent) : "")}" : "")}";
                    continue;
                }

                if (c == ':')
                {
                    final += ": ";
                    continue;
                }

                if (c == ',')
                {
                    bool addNewLine = !inSquareBrackets;
                    final += $",{(addNewLine ? $" \n{new string(' ', indent)}" : "")}";
                    continue;
                }

                if (c == ']') inSquareBrackets = false;
            }

            final += c;
        }

        return final;
    }
}