/// <summary>
/// Перечисление, представляющее различные лексемы или токены, используемые в лексическом анализе.
/// </summary>
public enum Lexems
{
    None, Name, True, False, Logical, Begin, End, Var, Print, Assign, If, While, EndIf, EndWhile, Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual, Then, ElseIf,Else,Do,
    LeftBracket, RightBracket, Semi, Comma, EOF,
    Disjunction, Conjunction, Implication,
    Negation, BinaryOp, Colon
}

// <summary>
// Структура, представляющая ключевое слово с его соответствующей лексемой.
// </summary>
public struct Keyword
{
    /// <summary>
    /// Ключевое слово
    /// </summary>
    public string word;

    /// <summary>
    /// Соответствующая ключевому слову лексема
    /// </summary>
    public Lexems lex;
}

/// <summary>
/// Статический класс, ответственный за лексический анализ исходного кода.
/// </summary>
public static class LexicalAnalyzer
{
    /// <summary>
    /// Массив ключевых слов
    /// </summary>
    private static Keyword[] keywords;

    /// <summary>
    /// Указатель для отслеживания добавленных ключевых слов
    /// </summary>
    private static int keywordsPointer;

    /// <summary>
    /// Текущая лексема, которая анализируется
    /// </summary>
    private static Lexems currentLexem;

    /// <summary>
    /// Текущее имя идентификатора
    /// </summary>
    private static string currentName;

    /// <summary>
    /// Максимальная длина названия идентификатора
    /// </summary>
    private const int MaxIdentifierLength = 50;

    /// <summary>
    /// Инициализирует лексический анализатор с указанным путем к файлу.
    /// </summary>
    /// <param name="filePath">Путь к исходному файлу.</param>
    public static void Initialize(string filePath)
    {
        keywords = new Keyword[20];
        keywordsPointer = 0;

        AddKeyword("Begin", Lexems.Begin);
        AddKeyword("End", Lexems.End);
        AddKeyword("Var", Lexems.Var);
        AddKeyword("Print", Lexems.Print);
        AddKeyword("Logical", Lexems.Logical);
        AddKeyword("If", Lexems.If);
        AddKeyword("While", Lexems.While);
        AddKeyword("Endif", Lexems.EndIf);
        AddKeyword("Endwhile", Lexems.EndWhile);
        AddKeyword("Then", Lexems.Then);
        AddKeyword("ElseIf", Lexems.If);
        AddKeyword("Else", Lexems.Else);
        AddKeyword("Do", Lexems.Do);

        Reader.Initialize(filePath);
        currentLexem = Lexems.None;
    }

    /// <summary>
    /// Добавляет ключевое слово в список ключевых слов.
    /// </summary>
    /// <param name="keyword">Ключевое слово в виде строки.</param>
    /// <param name="lex">Связанная лексема.</param>
    private static void AddKeyword(string keyword, Lexems lex)
    {
        Keyword kw = new Keyword { word = keyword, lex = lex };
        keywords[keywordsPointer++] = kw;
    }

    /// <summary>
    /// Получает лексему, связанную с указанным ключевым словом.
    /// </summary>
    /// <param name="keyword">Ключевое слово для получения лексемы.</param>
    /// <returns>Соответствующая лексема.</returns>
    private static Lexems GetKeywordLexem(string keyword)
    {
        for (int i = 0; i < keywordsPointer; i++)
        {
            if (keywords[i].word == keyword)
                return keywords[i].lex;
        }
        return Lexems.Name;
    }

    /// <summary>
    /// Парсит следующую лексему в исходном коде.
    /// </summary>
    public static void ParseNextLexem()
    {
        while (char.IsWhiteSpace(Reader.CurrentSymbol))
            Reader.ReadNextSymbol();

        if (Reader.CurrentSymbol == Reader.EndOfFile)
        {
            currentLexem = Lexems.EOF;
            return;
        }

        if (char.IsLetter(Reader.CurrentSymbol))
        {
            ParseIdentifier();
        }
        else if (char.IsDigit(Reader.CurrentSymbol))
        {
            if (Reader.CurrentSymbol == '0')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.False;
            }
            else if (Reader.CurrentSymbol == '1')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.True;
            }
        }
        else if (Reader.CurrentSymbol == '(')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.LeftBracket;
        }
        else if (Reader.CurrentSymbol == ')')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.RightBracket;
        }
        else if (Reader.CurrentSymbol == ';')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.Semi;
        }
        else if (Reader.CurrentSymbol == ':')
        {
            Reader.ReadNextSymbol();
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.Assign;
            }
            else
            {
                currentName = null;
                currentLexem = Lexems.Colon;
            }
        }
        else if (Reader.CurrentSymbol == ',')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.Comma;
        }
        else if (Reader.CurrentSymbol == '!')
        {
            Reader.ReadNextSymbol(); 
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.NotEqual;
            }
            else
            {
                currentName = null;
                currentLexem = Lexems.Negation;
            }
        }
        else if (Reader.CurrentSymbol == '&')
        {
            currentName = null;
            currentLexem = Lexems.Conjunction;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '|')
        {
            currentName = null;
            currentLexem = Lexems.Disjunction;
            Reader.ReadNextSymbol();

        }
        else if (Reader.CurrentSymbol == '^')
        {
            currentName = null;
            currentLexem = Lexems.Implication;
            Reader.ReadNextSymbol();
        }
        else if(Reader.CurrentSymbol == '<')
        {
            Reader.ReadNextSymbol();
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.LessOrEqual;
            }
            else
            {
                currentName = null;
                currentLexem = Lexems.Less;
            }
        }
        else if (Reader.CurrentSymbol == '>')
        {
            Reader.ReadNextSymbol();
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.GreaterOrEqual;
            }
            else
            {
                currentName = null;
                currentLexem = Lexems.Greater;
            }
        }
        else if (Reader.CurrentSymbol == '=')
        {
            Reader.ReadNextSymbol(); 
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.Equal;
            }
            else
            {
                throw new Exception($"Ошибка: Недопустимый символ: {Reader.CurrentSymbol}");
            }
        }
        else
        {
            throw new Exception($"Ошибка: Недопустимый символ: {Reader.CurrentSymbol}");
        }
    }

    /// <summary>
    /// Получает идентификатор из исходного кода
    /// </summary>
    /// <exception cref="Exception">Выдаёт исключение при превышении максимальной длины имени идентификатора</exception>
    private static void ParseIdentifier()
    {
        string identifier = string.Empty;

        do
        {
            identifier += Reader.CurrentSymbol;
            Reader.ReadNextSymbol();
        }
        while (char.IsLetter(Reader.CurrentSymbol) && identifier.Length < MaxIdentifierLength);

        if (identifier.Length >= MaxIdentifierLength)
        {
            throw new Exception("Ошибка: Длина идентификатора превышает максимальную допустимую.");
        }

        currentName = identifier;
        currentLexem = GetKeywordLexem(identifier);
    }


    /// <summary>
    /// Текущая лексема, которая анализируется
    /// </summary>
    public static Lexems CurrentLexem => currentLexem;
    
    /// <summary>
    /// Текущее имя идентификатора
    /// </summary>
    public static string? CurrentName => currentName;
}
