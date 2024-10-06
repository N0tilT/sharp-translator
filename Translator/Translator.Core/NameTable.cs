﻿/// <summary>
/// Перечисление, представляющее категории идентификаторов.
/// </summary>
public enum tCat
{
    Const,
    Var,
    Type
}

/// <summary>
/// Перечисление, представляющее типы идентификаторов.
/// </summary>
public enum tType
{
    None,
    Int,
    Bool
}

/// <summary>
/// Структура, представляющая идентификатор с именем, типом и категорией.
/// </summary>
public struct Identifier
{
    public string Name;
    public tType Type;
    public tCat Category;

    /// <summary>
    /// Конструктор структуры Identifier.
    /// </summary>
    /// <param name="name">Имя идентификатора.</param>
    /// <param name="type">Тип идентификатора.</param>
    /// <param name="category">Категория идентификатора.</param>
    public Identifier(string name, tType type, tCat category)
    {
        Name = name;
        Type = type;
        Category = category;
    }
}

/// <summary>
/// Класс, представляющий таблицу имен, которая хранит идентификаторы.
/// </summary>
public class NameTable
{
    private List<Identifier> identifiers;

    /// <summary>
    /// Конструктор, который инициализирует таблицу имен.
    /// </summary>
    public NameTable()
    {
        identifiers = new List<Identifier>();
    }

    /// <summary>
    /// Добавляет новый идентификатор в таблицу имен.
    /// </summary>
    /// <param name="name">Имя идентификатора.</param>
    /// <param name="category">Категория идентификатора.</param>
    /// <param name="type">Тип идентификатора.</param>
    /// <returns>Добавленный идентификатор.</returns>
    /// <exception cref="Exception">Выбрасывается, если идентификатор с таким же именем уже существует.</exception>
    public Identifier AddIdentifier(string name, tCat category, tType type)
    {
        if (FindByName(name).Equals(default(Identifier)))
        {
            Identifier identifier = new Identifier
            {
                Name = name,
                Category = category,
                Type = type
            };

            identifiers.Add(identifier);
            return identifier;
        }
        else
        {
            throw new Exception($"Ошибка: Идентификатор с именем '{name}' уже существует.");
        }
    }

    /// <summary>
    /// Находит идентификатор по его имени.
    /// </summary>
    /// <param name="name">Имя идентификатора.</param>
    /// <returns>Найденный идентификатор или значение по умолчанию, если не найден.</returns>
    public Identifier FindByName(string name)
    {
        foreach (var id in identifiers)
        {
            if (id.Name == name)
            {
                return id;
            }
        }
        return default;
    }

    /// <summary>
    /// Получает список идентификаторов, хранящихся в таблице имен.
    /// </summary>
    /// <returns>Список идентификаторов.</returns>
    public List<Identifier> GetIdentifiers() => identifiers;

    /// <summary>
    /// Инициализирует таблицу имен, очищая все идентификаторы.
    /// </summary>
    public void Initialize()
    {
        identifiers = new List<Identifier>();
    }
}
