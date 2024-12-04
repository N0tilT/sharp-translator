


namespace Translator.Core
{
    /// <summary>
    /// Класс, ответственный за синтаксический анализ и компиляцию исходного кода.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private NameTable nameTable = new NameTable();
        private string currentLabel;

        /// <summary>
        /// Компилирует исходный код из указанного файла.
        /// </summary>
        /// <param name="code">Исходный код.</param>
        public void Compile(string code)
        {
            LexicalAnalyzer.Initialize(code);
            CodeGenerator.Initialize();
            CodeGenerator.DeclareDataSegment();
             
            LexicalAnalyzer.ParseNextLexem();
            ParseVariableDeclaration();
            CodeGenerator.DeclareVariables(nameTable);

            CodeGenerator.DeclareStackAndCodeSegments();
            CheckLexem(Lexems.Semi);
            CheckLexem(Lexems.Begin);

            ParseInstructionSequence();

            CheckLexem(Lexems.End);
            CheckLexem(Lexems.Semi);

            ParsePrintInstruction();
            CodeGenerator.DeclareMainProcedureEnd();
            CodeGenerator.DeclarePrintProcedure();
            //CodeGenerator.DeclarePrintSpaceProcedure();
            CodeGenerator.DeclareEndOfCode();
        }

        /// <summary>
        /// Парсит инструкцию печати из исходного кода.
        /// </summary>
        private void ParsePrintInstruction()
        {
            CheckLexem(Lexems.Print);
            while (true)
            {
                if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
                {
                    Identifier x = nameTable.FindByName(LexicalAnalyzer.CurrentName);
                    CodeGenerator.AddInstruction("mov ax, " + LexicalAnalyzer.CurrentName);
                    CodeGenerator.AddInstruction("push ax");
                    CodeGenerator.AddInstruction("CALL PRINT");
                    //CodeGenerator.AddInstruction("CALL PRINT_SPACE");
                    CodeGenerator.AddInstruction("pop ax");
                    LexicalAnalyzer.ParseNextLexem();
                }
                else if (LexicalAnalyzer.CurrentLexem == Lexems.Semi) break;
                else if (LexicalAnalyzer.CurrentLexem == Lexems.Comma) LexicalAnalyzer.ParseNextLexem();
                else Error();
            }
        }

        /// <summary>
        /// Парсит объявления переменных из исходного кода.
        /// </summary>
        private void ParseVariableDeclaration()
        {
            CheckLexem(Lexems.Var);

            List<string> variables = new List<string>();

            while (true)
            {
                if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
                {
                    string variableName = LexicalAnalyzer.CurrentName;
                    LexicalAnalyzer.ParseNextLexem();

                    if (LexicalAnalyzer.CurrentLexem == Lexems.Colon)
                    {
                        LexicalAnalyzer.ParseNextLexem();

                        if (LexicalAnalyzer.CurrentLexem == Lexems.Logical)
                        {
                            variables.Add(variableName);
                            variables.ForEach(variable => nameTable.AddIdentifier(variable, tCat.Var, tType.Bool));
                            variables.Clear();
                            LexicalAnalyzer.ParseNextLexem();
                        }
                        else if (LexicalAnalyzer.CurrentLexem == Lexems.Integer)
                        {
                            variables.Add(variableName);
                            variables.ForEach(variable => nameTable.AddIdentifier(variable, tCat.Var, tType.Int));
                            variables.Clear();
                            LexicalAnalyzer.ParseNextLexem();
                        }
                        else
                        {
                            Error();
                        }
                    }
                    else if (LexicalAnalyzer.CurrentLexem == Lexems.Comma)
                    {
                        variables.Add(variableName);
                        LexicalAnalyzer.ParseNextLexem();
                    }
                    else
                    {
                        Error();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Парсит последовательность инструкций.
        /// </summary>
        private void ParseInstructionSequence()
        {
            ParseInstruction();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Semi)
            {
                LexicalAnalyzer.ParseNextLexem();
                ParseInstruction();
            }
        }

        /// <summary>
        /// Парсит одну инструкцию.
        /// </summary>
        private void ParseInstruction()
        {
            if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
            {
                Identifier x = nameTable.FindByName(LexicalAnalyzer.CurrentName);
                if (!x.Equals(default(Identifier)))
                {
                    ParseAssignmentInstruction();
                    CodeGenerator.AddInstruction("pop ax");
                    CodeGenerator.AddInstruction("mov " + x.Name + ", ax");
                }
                else
                {
                    Error();
                }
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.If)
            {
                ParseBranching();
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.While)
            {
                ParseLoop();
            }
        }
        /// <summary>
        /// Парсит инструкцию ветвления.
        /// </summary>
        private void ParseBranching()
        {
            CheckLexem(Lexems.If);

            CodeGenerator.AddLabel();
            string lowerLabel = CodeGenerator.GetCurrentLabel();
            currentLabel = lowerLabel;
            CodeGenerator.AddLabel();
            string exitLabel = CodeGenerator.GetCurrentLabel();
            bool onlyIf = true;

            ParseExpression();
            CheckLexem(Lexems.Then);
            ParseInstructionSequence();
            CodeGenerator.AddInstruction("jmp " + exitLabel);

            while (LexicalAnalyzer.CurrentLexem == Lexems.ElseIf)
            {
                onlyIf = false;
                CodeGenerator.AddInstruction(lowerLabel + ":");
                CodeGenerator.AddLabel();
                lowerLabel = CodeGenerator.GetCurrentLabel();
                currentLabel = lowerLabel;

                LexicalAnalyzer.ParseNextLexem();
                ParseExpression();
                CheckLexem(Lexems.Then);
                ParseInstructionSequence();
                CodeGenerator.AddInstruction("jmp " + exitLabel);
            }

            if (LexicalAnalyzer.CurrentLexem == Lexems.Else)
            {
                onlyIf = false;
                CodeGenerator.AddInstruction(lowerLabel + ":");
                LexicalAnalyzer.ParseNextLexem();
                ParseInstructionSequence();
            }
            if (onlyIf)
            {
                CodeGenerator.AddInstruction(lowerLabel + ":");
            }
            CheckLexem(Lexems.EndIf);
            CodeGenerator.AddInstruction(exitLabel + ":");
        }

        /// <summary>
        /// Парсит инструкцию цикла.
        /// </summary>
        private void ParseLoop()
        {
            CheckLexem(Lexems.While);
            CodeGenerator.AddLabel();
            string upperLabel = CodeGenerator.GetCurrentLabel();
            CodeGenerator.AddLabel();
            string lowerLabel = CodeGenerator.GetCurrentLabel();
            currentLabel = lowerLabel;
            CodeGenerator.AddInstruction(upperLabel + ":");
            ParseExpression();
            CheckLexem(Lexems.Do);
            ParseInstructionSequence();
            CheckLexem(Lexems.EndWhile);
            CodeGenerator.AddInstruction("jmp " + upperLabel);
            CodeGenerator.AddInstruction(lowerLabel + ":");
        }


        /// <summary>
        /// Парсит инструкцию присваивания.
        /// </summary>
        private void ParseAssignmentInstruction()
        {
            LexicalAnalyzer.ParseNextLexem();
            if (LexicalAnalyzer.CurrentLexem == Lexems.Assign)
            {
                LexicalAnalyzer.ParseNextLexem();
                ParseExpression();

            }
            else
            {
                Error();
            }
        }

        /// <summary>
        /// Парсит выражение
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseExpression()
        {
            tType t = ParseImplication();

            while (LexicalAnalyzer.CurrentLexem == Lexems.Equal ||
                   LexicalAnalyzer.CurrentLexem == Lexems.NotEqual ||
                   LexicalAnalyzer.CurrentLexem == Lexems.Less ||
                   LexicalAnalyzer.CurrentLexem == Lexems.Greater ||
                   LexicalAnalyzer.CurrentLexem == Lexems.LessOrEqual ||
                   LexicalAnalyzer.CurrentLexem == Lexems.GreaterOrEqual)
            {
                string jump = "";
                switch (LexicalAnalyzer.CurrentLexem)
                {
                    case Lexems.Equal:
                        jump = "jne";
                        break;
                    case Lexems.NotEqual:
                        jump = "je";
                        break;
                    case Lexems.Greater:
                        jump = "jle";
                        break;
                    case Lexems.GreaterOrEqual:
                        jump = "jl";
                        break;
                    case Lexems.Less:
                        jump = "jge";
                        break;
                    case Lexems.LessOrEqual:
                        jump = "jg";
                        break;
                }
                LexicalAnalyzer.ParseNextLexem();
                ParseSumOrSubtraction();
                CodeGenerator.AddInstruction("pop ax");
                CodeGenerator.AddInstruction("pop bx");
                CodeGenerator.AddInstruction("cmp bx, ax");
                CodeGenerator.AddInstruction(jump + " " + currentLabel);
                currentLabel = "";
                t = tType.Bool;
            }
            return t;
        }
       
        /// <summary>
        /// Парсит импликацию
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseImplication()
        {
            tType type = ParseDisjunction();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Implication)
            {
                Lexems operatorLexem = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType s_type = ParseDisjunction();
                if (type == tType.Bool && s_type == tType.Bool)
                    CodeGenerator.AddImplicationInstruction();
                else
                    Error();
            }
            return type;
        }

        /// <summary>
        /// Парсит дизъюнкцию
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseDisjunction()
        {
            tType type = ParseConjunction();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Disjunction)
            {
                Lexems operatorLexem = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType s_type = ParseConjunction();
                if (type == tType.Bool && s_type == tType.Bool)
                    CodeGenerator.AddDisjunctionInstruction();
                else
                    Error();
            }
            return type;
        }

        /// <summary>
        /// Парсит конъюнкцию
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseConjunction()
        {
            tType type = ParseSumOrSubtraction();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Conjunction)
            {
                Lexems operatorLexem = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType s_type = ParseSumOrSubtraction();
                if (type == tType.Bool && s_type == tType.Bool)
                    CodeGenerator.AddConjunctionInstruction();
                else
                    Error();
            }
            return type;
        }

        /// <summary>
        /// Парсит сумму или вычитание
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseSumOrSubtraction()
        {
            tType type = ParseMultiplicationOrDivisionOrRemainder();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Sum || LexicalAnalyzer.CurrentLexem == Lexems.Subtract)
            {
                Lexems operatorLexem = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType s_type = ParseMultiplicationOrDivisionOrRemainder();
                if (type == s_type && type == tType.Int)
                    if (operatorLexem == Lexems.Sum)
                        CodeGenerator.AddSumInstruction();
                    else if (operatorLexem == Lexems.Subtract)
                        CodeGenerator.AddSubtractInstruction();
                    else
                        Error();
                else
                    Error();
            }
            return type;
        }


        /// <summary>
        /// Парсит Умножение или деление
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseMultiplicationOrDivisionOrRemainder()
        {
            tType type = ParseSubexpression();
            while (LexicalAnalyzer.CurrentLexem == Lexems.Multiplication || LexicalAnalyzer.CurrentLexem == Lexems.Division || LexicalAnalyzer.CurrentLexem == Lexems.Remainder)
            {
                Lexems operatorLexem = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType s_type = ParseSubexpression();

                if (type == s_type && type == tType.Int)
                    if (operatorLexem == Lexems.Multiplication)
                        CodeGenerator.AddMultiplicationInstruction();
                    else if (operatorLexem == Lexems.Division)
                        CodeGenerator.AddDivisionInstruction();
                    else if (operatorLexem == Lexems.Remainder)
                        CodeGenerator.AddRemainderInstruction();
                    else
                        Error();
                else
                    Error();
            }
            return type;
        }

        /// <summary>
        /// Парсит подвыражения - операция отрицания, переменные, константы и скобки
        /// </summary>
        /// <returns>Тип операции</returns>
        private tType ParseSubexpression()
        {
            if (LexicalAnalyzer.CurrentLexem == Lexems.Negation)
            {
                LexicalAnalyzer.ParseNextLexem();
                tType type = ParseSubexpression();

                if (type == tType.Bool)
                {
                    CodeGenerator.AddNegationInstruction();
                    return tType.Bool;
                }
                else
                {
                    Error();
                }
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
            {
                Identifier x = nameTable.FindByName(LexicalAnalyzer.CurrentName);
                if (!x.Equals(default(Identifier)) && x.Category == tCat.Var)
                {
                    CodeGenerator.AddExtractValueInstruction();
                    LexicalAnalyzer.ParseNextLexem();
                    return x.Type;
                }
                else
                {
                    Error();
                }
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.True)
            {
                CodeGenerator.AddExtractTrueInstruction();
                LexicalAnalyzer.ParseNextLexem();
                return tType.Bool;
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.False)
            {
                CodeGenerator.AddExtractFalseInstruction();
                LexicalAnalyzer.ParseNextLexem();
                return tType.Bool;
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.Integer)
            {
                CodeGenerator.AddLoadIntegerInstruction(LexicalAnalyzer.CurrentName);
                LexicalAnalyzer.ParseNextLexem();
                return tType.Int;
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.LeftBracket)
            {
                LexicalAnalyzer.ParseNextLexem();
                tType type = ParseExpression();
                CheckLexem(Lexems.RightBracket);
                return type;
            }
            else
            {
                Error();
            }
            return tType.None;
        }

        /// <summary>
        /// Проверяет, совпадает ли текущая лексема с ожидаемой лексемой. 
        /// Если нет, вызывает метод Error().
        /// </summary>
        /// <param name="expectedLexem">Ожидаемая лексема для проверки.</param>
        private void CheckLexem(Lexems expectedLexem)
        {
            if (LexicalAnalyzer.CurrentLexem != expectedLexem)
            {
                Error();
            }
            LexicalAnalyzer.ParseNextLexem();
        }

        /// <summary>
        /// Обрабатывает ошибки в процессе синтаксического анализа, выводя детали ошибки.
        /// </summary>
        private void Error()
        {
            throw new Exception(
                $"Ошибка в строке {Reader.LineNumber}, позиция {Reader.CharacterPositionInLine}: " +
                $"Неверная лексема: {LexicalAnalyzer.CurrentLexem}");
        }
    }
}
