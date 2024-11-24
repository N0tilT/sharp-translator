



namespace Translator.Core
{
    /// <summary>
    /// Статический класс, ответственный за генерацию кода во время компиляции.
    /// </summary>
    public static class CodeGenerator
    {
        private static List<string> code = new List<string>();
        private static int countLabels = 0;

        public static void AddLabel()
        {
            countLabels++;
        }

        public static string GetCurrentLabel()
        {
            return "label" + countLabels.ToString();
        }

        /// <summary>
        /// Добавляет инструкцию в сгенерированный код.
        /// </summary>
        /// <param name="instruction">Инструкция для добавления.</param>
        public static void AddInstruction(string instruction)
        {
            code.Add(instruction);
        }

        /// <summary>
        /// Объявляет сегмент данных в сгенерированном коде.
        /// </summary>
        public static void DeclareDataSegment()
        {
            AddInstruction("data segment");
        }

        /// <summary>
        /// Объявляет сегменты стека и кода в сгенерированном коде.
        /// </summary>
        public static void DeclareStackAndCodeSegments()
        {
            AddInstruction("PRINT_BUF DB ' ' DUP(10)");
            AddInstruction("BUFEND    DB '$'");
            AddInstruction("data ends");
            AddInstruction("stk segment stack");
            AddInstruction("db 256 dup (\"?\")");
            AddInstruction("stk ends");
            AddInstruction("code segment");
            AddInstruction("assume cs:code,ds:data,ss:stk");
            AddInstruction("start:");
            AddInstruction("main proc");
            AddInstruction("mov ax,data");
            AddInstruction("mov ds,ax");
        }

        /// <summary>
        /// Объявляет конец основной процедуры.
        /// </summary>
        public static void DeclareMainProcedureEnd()
        {
            AddInstruction("mov ax,4c00h");
            AddInstruction("int 21h");
            AddInstruction("main endp");
        }

        /// <summary>
        /// Объявляет конец сгенерированного кода.
        /// </summary>
        public static void DeclareEndOfCode()
        {
            AddInstruction("code ends");
            AddInstruction("end main");
        }

        /// <summary>
        /// Объявляет процедуру печати в сгенерированном коде.
        /// </summary>
        public static void DeclarePrintProcedure()
        {
            AddInstruction("PRINT PROC NEAR");
            AddInstruction("MOV CX, 10");
            AddInstruction("MOV DI, BUFEND - PRINT_BUF");
            AddInstruction("PRINT_LOOP:");
            AddInstruction("MOV DX, 0");
            AddInstruction("DIV CX");
            AddInstruction("ADD DL, '0'");
            AddInstruction("MOV [PRINT_BUF + DI - 1], DL");
            AddInstruction("DEC DI");
            AddInstruction("CMP AL, 0");
            AddInstruction("JNE PRINT_LOOP");
            AddInstruction("LEA DX, PRINT_BUF");
            AddInstruction("ADD DX, DI");
            AddInstruction("MOV AH, 09H");
            AddInstruction("INT 21H");
            AddInstruction("RET");
            AddInstruction("PRINT ENDP");
        }
        /// <summary>
        /// Объявляет процедуру печати в сгенерированном коде.
        /// </summary>
        public static void DeclarePrintSpaceProcedure()
        {
            AddInstruction("PRINT_SPACE:");
            AddInstruction("ADD DL, ' '");
            AddInstruction("MOV AH, 2");
            AddInstruction("INT 21H");
            AddInstruction("RET");
        }

        /// <summary>
        /// Объявляет переменные в сегменте данных на основе таблицы имен.
        /// </summary>
        /// <param name="nameTable">Таблица имен, содержащая идентификаторы.</param>
        public static void DeclareVariables(NameTable nameTable)
        {
            nameTable.GetIdentifiers().ForEach(
            node =>
                {
                    AddInstruction($"{node.Name}  dw    1");
                });
        }

        /// <summary>
        /// Генерация инструкций импликации
        /// </summary>
        public static void AddImplicationInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("not ax");
            AddInstruction("and ax, 1"); // Допускаем значения только 0 или 1
            AddInstruction("or ax, bx");
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций дизъюнкции
        /// </summary>
        public static void AddDisjunctionInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("or ax, bx");
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций конъюнкции
        /// </summary>
        public static void AddConjunctionInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("and ax, bx");
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций отрицания
        /// </summary>
        public static void AddNegationInstruction()
        {
            AddInstruction("pop ax");
            AddInstruction("not ax");
            AddInstruction("and ax, 1"); // Допускаем значения только 0 или 1
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций добавления в регистр значения лжи
        /// </summary>
        public static void AddExtractFalseInstruction()
        {
            AddInstruction("mov ax, " + 0);
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций добавления в регистр значения правды
        /// </summary>
        public static void AddExtractTrueInstruction()
        {
            AddInstruction("mov ax, " + 1);
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций добавления в регистр значения из текущей переменной
        /// </summary>
        public static void AddExtractValueInstruction()
        {
            AddInstruction("mov ax, " + LexicalAnalyzer.CurrentName);
            AddInstruction("push ax");
        }

        /// <summary>
        /// Генерация инструкций добавления в регистр целого числа
        /// </summary>
        /// <param name="currentName"></param>
        public static void AddLoadIntegerInstruction(string? currentName)
        {
            AddInstruction($"mov ax, {currentName}");
            AddInstruction("push ax");
        }
        /// <summary>
        /// Получает сгенерированный код в виде массива строк.
        /// </summary>
        /// <returns>Массив строк с сгенерированным кодом.</returns>
        public static string[] GetGeneratedCode()
        {
            return code.ToArray();
        }

        public static void Initialize()
        {
            code.Clear();
        }

        public static void AddSumInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("add ax, bx");
            AddInstruction("push ax");
        }

        public static void AddSubtractInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("sub ax, bx");
            AddInstruction("push ax");
        }

        public static void AddMultiplicationInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("mul bx");
            AddInstruction("push ax");
        }

        public static void AddDivisionInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("xor dx, dx");
            AddInstruction("div bx");
            AddInstruction("mov ax,al");
            AddInstruction("push ax");
        }

        public static void AddRemainderInstruction()
        {
            AddInstruction("pop bx");
            AddInstruction("pop ax");
            AddInstruction("xor dx, dx");
            AddInstruction("div bx");
            AddInstruction("mov ax,dx");
            AddInstruction("push ax");
        }
    }
}
