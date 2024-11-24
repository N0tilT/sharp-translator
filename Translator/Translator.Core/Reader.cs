/// <summary>
/// Статический класс, отвечающий за чтение символов из файла.
/// </summary>
public static class Reader
{
    private static int lineNumber;
    private static int characterPositionInLine;
    private static int currentSymbol;

    private static string code;

    /// <summary>
    /// Номер текущей строки.
    /// </summary>
    public static int LineNumber => lineNumber;

    /// <summary>
    /// Позиция текущего символа в строке.
    /// </summary>
    public static int CharacterPositionInLine => characterPositionInLine;

    /// <summary>
    /// Текущий читаемый символ.
    /// </summary>
    public static char CurrentSymbol => code[currentSymbol];

    /// <summary>
    /// Константа, представляющая конец файла.
    /// </summary>
    public const char EndOfFile = '\0';

    /// <summary>
    /// Читает следующий символ из файла и обновляет состояние строки и позиции.
    /// </summary>
    public static void ReadNextSymbol()
    {
        currentSymbol++;
        if (currentSymbol >= code.Length)
            return;

        if (code[currentSymbol] == EndOfFile)
        {
            return;
        }
        else if (code[currentSymbol] == '\n')
        {
            lineNumber++;
            characterPositionInLine = 0;
        }
        else if (code[currentSymbol] == '\r' || code[currentSymbol] == '\t')
        {
            ReadNextSymbol();
            return;
        }
        else
        {
            characterPositionInLine++;
        }
    }

    /// <summary>
    /// Инициализирует чтение из указанного файла.
    /// </summary>
    /// <param name="code">Исходный файл для чтения.</param>
    public static void Initialize(string code)
    {
        Reader.code = code;
        currentSymbol = -1;
        lineNumber = 1;
        characterPositionInLine = 0;
        ReadNextSymbol();
    }

}
